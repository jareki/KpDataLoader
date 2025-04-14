using Microsoft.Extensions.DependencyInjection;

namespace KpDataLoader.Settings
{
    /// <summary>
    /// Класс расширений для регистрации сервиса настроек в DI
    /// </summary>
    public static class SettingsServiceCollectionExtensions
    {
        /// <summary>
        /// Добавляет сервис настроек в DI контейнер
        /// </summary>
        public static AppSettings AddSettingsService(this IServiceCollection services, string settingsFilePath)
        {
            // Регистрируем как синглтон
            var settingsService = new SettingsService(settingsFilePath);
            services.AddSingleton<ISettingsService>(settingsService);

            // Дополнительно регистрируем сам объект настроек как отдельный синглтон
            services.AddSingleton(settingsService.Settings);

            // Возвращаем объект настроек для использования
            return settingsService.Settings;
        }
    }
}
