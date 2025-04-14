using System.Text.Json;

namespace KpDataLoader.Settings
{
    /// <summary>
    /// Реализация сервиса настроек
    /// </summary>
    public class SettingsService : ISettingsService
    {
        public AppSettings Settings { get; private set; }

        // Путь к файлу настроек
        private readonly string _settingsFilePath;

        public SettingsService(string settingsFilePath)
        {
            this._settingsFilePath = settingsFilePath;
            this.LoadSettings();
        }

        /// <summary>
        /// Загружает настройки из файла
        /// </summary>
        public void LoadSettings()
        {
            try
            {
                if (!File.Exists(this._settingsFilePath))
                {
                    throw new FileNotFoundException($"Файл настроек не найден: {this._settingsFilePath}");
                }

                string json = File.ReadAllText(this._settingsFilePath);
                this.Settings = JsonSerializer.Deserialize<AppSettings>(json);

                Console.WriteLine("Настройки успешно загружены.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке настроек: {ex.Message}");
                throw;
            }
        }
    }
}
