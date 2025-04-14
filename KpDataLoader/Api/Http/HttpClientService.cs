using System.Text;
using System.Text.Json;

namespace KpDataLoader.Api.Http
{
    public class HttpClientService : IHttpClientService
    {
        private readonly HttpClient _httpClient;
        private readonly HttpClientServiceOptions _options;
        private readonly string _clientKey;

        public HttpClientService(HttpClientServiceOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _clientKey = string.IsNullOrEmpty(options.BaseAddress) ? Guid.NewGuid().ToString() : options.BaseAddress;

            // Получаем HttpClient из пула или создаем новый
            _httpClient = HttpClientPool.Instance.GetOrCreateClient(_clientKey, options.Timeout);

            // Добавляем API ключ в заголовки, если он указан
            if (!string.IsNullOrEmpty(options.ApiKey) &&
                !_httpClient.DefaultRequestHeaders.Contains(options.ApiKeyHeaderName))
            {
                _httpClient.DefaultRequestHeaders.Add(options.ApiKeyHeaderName, options.ApiKey);
            }
        }

        public async Task<HttpResponseMessage> GetAsync(string url, CancellationToken cancellationToken = default)
        {
            return await SendWithRetryAsync(ct => _httpClient.GetAsync(url, ct), cancellationToken);
        }

        public async Task<HttpResponseMessage> PostAsync<T>(string url, T content, CancellationToken cancellationToken = default)
        {
            var jsonContent = CreateJsonContent(content);
            return await SendWithRetryAsync(ct => _httpClient.PostAsync(url, jsonContent, ct), cancellationToken);
        }
        
        private StringContent CreateJsonContent<T>(T content)
        {
            var json = JsonSerializer.Serialize(content);
            var stringContent = new StringContent(json, Encoding.UTF8, "application/json");
            return stringContent;
        }

        private async Task<HttpResponseMessage> SendWithRetryAsync(
            Func<CancellationToken, Task<HttpResponseMessage>> sendRequestAsync,
            CancellationToken cancellationToken)
        {
            int attempt = 0;
            TimeSpan delay = _options.RetryInitialDelay;

            while (true)
            {
                attempt++;

                // Создаем таймаут для запроса
                using var timeoutCts = new CancellationTokenSource(_options.Timeout);
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, cancellationToken);

                try
                {
                    var response = await sendRequestAsync(linkedCts.Token);

                    // Если успешный ответ или достигли максимального количества попыток,
                    // или это не ошибка сервера (не 5xx), возвращаем ответ
                    bool isServerError = (int)response.StatusCode >= 500 && (int)response.StatusCode <= 599;
                    if (response.IsSuccessStatusCode 
                        || attempt >= _options.MaxRetryAttempts 
                        || !isServerError)
                    {
                        return response;
                    }

                    // При ошибке сервера пробуем еще раз после задержки
                    await Task.Delay(delay, cancellationToken);
                    // Экспоненциальное увеличение задержки (backoff)
                    delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * 2);
                }
                catch (TaskCanceledException) when (timeoutCts.Token.IsCancellationRequested)
                {
                    // Произошел таймаут запроса
                    if (attempt >= _options.MaxRetryAttempts)
                    {
                        throw new TimeoutException($"Request timed out after {_options.Timeout.TotalSeconds} seconds and {attempt} attempts.");
                    }

                    // Увеличиваем задержку и пробуем еще раз
                    await Task.Delay(delay, cancellationToken);
                    delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * 2);
                }
                catch (HttpRequestException ex)
                {
                    // При сетевых ошибках тоже повторяем
                    if (attempt >= _options.MaxRetryAttempts)
                    {
                        throw new HttpRequestException($"Request failed after {attempt} attempts: {ex.Message}", ex);
                    }

                    await Task.Delay(delay, cancellationToken);
                    delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * 2);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    // Если отмена пришла извне, пробрасываем исключение
                    throw;
                }
                catch (Exception ex)
                {
                    // Другие ошибки не обрабатываем, просто перебрасываем
                    throw new HttpRequestException($"Request failed: {ex.Message}", ex);
                }
            }
        }

        public void Dispose()
        {
            // HttpClient не удаляем, так как он используется в пуле
            // Но можно удалить отдельные заголовки, если нужно
        }
    }
}
