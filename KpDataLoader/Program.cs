using KpDataLoader.Http;
using Microsoft.Extensions.DependencyInjection;

var serviceProvider = new ServiceCollection()
        .AddHttpClientService(options =>
    {
        options.BaseAddress = "https://api.example.com";
        options.ApiKey = "your-api-key";
        options.Timeout = TimeSpan.FromSeconds(60);
        options.MaxRetryAttempts = 3;
        options.RetryInitialDelay = TimeSpan.FromSeconds(1);
    })
    /* todo:
     .AddProbabilityFactory<IWorker>(builder => {
        builder.AddImplementation<EmailWorker>("Email", 0.2)
           .AddImplementation<SmsWorker>("SMS", 0.3)
           .AddImplementation<PushWorker>("Push", 0.5);
    })*/
    .BuildServiceProvider();
