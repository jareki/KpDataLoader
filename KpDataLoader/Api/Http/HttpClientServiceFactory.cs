namespace KpDataLoader.Api.Http
{
    // Фабрика для создания HTTP-клиентов
    public class HttpClientServiceFactory
    {
        private static readonly HttpClientServiceFactory _instance = new HttpClientServiceFactory();
        public static HttpClientServiceFactory Instance => _instance;

        private HttpClientServiceFactory() { }

        public IHttpClientService CreateHttpClient(Action<HttpClientServiceOptions> configureOptions)
        {
            var options = new HttpClientServiceOptions();
            configureOptions?.Invoke(options);
            return new HttpClientService(options);
        }

        public void Dispose()
        {
            HttpClientPool.Instance.Dispose();
        }
    }
}
