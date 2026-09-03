using InnoSetupStudio.Core.Project;

namespace InnoSetupStudio.Tests;

public class ProjectAssetServiceTests
{
    [Fact]
    public void ImportCopiesFileFromOutsideProjectFolderIntoAssetsSubfolder()
    {
        using var scope = new TempScope();
        var projectFilePath = Path.Combine(scope.ProjectDirectory, "Mijn Project.issproj");
        var sourceFilePath = Path.Combine(scope.OutsideDirectory, "licentie.txt");
        File.WriteAllText(sourceFilePath, "Licentietekst");

        var service = new ProjectAssetService();
        var result = service.Import(projectFilePath, sourceFilePath);

        var expectedPath = Path.Combine(scope.ProjectDirectory, "Assets", "licentie.txt");
        Assert.Equal(expectedPath, result);
        Assert.True(File.Exists(expectedPath));
        Assert.Equal("Licentietekst", File.ReadAllText(expectedPath));
        Assert.True(File.Exists(sourceFilePath), "Het oorspronkelijke bestand mag niet verplaatst worden, alleen gekopieerd.");
    }

    [Fact]
    public void ImportLeavesPathUnchangedWhenProjectNotYetSaved()
    {
        using var scope = new TempScope();
        var sourceFilePath = Path.Combine(scope.OutsideDirectory, "licentie.txt");
        File.WriteAllText(sourceFilePath, "Licentietekst");

        var service = new ProjectAssetService();
        var result = service.Import(projectFilePath: null, sourceFilePath);

        Assert.Equal(sourceFilePath, result);
    }

    [Fact]
    public void ImportLeavesPathUnchangedWhenSourceAlreadyInsideProjectFolder()
    {
        using var scope = new TempScope();
        var projectFilePath = Path.Combine(scope.ProjectDirectory, "Mijn Project.issproj");
        var sourceFilePath = Path.Combine(scope.ProjectDirectory, "licentie.txt");
        File.WriteAllText(sourceFilePath, "Licentietekst");

        var service = new ProjectAssetService();
        var result = service.Import(projectFilePath, sourceFilePath);

        Assert.Equal(sourceFilePath, result);
        // Geen Assets-map aangemaakt: er viel niets te kopiëren.
        Assert.False(Directory.Exists(Path.Combine(scope.ProjectDirectory, "Assets")));
    }

    [Fact]
    public void ImportGivesSecondFileWithSameNameAUniqueCopy()
    {
        using var scope = new TempScope();
        var projectFilePath = Path.Combine(scope.ProjectDirectory, "Mijn Project.issproj");
        var assetsDirectory = Path.Combine(scope.ProjectDirectory, "Assets");
        Directory.CreateDirectory(assetsDirectory);
        File.WriteAllText(Path.Combine(assetsDirectory, "licentie.txt"), "Bestaande kopie");

        var sourceFilePath = Path.Combine(scope.OutsideDirectory, "licentie.txt");
        File.WriteAllText(sourceFilePath, "Nieuwe licentietekst");

        var service = new ProjectAssetService();
        var result = service.Import(projectFilePath, sourceFilePath);

        var expectedPath = Path.Combine(assetsDirectory, "licentie (2).txt");
        Assert.Equal(expectedPath, result);
        Assert.Equal("Bestaande kopie", File.ReadAllText(Path.Combine(assetsDirectory, "licentie.txt")));
        Assert.Equal("Nieuwe licentietekst", File.ReadAllText(expectedPath));
    }

    /// <summary>Ruimt de twee tijdelijke mappen (project en "ergens anders") na elke test weer op.</summary>
    private sealed class TempScope : IDisposable
    {
        public TempScope()
        {
            var root = Path.Combine(Path.GetTempPath(), $"iss-assets-test-{Guid.NewGuid()}");
            ProjectDirectory = Path.Combine(root, "Project");
            OutsideDirectory = Path.Combine(root, "Elders");
            Directory.CreateDirectory(ProjectDirectory);
            Directory.CreateDirectory(OutsideDirectory);
        }

        public string ProjectDirectory { get; }

        public string OutsideDirectory { get; }

        public void Dispose()
        {
            var root = Directory.GetParent(ProjectDirectory)!.FullName;
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }
}
