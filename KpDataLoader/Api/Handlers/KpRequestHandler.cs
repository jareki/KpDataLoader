using KpDataLoader.Api.Http;
using KpDataLoader.Api.Models.Repsonses;
using KpDataLoader.Api.Models.Requests;

namespace KpDataLoader.Api.Handlers
{
    public class KpRequestHandler<TRequest,TResponse>: JsonGetRequestHandler<TRequest,TResponse>
    where TRequest: IRequestModel
    where TResponse : IResponseModel, new()
    {
        public KpRequestHandler(
            IHttpClientService httpClientService,
            string method) : base(httpClientService, method)
        {
        }

        protected override async ValueTask<TResponse> HandleErrorAsync(HttpResponseMessage response, CancellationToken ct = default)
        {
            return await this.HandleResponseAsync(response, ct);
        }

        protected override ValueTask<TResponse> HandleErrorAsync(Exception ex, CancellationToken ct = default)
        {
            return ValueTask.FromResult(
                new TResponse()
                {
                    Error = ex.GetType().ToString(),
                    Message = ex.Message,
                    StatusCode = 500
                });
        }
    }
}
