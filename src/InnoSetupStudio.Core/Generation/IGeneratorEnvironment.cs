namespace InnoSetupStudio.Core.Generation;

/// <summary>
/// De enige toegang van de generator tot het bestandssysteem: controleren of een map of bestand
/// bestaat, voor de waarschuwingen. Als interface zodat tests zonder echte bestanden werken.
/// </summary>
public interface IGeneratorEnvironment
{
    bool DirectoryExists(string path);

    bool FileExists(string path);
}

/// <summary>De standaardomgeving: kijkt op de echte schijf.</summary>
public sealed class DiskGeneratorEnvironment : IGeneratorEnvironment
{
    public bool DirectoryExists(string path) => Directory.Exists(path);

    public bool FileExists(string path) => File.Exists(path);
}
