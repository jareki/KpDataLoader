using KpDataLoader.Db;
using KpDataLoader.Http;
using KpDataLoader.Settings;
using KpDataLoader.Workers;
using Microsoft.Extensions.DependencyInjection;

namespace KpDataLoader
{
    public class Program
    {
        private static CancellationTokenSource cts = new CancellationTokenSource();

        static async Task Main(string[] args)
        {
            Console.CancelKeyPress += OnCancelKeyPress;
            AppDomain.CurrentDomain.ProcessExit += OnProcessExit;

            try
            {
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
                        .AddSingleton(new DataService(settings.DbPath))
                        .AddSingleton<MainWorker>()
                        .BuildServiceProvider();



                var mainWorker = serviceProvider.GetRequiredService<MainWorker>();
                await mainWorker.RunAsync(cts.Token);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine($"Работа была остановлена");
            }

        }

        private static void OnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            // Предотвращаем немедленное завершение процесса
            e.Cancel = true;

            Console.WriteLine("Получен сигнал Ctrl+C");
            cts.Cancel();
        }

        private static void OnProcessExit(object sender, EventArgs e)
        {
            Console.WriteLine("Получен сигнал SIGTERM");
            cts.Cancel();

            // Даем время для выполнения очистки (Docker дает около 10 секунд перед отправкой SIGKILL)
            Thread.Sleep(2000);
        }
    }
}