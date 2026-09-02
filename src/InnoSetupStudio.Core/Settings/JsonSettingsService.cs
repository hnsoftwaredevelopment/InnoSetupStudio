using System.IO;
using System.Text.Json;

namespace InnoSetupStudio.Core.Settings;

/// <summary>Bewaart taal/thema-instellingen als een klein JSON-bestand onder %AppData%\InnoSetupStudio.</summary>
public sealed class JsonSettingsService : ISettingsService
{
    private static readonly JsonSerializerOptions SerializerOptions = new() { WriteIndented = true };

    /// <summary>De enige cultuurnamen waarvoor dit project vertalingen bijhoudt (Strings*.resx).</summary>
    private static readonly HashSet<string> SupportedLanguages = new(StringComparer.OrdinalIgnoreCase)
    {
        "nl-NL", "en-US", "de-DE",
    };

    private const string FallbackLanguage = "nl-NL";

    // Beschermt LoadAsync/SaveAsync tegen elkaar: zonder dit kan een load die halverwege een
    // save leest een half geschreven bestand tegenkomen.
    private readonly SemaphoreSlim _fileLock = new(1, 1);
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

        await _fileLock.WaitAsync();
        try
        {
            using var stream = File.OpenRead(_filePath);
            var loaded = await JsonSerializer.DeserializeAsync<AppSettings>(stream);
            if (loaded is not null)
            {
                if (!SupportedLanguages.Contains(loaded.Language))
                {
                    loaded.Language = FallbackLanguage;
                }

                Current = loaded;
            }
        }
        catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException)
        {
            // Beschadigd of (tijdelijk) ontoegankelijk instellingenbestand: negeren en met de
            // standaardinstellingen doorgaan in plaats van de hele app te laten crashen.
        }
        finally
        {
            _fileLock.Release();
        }
    }

    public async Task SaveAsync()
    {
        await _fileLock.WaitAsync();
        try
        {
            // Schrijf eerst naar een tijdelijk bestand en verplaats dat pas na een succesvolle
            // flush naar de echte bestandsnaam. Zo blijft settings.json bij een crash of
            // stroomstoring halverwege het schrijven altijd de vorige, complete versie, in
            // plaats van een half weggeschreven, corrupt bestand.
            var tempPath = _filePath + ".tmp";
            using (var stream = File.Create(tempPath))
            {
                await JsonSerializer.SerializeAsync(stream, Current, SerializerOptions);
            }

            File.Move(tempPath, _filePath, overwrite: true);
        }
        finally
        {
            _fileLock.Release();
        }
    }
}
