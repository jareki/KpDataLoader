namespace KpDataLoader.Http;

public interface IHttpClientService
{
    Task<HttpResponseMessage> GetAsync(string url, CancellationToken cancellationToken = default);
    Task<HttpResponseMessage> PostAsync<T>(string url, T content, CancellationToken cancellationToken = default);
}