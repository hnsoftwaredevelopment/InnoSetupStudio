namespace InnoSetupStudio.Core.Project;

/// <inheritdoc cref="IProjectAssetService"/>
public sealed class ProjectAssetService : IProjectAssetService
{
    // Eigen, vaste submap voor gekopieerde bestanden, los van CustomImagesPath (InstallerProject):
    // dat is de map die de gebruiker zelf aanwijst als bron voor eigen wizard-afbeeldingen (fase
    // 2), dit is waar Inno Setup Studio zelf naartoe kopieert zodra een gekozen bestand van
    // buiten de projectmap komt. Eén map voor alle soorten bestanden (licentie, later
    // afbeeldingen, ...) in plaats van per bestandstype een andere map: eenvoudiger, en de
    // gebruiker hoeft niet te weten in welke submap welk type terechtkomt.
    private const string AssetsFolderName = "Assets";

    public string Import(string? projectFilePath, string sourceFilePath)
    {
        if (string.IsNullOrWhiteSpace(sourceFilePath) || !File.Exists(sourceFilePath))
        {
            return sourceFilePath;
        }

        if (string.IsNullOrWhiteSpace(projectFilePath))
        {
            return sourceFilePath;
        }

        var projectDirectory = Path.GetDirectoryName(Path.GetFullPath(projectFilePath));
        if (string.IsNullOrWhiteSpace(projectDirectory))
        {
            return sourceFilePath;
        }

        var fullSourcePath = Path.GetFullPath(sourceFilePath);

        if (IsInsideDirectory(fullSourcePath, projectDirectory))
        {
            // Al binnen de projectmap (bijvoorbeeld een eerdere kopie in Assets, of een bestand
            // dat de gebruiker daar zelf al had staan): niets te doen.
            return fullSourcePath;
        }

        var assetsDirectory = Path.Combine(projectDirectory, AssetsFolderName);
        Directory.CreateDirectory(assetsDirectory);

        var destinationPath = MakeUnique(Path.Combine(assetsDirectory, Path.GetFileName(fullSourcePath)));
        File.Copy(fullSourcePath, destinationPath, overwrite: false);

        return destinationPath;
    }

    private static bool IsInsideDirectory(string path, string directory)
    {
        var normalizedDirectory = directory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;

        return path.StartsWith(normalizedDirectory, StringComparison.OrdinalIgnoreCase);
    }

    private static string MakeUnique(string path)
    {
        if (!File.Exists(path))
        {
            return path;
        }

        var directory = Path.GetDirectoryName(path)!;
        var baseName = Path.GetFileNameWithoutExtension(path);
        var extension = Path.GetExtension(path);

        for (var i = 2; ; i++)
        {
            var candidate = Path.Combine(directory, $"{baseName} ({i}){extension}");
            if (!File.Exists(candidate))
            {
                return candidate;
            }
        }
    }
}
