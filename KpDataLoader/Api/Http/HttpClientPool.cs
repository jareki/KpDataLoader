using System.Collections.Concurrent;

namespace KpDataLoader.Api.Http
{
    /// <summary>
    /// Singleton класс для управления пулом HttpClient экземпляров
    /// </summary>
    public class HttpClientPool
    {
        private static readonly Lazy<HttpClientPool> instance = new Lazy<HttpClientPool>(() => new HttpClientPool());
        private readonly ConcurrentDictionary<string, HttpClient> _clients = new ConcurrentDictionary<string, HttpClient>();

        public static HttpClientPool Instance => instance.Value;

        private HttpClientPool() { }

        public HttpClient GetOrCreateClient(string baseAddress, TimeSpan timeout)
        {
            return this._clients.GetOrAdd(baseAddress, key =>
            {
                var client = new HttpClient
                {
                    Timeout = timeout
                };

                if (!string.IsNullOrEmpty(baseAddress))
                {
                    client.BaseAddress = new Uri(baseAddress);
                }

                return client;
            });
        }

        public void RemoveClient(string baseAddress)
        {
            if (this._clients.TryRemove(baseAddress, out var client))
            {
                client.Dispose();
            }
        }

        public void Dispose()
        {
            foreach (var client in this._clients.Values)
            {
                client.Dispose();
            }

            this._clients.Clear();
        }
    }
}
