namespace KpDataLoader.Http
{
    public class HttpClientServiceOptions
    {
        public string BaseAddress { get; set; }
        public string ApiKey { get; set; }
        public string ApiKeyHeaderName { get; set; } = "X-Api-Key";

        // Настройки таймаутов
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

        // Настройки политики повторов
        public int MaxRetryAttempts { get; set; } = 3;
        public TimeSpan RetryInitialDelay { get; set; } = TimeSpan.FromSeconds(1);
    }
}
