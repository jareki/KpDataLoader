using System.Text.Json.Serialization;

namespace KpDataLoader.Api.Models.Requests
{
    public class GetMovieImagesRequestModel: IRequestModel
    {
        [JsonPropertyName("page")]
        public int Page { get; set; }

        [JsonPropertyName("limit")]
        public int Limit { get; set; }

        [JsonPropertyName("movieId")]
        public int MovieId { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; } = "frame";
    }
}
