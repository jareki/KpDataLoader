using KpDataLoader.Api.Http;
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
    .BuildServiceProvider();
