using System.IO;
using System.Text.Json;

namespace InnoSetupStudio.Core.Settings;

/// <summary>Bewaart taal/thema-instellingen als een klein JSON-bestand onder %AppData%\InnoSetupStudio.</summary>
public sealed class JsonSettingsService : ISettingsService
{
    private static readonly JsonSerializerOptions SerializerOptions = new() { WriteIndented = true };
    private readonly string _filePath;

    public AppSettings Current { get; private set; } = new();

    public JsonSettingsService()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var folder = Path.Combine(appData, "InnoSetupStudio");
        Directory.CreateDirectory(folder);
        _filePath = Path.Combine(folder, "settings.json");
    }

    public async Task LoadAsync()
    {
        if (!File.Exists(_filePath))
        {
            return;
        }

        try
        {
            using var stream = File.OpenRead(_filePath);
            var loaded = await JsonSerializer.DeserializeAsync<AppSettings>(stream);
            if (loaded is not null)
            {
                Current = loaded;
            }
        }
        catch (JsonException)
        {
            // Beschadigd instellingenbestand: negeren en met de standaardinstellingen doorgaan.
        }
    }

    public async Task SaveAsync()
    {
        using var stream = File.Create(_filePath);
        await JsonSerializer.SerializeAsync(stream, Current, SerializerOptions);
    }
}
