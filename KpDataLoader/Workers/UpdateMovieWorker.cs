using KpDataLoader.Api.Handlers;
using KpDataLoader.Api.Models.Repsonses;
using KpDataLoader.Api.Models.Requests;
using KpDataLoader.Db;
using KpDataLoader.Db.Models;
using KpDataLoader.Settings;

namespace KpDataLoader.Workers
{
    public class UpdateMovieWorker : IWorker
    {
        private readonly ISettingsService settingsService;
        private readonly DataService dataService;
        private readonly IRequestHandler<GetRandomMovieRequestModel, GetRandomMovieResponseModel> randomMovieRequestHandler;

        public UpdateMovieWorker(
            ISettingsService settingsService, 
            DataService dataService, 
            IRequestHandler<GetRandomMovieRequestModel, GetRandomMovieResponseModel> randomMovieRequestHandler)
        {
            this.settingsService = settingsService;
            this.dataService = dataService;
            this.randomMovieRequestHandler = randomMovieRequestHandler;
        }

        public async Task<bool> RunAsync(CancellationToken ct = default)
        {
            // получить самый старый фильм из базы
            var oldestMovie = await this.dataService.GetOldestUpdatedMovie();

            // загрузить фильм из апи
            var requestModel = new GetRandomMovieRequestModel()
            {
                Id = oldestMovie.KpId
            };
            var responseModel = await this.randomMovieRequestHandler.HandleAsync(requestModel, ct);
            if (!string.IsNullOrEmpty(responseModel.Error))
            {
                return false;
            }

            int id = responseModel.Id;

            // проверить, есть ли фильм в базе
            var existingMovie = await this.dataService.GetMovieByIdAsync(id);
            if (existingMovie != null)
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

            return false;
        }
    }
}
