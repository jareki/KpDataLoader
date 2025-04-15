using KpDataLoader.Api.Handlers;
using KpDataLoader.Api.Models.Repsonses;
using KpDataLoader.Api.Models.Requests;
using KpDataLoader.Db;
using KpDataLoader.Db.Models;
using KpDataLoader.Settings;

namespace KpDataLoader.Workers
{
    public class LoadRandomMovieWorker: IWorker
    {
        private readonly ISettingsService settingsService;
        private readonly DataService dataService;
        private readonly IRequestHandler<GetRandomMovieRequestModel, GetRandomMovieResponseModel> randomMovieRequestHandler;

        public LoadRandomMovieWorker(
            ISettingsService settingsService,
            IRequestHandler<GetRandomMovieRequestModel, GetRandomMovieResponseModel> randomMovieRequestHandler, 
            DataService dataService)
        {
            this.randomMovieRequestHandler = randomMovieRequestHandler;
            this.dataService = dataService;
            this.settingsService = settingsService;
        }

        public async Task<bool> RunAsync(CancellationToken ct = default)
        {
            // загрузить фильм из апи
            var search = this.settingsService.Settings.Search;
            var requestModel = new GetRandomMovieRequestModel()
            {
                MinRatingImdb = search.MinRatingImdb,
                MaxRatingImdb = search.MaxRatingImdb,

                MinRatingKp = search.MinRatingKp,
                MaxRatingKp = search.MaxRatingKp,

                MinVotesKp = search.MinVotesKp,
                MaxVotesKp = search.MaxVotesKp,

                MinVotesImdb = search.MinVotesImdb,
                MaxVotesImdb = search.MaxVotesKp,

                MinYear = search.MinYear,
                MaxYear = search.MaxYear
            };

            var responseModel = await this.randomMovieRequestHandler.HandleAsync(requestModel, ct);
            if (!string.IsNullOrEmpty(responseModel.Error))
            {
                return false;
            }

            int id = responseModel.Id;

            // проверить, есть ли фильм в базе
            var existingMovie = await this.dataService.GetMovieByIdAsync(id);
            if (existingMovie == null)
            {
                await this.dataService.AddMovieAsync(
                    new Movie()
                    {
                        KpId = responseModel.Id,
                        NameRu = responseModel.Name,
                        NameEn = responseModel.EnName,
                        RatingKp = responseModel.Rating.Kp,
                        RatingImdb = responseModel.Rating.Imdb,
                        VotesKp = responseModel.Votes.Kp,
                        VotesImdb = responseModel.Votes.Imdb,
                        Year = responseModel.Year,
                        TypeId = responseModel.TypeNumber

                    });

                return true;
            }
            else
            {
                existingMovie.NameRu = responseModel.Name;
                existingMovie.NameEn = responseModel.EnName;
                existingMovie.RatingImdb = responseModel.Rating.Imdb;
                existingMovie.RatingKp = responseModel.Rating.Kp;
                existingMovie.VotesImdb = responseModel.Votes.Imdb;
                existingMovie.VotesKp = responseModel.Votes.Kp;
                existingMovie.Year = responseModel.Year;
                existingMovie.TypeId = responseModel.TypeNumber;

                return await this.dataService.UpdateMovieAsync(existingMovie);
            }
        }
    }
}
