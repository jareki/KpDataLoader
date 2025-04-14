namespace KpDataLoader.Settings
{
    /// <summary>
    /// Класс для хранения настроек приложения
    /// </summary>
    public class AppSettings
    {
        public string DbPath { get; set; }
        public ApiSettings Api { get; set; }
        public ProbabilitySettings Probabilities { get; set; }
        public Search Search { get; set; }
    }

    public class ApiSettings
    {
        public string BaseAddress { get; set; }
        public string Key { get; set; }
        public int MaxRetries { get; set; }
        public int RetryDelaySec { get; set; }
        public int TimeoutSec { get; set; }
    }

    public class ProbabilitySettings
    {
        public double LoadMovie { get; set; }
        public double UpdateMovie { get; set; }
        public double UpdateImages { get; set; }
    }
    public class Search
    {
        public int? MinYear { get; set; }
        public int? MaxYear { get; set; }
        public double? MinRatingKp { get; set; }
        public double? MaxRatingKp { get; set; }
        public double? MinRatingImdb { get; set; }
        public double? MaxRatingImdb { get; set; }
        public int? MinVotesKp { get; set; }
        public int? MaxVotesKp { get; set; }
        public int? MinVotesImdb { get; set; }
        public int? MaxVotesImdb { get; set; }
    }
}
