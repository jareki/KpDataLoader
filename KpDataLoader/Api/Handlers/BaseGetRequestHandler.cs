using System.Text.Json;
using KpDataLoader.Api.Http;
using KpDataLoader.Api.Models.Repsonses;
using KpDataLoader.Api.Models.Requests;

namespace KpDataLoader.Api.Handlers
{
    public abstract class BaseGetRequestHandler<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
        where TRequest : IRequestModel
        where TResponse : IResponseModel
    {
        private readonly IHttpClientService _httpClientService;

        protected BaseGetRequestHandler(IHttpClientService httpClientService)
        {
            this._httpClientService = httpClientService;
        }

        public async Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                string requestUri = BuildRequestUri(request);
                HttpResponseMessage response = await _httpClientService.GetAsync(requestUri, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    return HandleError(response);
                }

                // Десериализуем JSON в объект ответа
                TResponse? result = await this.HandleResponseAsync(response, cancellationToken);

                return result;
            }
            catch (HttpRequestException ex)
            {
                // Обрабатываем ошибки сетевого соединения
                return HandleError(ex);
            }
            catch (JsonException ex)
            {
                // Обрабатываем ошибки десериализации
                return HandleError(ex);
            }
            catch (Exception ex)
            {
                // Обрабатываем другие исключения
                return HandleError(ex);
            }
        }

        /// <summary>
        /// Обработка ответа
        /// </summary>
        protected abstract Task<TResponse> HandleResponseAsync(HttpResponseMessage responseMessage, CancellationToken ct = default);

        /// <summary>
        /// для формирования URI запроса с параметрами
        /// </summary>
        /// <param name="request">Объект запроса</param>
        /// <returns>URI запроса с параметрами</returns>
        protected abstract string BuildRequestUri(TRequest request);

        /// <summary>
        /// Обработка ошибочных http-кодов ответа
        /// </summary>
        /// <param name="response">ответ</param>
        /// <param name="jsonContent">json-контент ответа</param>
        /// <returns></returns>
        protected abstract TResponse HandleError(HttpResponseMessage response);

        protected abstract TResponse HandleError(Exception ex);
    }
}
