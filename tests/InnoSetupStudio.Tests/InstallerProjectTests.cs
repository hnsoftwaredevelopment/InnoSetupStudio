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
        project.WizardScreens = new WizardScreenSelection
        {
            ShowWelcomePage = false,
            ShowLicensePage = true,
            ShowInfoBeforePage = true,
            ShowUserInfoPage = true,
            ShowSelectDestinationPage = false,
            ShowSelectComponentsPage = true,
            ShowSelectProgramGroupPage = false,
            ShowSelectTasksPage = true,
            ShowReadyPage = false,
            ShowInfoAfterPage = true,
            ShowFinishedPage = false,
        };
        project.WelcomeScreenButtons = new WizardScreenButtonSettings
        {
            BackButtonCaption = "Terug",
            BackButtonEnabled = false,
            BackButtonVisible = true,
            NextButtonCaption = "Doorgaan",
            NextButtonEnabled = true,
            NextButtonVisible = false,
            CancelButtonCaption = "Stoppen",
            CancelButtonEnabled = null,
            CancelButtonVisible = null,
        };
        project.LicenseScreenButtons = new WizardScreenButtonSettings
        {
            BackButtonCaption = "Vorige",
            BackButtonEnabled = true,
            BackButtonVisible = false,
            NextButtonCaption = "Akkoord",
            NextButtonEnabled = false,
            NextButtonVisible = true,
            CancelButtonCaption = "Weigeren",
            CancelButtonEnabled = true,
            CancelButtonVisible = true,
        };
        project.SelectDestinationScreenButtons = new WizardScreenButtonSettings
        {
            BackButtonCaption = "Terugkeren",
            BackButtonEnabled = null,
            BackButtonVisible = null,
            NextButtonCaption = "Installeren",
            NextButtonEnabled = true,
            NextButtonVisible = true,
            CancelButtonCaption = "Afbreken",
            CancelButtonEnabled = false,
            CancelButtonVisible = false,
        };

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
            Assert.Equal(project.WizardScreens.ShowWelcomePage, loaded.WizardScreens.ShowWelcomePage);
            Assert.Equal(project.WizardScreens.ShowLicensePage, loaded.WizardScreens.ShowLicensePage);
            Assert.Equal(project.WizardScreens.ShowInfoBeforePage, loaded.WizardScreens.ShowInfoBeforePage);
            Assert.Equal(project.WizardScreens.ShowUserInfoPage, loaded.WizardScreens.ShowUserInfoPage);
            Assert.Equal(project.WizardScreens.ShowSelectDestinationPage, loaded.WizardScreens.ShowSelectDestinationPage);
            Assert.Equal(project.WizardScreens.ShowSelectComponentsPage, loaded.WizardScreens.ShowSelectComponentsPage);
            Assert.Equal(project.WizardScreens.ShowSelectProgramGroupPage, loaded.WizardScreens.ShowSelectProgramGroupPage);
            Assert.Equal(project.WizardScreens.ShowSelectTasksPage, loaded.WizardScreens.ShowSelectTasksPage);
            Assert.Equal(project.WizardScreens.ShowReadyPage, loaded.WizardScreens.ShowReadyPage);
            Assert.Equal(project.WizardScreens.ShowInfoAfterPage, loaded.WizardScreens.ShowInfoAfterPage);
            Assert.Equal(project.WizardScreens.ShowFinishedPage, loaded.WizardScreens.ShowFinishedPage);
            Assert.Equal(project.WelcomeScreenButtons.BackButtonCaption, loaded.WelcomeScreenButtons.BackButtonCaption);
            Assert.Equal(project.WelcomeScreenButtons.BackButtonEnabled, loaded.WelcomeScreenButtons.BackButtonEnabled);
            Assert.Equal(project.WelcomeScreenButtons.BackButtonVisible, loaded.WelcomeScreenButtons.BackButtonVisible);
            Assert.Equal(project.WelcomeScreenButtons.NextButtonCaption, loaded.WelcomeScreenButtons.NextButtonCaption);
            Assert.Equal(project.WelcomeScreenButtons.NextButtonEnabled, loaded.WelcomeScreenButtons.NextButtonEnabled);
            Assert.Equal(project.WelcomeScreenButtons.NextButtonVisible, loaded.WelcomeScreenButtons.NextButtonVisible);
            Assert.Equal(project.WelcomeScreenButtons.CancelButtonCaption, loaded.WelcomeScreenButtons.CancelButtonCaption);
            Assert.Null(loaded.WelcomeScreenButtons.CancelButtonEnabled);
            Assert.Null(loaded.WelcomeScreenButtons.CancelButtonVisible);
            Assert.Equal(project.LicenseScreenButtons.BackButtonCaption, loaded.LicenseScreenButtons.BackButtonCaption);
            Assert.Equal(project.LicenseScreenButtons.BackButtonEnabled, loaded.LicenseScreenButtons.BackButtonEnabled);
            Assert.Equal(project.LicenseScreenButtons.BackButtonVisible, loaded.LicenseScreenButtons.BackButtonVisible);
            Assert.Equal(project.LicenseScreenButtons.NextButtonCaption, loaded.LicenseScreenButtons.NextButtonCaption);
            Assert.Equal(project.LicenseScreenButtons.NextButtonEnabled, loaded.LicenseScreenButtons.NextButtonEnabled);
            Assert.Equal(project.LicenseScreenButtons.NextButtonVisible, loaded.LicenseScreenButtons.NextButtonVisible);
            Assert.Equal(project.LicenseScreenButtons.CancelButtonCaption, loaded.LicenseScreenButtons.CancelButtonCaption);
            Assert.Equal(project.LicenseScreenButtons.CancelButtonEnabled, loaded.LicenseScreenButtons.CancelButtonEnabled);
            Assert.Equal(project.LicenseScreenButtons.CancelButtonVisible, loaded.LicenseScreenButtons.CancelButtonVisible);
            Assert.Equal(project.SelectDestinationScreenButtons.BackButtonCaption, loaded.SelectDestinationScreenButtons.BackButtonCaption);
            Assert.Null(loaded.SelectDestinationScreenButtons.BackButtonEnabled);
            Assert.Null(loaded.SelectDestinationScreenButtons.BackButtonVisible);
            Assert.Equal(project.SelectDestinationScreenButtons.NextButtonCaption, loaded.SelectDestinationScreenButtons.NextButtonCaption);
            Assert.Equal(project.SelectDestinationScreenButtons.NextButtonEnabled, loaded.SelectDestinationScreenButtons.NextButtonEnabled);
            Assert.Equal(project.SelectDestinationScreenButtons.NextButtonVisible, loaded.SelectDestinationScreenButtons.NextButtonVisible);
            Assert.Equal(project.SelectDestinationScreenButtons.CancelButtonCaption, loaded.SelectDestinationScreenButtons.CancelButtonCaption);
            Assert.Equal(project.SelectDestinationScreenButtons.CancelButtonEnabled, loaded.SelectDestinationScreenButtons.CancelButtonEnabled);
            Assert.Equal(project.SelectDestinationScreenButtons.CancelButtonVisible, loaded.SelectDestinationScreenButtons.CancelButtonVisible);
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
    public async Task LoadAsyncRejectsFileLargerThanConfiguredLimit()
    {
        // Voorkomt dat een enorm (per ongeluk of moedwillig groot) bestand volledig gedeserialiseerd
        // wordt voordat het als ongeldig projectbestand wordt afgewezen (CWE-400).
        var service = new JsonInstallerProjectService();
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.issproj");

        // Ruim boven de 10 MB-limiet in JsonInstallerProjectService, met geldige lege JSON-inhoud
        // eromheen zodat alleen de bestandsgrootte de reden van afwijzing kan zijn.
        var padding = new string(' ', 11 * 1024 * 1024);
        await File.WriteAllTextAsync(path, "{\"AppName\":\"" + padding + "\"}");

        try
        {
            var ex = await Assert.ThrowsAsync<IOException>(() => service.LoadAsync(path));
            Assert.Contains("te groot", ex.Message, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    [Fact]
    public async Task LoadAsyncRejectsJsonNullInsteadOfSilentlyCreatingNewProject()
    {
        // Een bestand met JSON null mag niet stilzwijgend als nieuw project (met een vers AppId)
        // worden behandeld: dat zou een volgende save het bestaande, ongeldige bestand laten
        // overschrijven zonder dat de gebruiker iets merkt.
        var service = new JsonInstallerProjectService();
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.issproj");
        await File.WriteAllTextAsync(path, "null");

        try
        {
            await Assert.ThrowsAsync<IOException>(() => service.LoadAsync(path));
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    [Fact]
    public async Task LoadAsyncNormalizesExplicitJsonNullWizardScreensToDefault()
    {
        // Een handmatig bewerkt of ouder projectbestand kan expliciet "WizardScreens": null
        // bevatten. Zonder normalisatie geeft dat een NullReferenceException zodra de
        // wizardschermen-selectie wordt geopend (bijvoorbeeld WizardScreensViewModel, die
        // meteen ShowWelcomePage etc. leest); LoadAsync moet dit stilzwijgend herstellen naar
        // een standaard WizardScreenSelection in plaats van null door te geven.
        var service = new JsonInstallerProjectService();
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.issproj");
        await File.WriteAllTextAsync(path, "{\"AppName\":\"Zonder wizardschermen\",\"WizardScreens\":null}");

        try
        {
            var loaded = await service.LoadAsync(path);

            Assert.NotNull(loaded.WizardScreens);
            Assert.True(loaded.WizardScreens.ShowWelcomePage);
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    [Fact]
    public async Task LoadAsyncNormalizesExplicitJsonNullButtonSettingsToDefault()
    {
        // Zelfde risico als bij WizardScreens (zie LoadAsyncNormalizesExplicitJsonNullWizardScreensToDefault),
        // maar dan voor de drie knopinstellingen-eigenschappen: een handmatig bewerkt of ouder
        // projectbestand kan expliciet "WelcomeScreenButtons": null (etc.) bevatten. Zonder
        // normalisatie geeft dat een NullReferenceException zodra de schermeditor voor dat
        // scherm wordt geopend.
        var service = new JsonInstallerProjectService();
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.issproj");
        await File.WriteAllTextAsync(
            path,
            "{\"AppName\":\"Zonder knopinstellingen\"," +
            "\"WelcomeScreenButtons\":null,\"LicenseScreenButtons\":null,\"SelectDestinationScreenButtons\":null}");

        try
        {
            var loaded = await service.LoadAsync(path);

            Assert.NotNull(loaded.WelcomeScreenButtons);
            Assert.NotNull(loaded.LicenseScreenButtons);
            Assert.NotNull(loaded.SelectDestinationScreenButtons);
            Assert.Equal(string.Empty, loaded.WelcomeScreenButtons.NextButtonCaption);
            Assert.Equal(string.Empty, loaded.LicenseScreenButtons.NextButtonCaption);
            Assert.Equal(string.Empty, loaded.SelectDestinationScreenButtons.NextButtonCaption);
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
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
