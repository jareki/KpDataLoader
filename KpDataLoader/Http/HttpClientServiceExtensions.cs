using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace KpDataLoader.Http
{
    // Расширения для IServiceCollection для регистрации в DI
    public static class HttpClientServiceExtensions
    {
        /// <summary>
        /// Регистрирует HttpClientService как синглтон в контейнере DI
        /// </summary>
        public static IServiceCollection AddHttpClientService(
            this IServiceCollection services,
            Action<HttpClientServiceOptions> configureOptions)
        {
            // Создаем экземпляр настроек
            var options = new HttpClientServiceOptions();
            configureOptions?.Invoke(options);

            // Регистрируем опции как синглтон
            services.TryAddSingleton(options);

            // Регистрируем сервис как синглтон
            services.TryAddSingleton<IHttpClientService>(provider =>
            {
                var serviceOptions = provider.GetRequiredService<HttpClientServiceOptions>();
                return new HttpClientService(serviceOptions);
            });

            return services;
        }
    }
}
