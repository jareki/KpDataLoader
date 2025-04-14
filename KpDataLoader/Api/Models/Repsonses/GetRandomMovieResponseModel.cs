using System.Text.Json.Serialization;

namespace KpDataLoader.Api.Models.Repsonses
{
    public class GetRandomMovieResponseModel
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("enName")]
        public string EnName { get; set; }

        [JsonPropertyName("typeNumber")]
        public int TypeNumber { get; set; }

        [JsonPropertyName("year")]
        public int Year { get; set; }

        [JsonPropertyName("rating")]
        public Rating Rating { get; set; }

        [JsonPropertyName("votes")]
        public Votes Votes { get; set; }
    }

    public class Rating
    {
        [JsonPropertyName("kp")]
        public double Kp { get; set; }

        [JsonPropertyName("imdb")]
        public double Imdb { get; set; }
    }

    public class Votes
    {
        [JsonPropertyName("kp")]
        public int Kp { get; set; }

        [JsonPropertyName("imdb")]
        public int Imdb { get; set; }
    }
}
