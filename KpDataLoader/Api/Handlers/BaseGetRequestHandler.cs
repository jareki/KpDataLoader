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
        private readonly string _method;

        protected BaseGetRequestHandler(
            IHttpClientService httpClientService, 
            string method)
        {
            this._httpClientService = httpClientService;
            this._method = method;
        }

        public async Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                string requestUri = this.BuildRequestUri(request);
                HttpResponseMessage response = await this._httpClientService.GetAsync(requestUri, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    return await this.HandleErrorAsync(response, cancellationToken);
                }

                // Десериализуем JSON в объект ответа
                TResponse? result = await this.HandleResponseAsync(response, cancellationToken);

                return result;
            }
            catch (HttpRequestException ex)
            {
                // Обрабатываем ошибки сетевого соединения
                return await this.HandleErrorAsync(ex, cancellationToken);
            }
            catch (JsonException ex)
            {
                // Обрабатываем ошибки десериализации
                return await this.HandleErrorAsync(ex, cancellationToken);
            }
            catch (Exception ex)
            {
                // Обрабатываем другие исключения
                return await this.HandleErrorAsync(ex, cancellationToken);
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
        protected virtual string BuildRequestUri(TRequest request)
        {
            if (request == null)
                return string.Empty;

            var properties = request.GetType().GetProperties()
                .Where(p => p.GetValue(request) != null)
                .ToList();

            var parameters = new Dictionary<string, string>();
            var processedProperties = new HashSet<string>();

            // Один проход по всем свойствам
            foreach (var prop in properties)
            {
                string name = prop.Name;

                // Пропускаем уже обработанные свойства
                if (processedProperties.Contains(name))
                    continue;
                string baseName = String.Empty;
                string minName = String.Empty;
                string maxName = String.Empty;

                if (name.StartsWith("min", StringComparison.OrdinalIgnoreCase)
                    || name.StartsWith("max", StringComparison.OrdinalIgnoreCase))
                {
                    baseName = name.Substring(3);
                    minName = "min" + baseName;
                    maxName = "max" + baseName;
                }

                // Проверяем, является ли свойство частью min/max пары
                if (!string.IsNullOrEmpty(baseName))
                {
                    var minProp = properties.FirstOrDefault(p =>
                        string.Equals(p.Name, minName, StringComparison.OrdinalIgnoreCase));
                    var maxProp = properties.FirstOrDefault(p =>
                        string.Equals(p.Name, maxName, StringComparison.OrdinalIgnoreCase));

                    if (maxProp != null)
                    {
                        var minValue = minProp?.GetValue(request)?.ToString();
                        var maxValue = maxProp.GetValue(request)?.ToString();

                        if (minValue != null && maxValue != null)
                        {
                            parameters[baseName] = $"{minValue}-{maxValue}";
                            processedProperties.Add(minName);
                            processedProperties.Add(maxName);
                            continue;
                        }
                    }
                }

                // Обычное свойство
                parameters[name] = prop.GetValue(request)?.ToString() ?? string.Empty;
                processedProperties.Add(name);
            }

            // Строим строку запроса
            var queryString =
                this._method
                + '?'
                + string.Join(
                    '&',
                    parameters.Select(p =>
                        $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));

            return queryString.ToString();
        }

        /// <summary>
        /// Обработка ошибочных http-кодов ответа
        /// </summary>
        /// <param name="response">ответ</param>
        /// <returns></returns>
        protected abstract ValueTask<TResponse> HandleErrorAsync(HttpResponseMessage response, CancellationToken ct = default);

        protected abstract ValueTask<TResponse> HandleErrorAsync(Exception ex, CancellationToken ct = default);
    }
}
