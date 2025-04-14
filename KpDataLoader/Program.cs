using KpDataLoader.Db;
using KpDataLoader.Http;
using KpDataLoader.Settings;
using KpDataLoader.Workers;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
var settings = services.AddSettingsService(Path.Combine(AppContext.BaseDirectory, "data", "settings.json"));

var serviceProvider =
services
        .AddHttpClientService(options =>
    {
        options.BaseAddress = settings.Api.BaseAddress;
        options.ApiKey = settings.Api.Key;
        options.Timeout = TimeSpan.FromSeconds(settings.Api.TimeoutSec);
        options.MaxRetryAttempts = settings.Api.MaxRetries;
        options.RetryInitialDelay = TimeSpan.FromSeconds(settings.Api.RetryDelaySec);
    })
     .AddProbabilityFactory<IWorker>(builder =>
     {
         builder
             .AddImplementation<LoadRandomMovieWorker>(nameof(LoadRandomMovieWorker), settings.Probabilities.LoadMovie)
             .AddImplementation<UpdateImagesWorker>(nameof(UpdateImagesWorker), settings.Probabilities.UpdateImages)
             .AddImplementation<UpdateMovieWorker>(nameof(UpdateMovieWorker), settings.Probabilities.UpdateMovie);
     })
     .AddSingleton<DataService>(new DataService(settings.DbPath))
    .BuildServiceProvider();

