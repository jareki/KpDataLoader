using System.Text.Json.Serialization;

namespace KpDataLoader.Api.Models.Repsonses
{
    public class GetMovieImagesResponseModel: BaseResponseModel
    {
        [JsonPropertyName("docs")]
        public Doc Docs { get; set; }
    }

    public class Doc
    {
        [JsonPropertyName("movieId")]
        public int MovieId { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }
    }
}
