namespace KpDataLoader.Api.Models.Requests
{
    public class GetRandomMovieRequestModel : IRequestModel
    {
        public int? Id { get; set; }
        public string Status { get; set; } = "completed";
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
