using KpDataLoader.Api.Handlers;
using KpDataLoader.Api.Models.Repsonses;
using KpDataLoader.Api.Models.Requests;
using KpDataLoader.Db;

namespace KpDataLoader.Workers
{
    public class UpdateImagesWorker : IWorker
    {
        private readonly DataService dataService;
        private readonly IRequestHandler<GetMovieImagesRequestModel, GetMovieImagesResponseModel> requestHandler;

        public UpdateImagesWorker(
            DataService dataService, 
            IRequestHandler<GetMovieImagesRequestModel, GetMovieImagesResponseModel> requestHandler)
        {
            this.dataService = dataService;
            this.requestHandler = requestHandler;
        }

        public async Task<bool> RunAsync(CancellationToken ct = default)
        {
            // получить самый старый по изображениям фильм из базы
            var oldestMovie = await this.dataService.GetOldestImagesUpdatedMovie();

            // загрузить изображения из апи
            var requestModel =
                new GetMovieImagesRequestModel()
                {
                    Page = 1,
                    Limit = 10,
                    MovieId = oldestMovie.KpId
                };
            var responseModel = await this.requestHandler.HandleAsync(requestModel, ct);
            if (!string.IsNullOrEmpty(responseModel.Error))
            {
                return false;
            }

            // обновить фильм в базе
            var images = responseModel.Docs.Select(d => d.Url).ToList();
            await this.dataService.DeleteMovieImagesAsync(oldestMovie.KpId);
            await this.dataService.AddMovieImagesAsync(
                oldestMovie.KpId,
                images);
            return true;
        }
    }
}
