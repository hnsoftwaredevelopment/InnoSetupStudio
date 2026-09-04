using System.Text.Json;

namespace InnoSetupStudio.Core.Project;

/// <summary>Bewaart een <see cref="InstallerProject"/> als JSON-bestand (extensie .issproj).</summary>
public sealed class JsonInstallerProjectService : IInstallerProjectService
{
    private static readonly JsonSerializerOptions SerializerOptions = new() { WriteIndented = true };

    // Een .issproj is een klein JSON-bestand met alleen instellingen (nooit meer dan enkele KB in
    // de praktijk). Deze limiet voorkomt dat een (per ongeluk of moedwillig) enorm bestand
    // volledig in het geheugen wordt gedeserialiseerd voordat het als project wordt afgewezen.
    private const long MaxProjectFileSizeBytes = 10 * 1024 * 1024;

    public async Task<InstallerProject> LoadAsync(string filePath)
    {
        try
        {
            using var stream = await OpenReadWithRetryAsync(filePath);

            if (stream.Length > MaxProjectFileSizeBytes)
            {
                throw new IOException(
                    $"Het bestand is te groot ({stream.Length / (1024 * 1024)} MB) om een geldig projectbestand te zijn " +
                    $"(maximum {MaxProjectFileSizeBytes / (1024 * 1024)} MB).");
            }

            var loaded = await JsonSerializer.DeserializeAsync<InstallerProject>(stream)
                ?? throw new JsonException("Het projectbestand bevat geen geldig project (JSON null).");

            // Een handmatig bewerkt of ouder projectbestand kan expliciet "WizardScreens": null
            // bevatten. Zonder deze normalisatie geeft dat later een NullReferenceException zodra
            // de wizardschermen-selectie wordt geopend, in plaats van gewoon de standaardwaarden.
            loaded.WizardScreens ??= new();

            // Zelfde verhaal voor de knopinstellingen per scherm (inclusief het Standaardscherm,
            // fase 4 vervolg): een expliciete JSON-null voor een van deze vier eigenschappen zou
            // anders pas een NullReferenceException geven zodra de schermeditor wordt geopend.
            loaded.WelcomeScreenButtons ??= new();
            loaded.LicenseScreenButtons ??= new();
            loaded.SelectDestinationScreenButtons ??= new();
            loaded.DefaultScreenButtons ??= new();

            return loaded;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            throw new IOException($"Kan het projectbestand niet laden vanaf '{filePath}'. {ex.Message}", ex);
        }
    }

    public async Task SaveAsync(string filePath, InstallerProject project)
    {
        // Zelfde tijdelijk-bestand-dan-verplaatsen patroon als JsonSettingsService: voorkomt een
        // half weggeschreven, corrupt projectbestand bij een crash of stroomstoring.
        var tempPath = filePath + ".tmp";

        try
        {
            using (var stream = File.Create(tempPath))
            {
                await JsonSerializer.SerializeAsync(stream, project, SerializerOptions);
            }

            // Deze laatste stap (hernoemen naar de echte bestandsnaam) kan kortstondig falen
            // doordat Windows, een virusscanner of een cloud-synchronisatieclient (bijvoorbeeld
            // OneDrive, wat bij deze projectfolder het geval bleek) het doelbestand net na het
            // schrijven eventjes vasthoudt. Een paar keer kort opnieuw proberen lost dat in de
            // praktijk vrijwel altijd op; de ene keer dat dit gebeurde, bleef zonder deze retry
            // een ".tmp"-bestand achter zonder dat de gebruiker een bruikbare foutmelding kreeg.
            await MoveWithRetryAsync(tempPath, filePath);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            TryDeleteTempFile(tempPath);
            throw new IOException($"Kan het projectbestand niet opslaan naar '{filePath}'. {ex.Message}", ex);
        }
    }

    private static async Task<FileStream> OpenReadWithRetryAsync(string filePath)
    {
        const int maxAttempts = 5;
        var delay = TimeSpan.FromMilliseconds(150);

        for (var attempt = 1; attempt < maxAttempts; attempt++)
        {
            try
            {
                return File.OpenRead(filePath);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                await Task.Delay(delay);
                delay += delay;
            }
        }

        // Laatste poging: laat een eventuele uitzondering nu wél doorkomen naar de aanroeper.
        return File.OpenRead(filePath);
    }

    private static async Task MoveWithRetryAsync(string tempPath, string filePath)
    {
        const int maxAttempts = 5;
        var delay = TimeSpan.FromMilliseconds(150);

        for (var attempt = 1; attempt < maxAttempts; attempt++)
        {
            try
            {
                File.Move(tempPath, filePath, overwrite: true);
                return;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                await Task.Delay(delay);
                delay += delay;
            }
        }

        // Laatste poging: laat een eventuele uitzondering nu wél doorkomen naar de aanroeper.
        File.Move(tempPath, filePath, overwrite: true);
    }

    private static void TryDeleteTempFile(string tempPath)
    {
        try
        {
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }
        catch
        {
            // Opruimen is best-effort: een falende delete van het tijdelijke bestand mag de
            // eigenlijke foutmelding aan de gebruiker niet overschaduwen.
        }
    }
}
