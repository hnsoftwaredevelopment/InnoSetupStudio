namespace InnoSetupStudio.Core.Settings;

public interface ISettingsService
{
    AppSettings Current { get; }

    Task LoadAsync();

    Task SaveAsync();
}
