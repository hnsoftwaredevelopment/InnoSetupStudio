using InnoSetupStudio.Core.Generation;
using InnoSetupStudio.Core.Project;
using static InnoSetupStudio.Tests.Generation.GeneratorTestSupport;

namespace InnoSetupStudio.Tests.Generation;

/// <summary>
/// Vergelijkt het gegenereerde script met een vastgelegd voorbeeld in de map Golden. Een bewuste
/// wijziging van de uitvoer laat deze tests falen; dan is het de bedoeling het nieuwe resultaat te
/// lezen en, als het klopt, de golden file te vervangen. Dat gaat met de omgevingsvariabele
/// UPDATE_GOLDEN=1: de test schrijft het huidige resultaat naar de map Golden in de broncode en
/// faalt één keer, zodat een gewijzigd bestand nooit ongezien door een test komt.
/// </summary>
public class IssGeneratorGoldenTests
{
    [Fact]
    public void Minimal_project() => AssertGolden("Minimal", MinimalProject());

    [Fact]
    public void Full_project() => AssertGolden("Full", FullProject());

    [Fact]
    public void Classic_x86_multilingual_project() => AssertGolden("ClassicX86Multilingual", ClassicProject());

    [Fact]
    public void Buttons_project() => AssertGolden("Buttons", ButtonsProject());

    private static InstallerProject MinimalProject() => SampleProject();

    private static InstallerProject ButtonsProject()
    {
        var project = SampleProject();
        project.LicenseFilePath = @"C:\Docs\licentie.txt";
        project.InfoBeforeFilePath = @"C:\Docs\voor.rtf";
        project.InfoAfterFilePath = @"C:\Docs\na.txt";
        ApplyButtonSettings(project);
        return project;
    }

    private static InstallerProject FullProject()
    {
        var project = SampleProject();
        project.Publisher = "Voortman Steel Machinery";
        project.PublisherUrl = "https://www.voortman.net";
        project.PublisherEmail = "info@voortman.net";
        project.OutputPath = @"C:\Uitvoer";
        project.OutputBaseFilename = "MijnApp-Setup";
        project.SetupIconFile = @"C:\Beeld\app.ico";
        project.WizardImageFile = @"C:\Beeld\groot.bmp";
        project.WizardSmallImageFile = @"C:\Beeld\klein.bmp";
        project.DefaultDirName = @"{autopf}\Voortman\MijnApp";
        project.DefaultGroupName = @"Voortman\MijnApp";
        project.DirPageMode = DisablePageMode.AutoSkipIfKnown;
        project.GroupPageMode = DisablePageMode.AlwaysShow;
        project.AppendDefaultGroupName = false;
        project.UsePreviousTasks = false;
        project.CreateStartMenuIcon = true;
        project.CreateDesktopIcon = true;
        project.SupportedLanguageIds = ["english", "dutch", "german"];
        project.LicenseFilePath = @"C:\Docs\licentie.txt";
        project.InfoBeforeFilePath = @"C:\Docs\voor.rtf";
        project.InfoAfterFilePath = @"C:\Docs\na.txt";
        project.DefaultUserInfoName = "Herbert";
        project.DefaultUserInfoOrg = "Voortman";
        project.DisableReadyMemo = true;
        project.WizardScreens.ShowWelcomePage = true;
        project.WizardScreens.ShowLicensePage = true;
        project.WizardScreens.ShowInfoBeforePage = true;
        project.WizardScreens.ShowUserInfoPage = true;
        project.WizardScreens.ShowSelectTasksPage = true;
        project.WizardScreens.ShowInfoAfterPage = true;
        return project;
    }

    private static InstallerProject ClassicProject()
    {
        var project = SampleProject();
        project.AppName = "Oude {Machine} \"Plus\"";
        project.AppVersion = "2.0";
        project.OutputBaseFilename = string.Empty; // standaardnaam uit AppName en AppVersion
        project.Architecture = InstallerArchitecture.X86;
        project.WizardStyle = InstallerWizardStyle.Classic;
        project.CreateStartMenuIcon = true;
        project.SupportedLanguageIds = ["french", "dutch", "spanish"];
        project.WizardScreens.ShowWelcomePage = false;
        project.WizardScreens.ShowSelectDestinationPage = false;
        project.WizardScreens.ShowSelectProgramGroupPage = false;
        project.WizardScreens.ShowReadyPage = false;
        project.WizardScreens.ShowFinishedPage = false;
        return project;
    }

    private static void AssertGolden(string name, InstallerProject project)
    {
        var actual = Normalize(Generate(project).Script);
        var sourcePath = Path.Combine(FindTestProjectDirectory(), "Golden", name + ".iss");

        if (Environment.GetEnvironmentVariable("UPDATE_GOLDEN") == "1")
        {
            Directory.CreateDirectory(Path.GetDirectoryName(sourcePath)!);
            File.WriteAllText(sourcePath, actual.Replace("\n", "\r\n", StringComparison.Ordinal), new System.Text.UTF8Encoding(false));
            Assert.Fail($"Golden file {name}.iss is opnieuw geschreven. Lees het resultaat, haal UPDATE_GOLDEN weg en draai de test opnieuw.");
        }

        Assert.True(File.Exists(sourcePath), $"Golden file {name}.iss ontbreekt. Maak hem met UPDATE_GOLDEN=1.");
        var expected = Normalize(File.ReadAllText(sourcePath));
        Assert.Equal(expected, actual);
    }

    private static string Normalize(string text) => text.Replace("\r\n", "\n", StringComparison.Ordinal);

    private static string FindTestProjectDirectory()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "InnoSetupStudio.Tests.csproj")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("Testproject niet gevonden vanaf " + AppContext.BaseDirectory);
    }
}
