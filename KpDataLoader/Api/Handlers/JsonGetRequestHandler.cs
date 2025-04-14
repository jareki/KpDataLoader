using System.Text.Json;
using KpDataLoader.Api.Http;
using KpDataLoader.Api.Models.Repsonses;
using KpDataLoader.Api.Models.Requests;

namespace KpDataLoader.Api.Handlers
{
    public abstract class JsonGetRequestHandler<TRequest, TResponse> : BaseGetRequestHandler<TRequest, TResponse>
    where TRequest : IRequestModel
    where TResponse : IResponseModel
    {
        public JsonGetRequestHandler(
            IHttpClientService httpClientService,
            string method) : base(httpClientService, method)
        {
        }

        protected override async Task<TResponse> HandleResponseAsync(HttpResponseMessage responseMessage, CancellationToken ct = default)
        {
            string jsonContent = await responseMessage.Content.ReadAsStringAsync(ct);
            return JsonSerializer.Deserialize<TResponse>(
                jsonContent,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
        }
    }
}
