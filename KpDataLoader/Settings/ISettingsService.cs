namespace KpDataLoader.Settings;

/// <summary>
/// Интерфейс сервиса настроек
/// </summary>
public interface ISettingsService
{
    AppSettings Settings { get; }
    void LoadSettings();
}