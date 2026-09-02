using System.Text.Json;

namespace InnoSetupStudio.Core.Project;

/// <summary>Bewaart een <see cref="InstallerProject"/> als JSON-bestand (extensie .issproj).</summary>
public sealed class JsonInstallerProjectService : IInstallerProjectService
{
    private static readonly JsonSerializerOptions SerializerOptions = new() { WriteIndented = true };

    public async Task<InstallerProject> LoadAsync(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        var loaded = await JsonSerializer.DeserializeAsync<InstallerProject>(stream);
        return loaded ?? InstallerProject.CreateNew();
    }

    public async Task SaveAsync(string filePath, InstallerProject project)
    {
        // Zelfde tijdelijk-bestand-dan-verplaatsen patroon als JsonSettingsService: voorkomt een
        // half weggeschreven, corrupt projectbestand bij een crash of stroomstoring.
        var tempPath = filePath + ".tmp";
        using (var stream = File.Create(tempPath))
        {
            await JsonSerializer.SerializeAsync(stream, project, SerializerOptions);
        }

        File.Move(tempPath, filePath, overwrite: true);
    }
}
