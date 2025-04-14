using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using KpDataLoader.Api.Http;
using KpDataLoader.Api.Models.Repsonses;
using KpDataLoader.Api.Models.Requests;

namespace KpDataLoader.Api.Handlers
{
    public abstract class JsonGetRequestHandler<TRequest, TResponse> : BaseGetRequestHandler<TRequest, TResponse>
    where TRequest : IRequestModel
    where TResponse : IResponseModel
    {
        public JsonGetRequestHandler(IHttpClientService httpClientService) : base(httpClientService)
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
