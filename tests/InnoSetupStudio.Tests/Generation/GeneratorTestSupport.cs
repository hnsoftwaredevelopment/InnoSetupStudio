using InnoSetupStudio.Core.Generation;
using InnoSetupStudio.Core.Project;

namespace InnoSetupStudio.Tests.Generation;

/// <summary>Nepomgeving voor de generator: bestaande mappen en bestanden staan in een lijst, zodat
/// de tests niet van de schijf van de ontwikkelaar afhangen.</summary>
internal sealed class FakeGeneratorEnvironment : IGeneratorEnvironment
{
    private readonly HashSet<string> _directories = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _files = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Een omgeving waarin alles bestaat: geen enkele bestandsmelding.</summary>
    public static FakeGeneratorEnvironment Everything => new() { AllExist = true };

    /// <summary>Een omgeving waarin niets bestaat.</summary>
    public static FakeGeneratorEnvironment Nothing => new();

    public bool AllExist { get; init; }

    public FakeGeneratorEnvironment WithDirectory(string path)
    {
        _directories.Add(path);
        return this;
    }

    public FakeGeneratorEnvironment WithFile(string path)
    {
        _files.Add(path);
        return this;
    }

    public bool DirectoryExists(string path) => AllExist || _directories.Contains(path);

    public bool FileExists(string path) => AllExist || _files.Contains(path);
}

internal static class GeneratorTestSupport
{
    public const string SampleAppId = "{8C5C7F52-0F3E-4F5B-9A59-1D2E3F4A5B6C}";

    /// <summary>Een project met precies genoeg gegevens voor een foutloos script.</summary>
    public static InstallerProject SampleProject() => new()
    {
        AppId = SampleAppId,
        AppName = "MijnApp",
        AppVersion = "1.2.3",
        SourceFilesPath = @"C:\Bron\MijnApp",
        MainExecutable = "MijnApp.exe",
        // Gelijk aan de standaardnaam, zodat het script gelijk blijft en het voorbeeldproject
        // geen melding "geen bestandsnaam ingevuld" geeft.
        OutputBaseFilename = "MijnApp-1.2.3-Setup",
        CreateStartMenuIcon = false,
    };

    /// <summary>
    /// Zet alle acht schermen met een knopmodel aan, kiest Engels, Nederlands en Duits en vult
    /// knopinstellingen in die elk onderdeel van de generator raken: het Standaardscherm met een
    /// eigen waarde erboven, vertalingen voor alle talen en voor één taal, een vertaling zonder
    /// universele tekst, lettertype, grootte, vet, tooltip, uitgeschakeld, verborgen en beide
    /// Bladeren-knoppen. De bestanden voor de pagina's met een bestand stelt de aanroeper in.
    /// </summary>
    public static void ApplyButtonSettings(InstallerProject project)
    {
        project.SupportedLanguageIds = ["english", "dutch", "german"];
        var screens = project.WizardScreens;
        screens.ShowWelcomePage = true;
        screens.ShowLicensePage = true;
        screens.ShowInfoBeforePage = true;
        screens.ShowUserInfoPage = true;
        screens.ShowSelectDestinationPage = true;
        screens.ShowSelectProgramGroupPage = true;
        screens.ShowReadyPage = true;
        screens.ShowInfoAfterPage = true;

        var defaults = project.DefaultScreenButtons;
        defaults.BackButtonCaption = "Back";
        defaults.NextButtonCaption = "Continue";
        defaults.NextButtonTooltip = "Go on";
        defaults.NextButtonFontBold = true;
        defaults.CancelButtonCaption = "Stop";
        defaults.CancelButtonFontSize = 9;

        var welcome = project.WelcomeScreenButtons;
        welcome.NextButtonCaption = "Start";
        welcome.NextButtonCaptionByLanguage["dutch"] = "Begin";
        welcome.NextButtonCaptionByLanguage["german"] = "Los";
        welcome.NextButtonTooltip = "Let's go";
        welcome.CancelButtonEnabled = false;

        var license = project.LicenseScreenButtons;
        license.NextButtonCaption = "I agree";
        license.NextButtonCaptionByLanguage["dutch"] = "Akkoord";
        license.NextButtonFontBold = false;
        license.NextButtonFontFamily = "Consolas";

        var infoBefore = project.InfoBeforeScreenButtons;
        infoBefore.BackButtonCaptionByLanguage["german"] = "Zurück";
        infoBefore.NextButtonTooltipByLanguage["dutch"] = "Lees de informatie";

        var userInfo = project.UserInfoScreenButtons;
        userInfo.NextButtonTooltip = "Enter your details";
        userInfo.NextButtonTooltipByLanguage["dutch"] = "Vul je gegevens in";

        var selectDir = project.SelectDestinationScreenButtons;
        selectDir.NextButtonFontSize = 10;
        var dirBrowse = project.SelectDestinationBrowseButton;
        dirBrowse.Caption = "Find...";
        dirBrowse.CaptionByLanguage["dutch"] = "Zoeken...";
        dirBrowse.Tooltip = "Pick a folder";
        dirBrowse.FontBold = true;

        var groupBrowse = project.SelectProgramGroupBrowseButton;
        groupBrowse.CaptionByLanguage["dutch"] = "Zoeken...";
        groupBrowse.Enabled = false;

        var ready = project.ReadyScreenButtons;
        ready.NextButtonCaption = "Install now";
        ready.NextButtonCaptionByLanguage["dutch"] = "Nu installeren";
        ready.NextButtonFontSize = 12;

        var infoAfter = project.InfoAfterScreenButtons;
        infoAfter.CancelButtonVisible = false;
        infoAfter.BackButtonEnabled = false;
    }

    public static GenerationResult Generate(InstallerProject project, IGeneratorEnvironment? environment = null)
        => new IssGenerator(environment ?? FakeGeneratorEnvironment.Everything).Generate(project);

    /// <summary>De regels van het script, zonder lege regels en zonder commentaar.</summary>
    public static string[] Lines(GenerationResult result)
        => result.Script
            .Split("\r\n", StringSplitOptions.None)
            .Where(line => line.Length > 0 && !line.StartsWith(';'))
            .ToArray();

    public static bool HasIssue(GenerationResult result, GenerationIssueCode code)
        => result.Issues.Any(issue => issue.Code == code);
}
