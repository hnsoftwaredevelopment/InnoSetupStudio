using InnoSetupStudio.Core.Project;

namespace InnoSetupStudio.Tests;

public class InstallerProjectTests
{
    [Fact]
    public void CreateNewGeneratesBracedUppercaseGuidAppId()
    {
        var project = InstallerProject.CreateNew();

        Assert.StartsWith("{", project.AppId);
        Assert.EndsWith("}", project.AppId);
        Assert.True(Guid.TryParse(project.AppId.Trim('{', '}'), out _));
        Assert.Equal(project.AppId.Trim('{', '}').ToUpperInvariant(), project.AppId.Trim('{', '}'));
    }

    [Fact]
    public void CreateNewProducesDifferentAppIdEachTime()
    {
        var first = InstallerProject.CreateNew();
        var second = InstallerProject.CreateNew();

        Assert.NotEqual(first.AppId, second.AppId);
    }

    [Fact]
    public async Task JsonInstallerProjectServiceRoundTripsAllFields()
    {
        var project = InstallerProject.CreateNew();
        project.AppName = "Mijn Applicatie";
        project.AppVersion = "1.2.3";
        project.Publisher = "HN Software Development";
        project.PublisherEmail = "info@example.com";
        project.PublisherUrl = "https://example.com";
        project.SourceFilesPath = @"C:\Source";
        project.OutputPath = @"C:\Output";
        project.CustomImagesPath = @"C:\Images";
        project.SetupIconFile = @"C:\Icons\setup.ico";

        var service = new JsonInstallerProjectService();
        var tempPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.issproj");

        try
        {
            await service.SaveAsync(tempPath, project);
            var loaded = await service.LoadAsync(tempPath);

            Assert.Equal(project.AppId, loaded.AppId);
            Assert.Equal(project.AppName, loaded.AppName);
            Assert.Equal(project.AppVersion, loaded.AppVersion);
            Assert.Equal(project.Publisher, loaded.Publisher);
            Assert.Equal(project.PublisherEmail, loaded.PublisherEmail);
            Assert.Equal(project.PublisherUrl, loaded.PublisherUrl);
            Assert.Equal(project.SourceFilesPath, loaded.SourceFilesPath);
            Assert.Equal(project.OutputPath, loaded.OutputPath);
            Assert.Equal(project.CustomImagesPath, loaded.CustomImagesPath);
            Assert.Equal(project.SetupIconFile, loaded.SetupIconFile);
        }
        finally
        {
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }
    }

    [Fact]
    public async Task SaveAsyncRetriesAndSucceedsWhenDestinationBrieflyLocked()
    {
        // Reproduceert het scenario dat Herbert tegenkwam: resaven van een bestaand
        // .issproj-bestand terwijl iets anders (in de praktijk: OneDrive) het doelbestand
        // eventjes exclusief vasthoudt vlak nadat het geschreven is.
        var project = InstallerProject.CreateNew();
        project.AppName = "Vergrendeld project";

        var service = new JsonInstallerProjectService();
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.issproj");
        await File.WriteAllTextAsync(path, "{}");

        var lockStream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        try
        {
            var saveTask = service.SaveAsync(path, project);

            // Simuleert dat de vergrendeling na een fractie van een seconde weer loslaat, ruim
            // binnen het retry-venster van SaveAsync.
            await Task.Delay(300);
            lockStream.Dispose();

            await saveTask;

            var loaded = await service.LoadAsync(path);
            Assert.Equal("Vergrendeld project", loaded.AppName);
            Assert.False(File.Exists(path + ".tmp"), "Geen .tmp-bestand mag achterblijven na een geslaagde save.");
        }
        finally
        {
            lockStream.Dispose();
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    [Fact]
    public async Task SaveAsyncCleansUpTempFileAndThrowsClearErrorWhenDestinationStaysLocked()
    {
        var project = InstallerProject.CreateNew();
        var service = new JsonInstallerProjectService();
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.issproj");
        await File.WriteAllTextAsync(path, "{}");

        var lockStream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);

        try
        {
            var ex = await Assert.ThrowsAsync<IOException>(() => service.SaveAsync(path, project));

            Assert.Contains(path, ex.Message);
            Assert.False(File.Exists(path + ".tmp"), "Het tijdelijke bestand moet opgeruimd worden na een mislukte save.");
        }
        finally
        {
            lockStream.Dispose();
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}
