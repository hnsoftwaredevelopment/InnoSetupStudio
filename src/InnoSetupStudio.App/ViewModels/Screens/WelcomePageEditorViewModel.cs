using InnoSetupStudio.App.Localization;
using InnoSetupStudio.Core.Project;

namespace InnoSetupStudio.App.ViewModels.Screens;

/// <summary>
/// Welkomstpagina: geen bewerkbare instellingen, alleen een voorvertoning op basis van de naam en
/// versie uit de projectinstellingen (fase 2). Het instellingenpaneel toont hiervoor alleen de
/// tekst uit ScreenEditorWelcomeInfo, geen invoervelden.
/// </summary>
public sealed class WelcomePageEditorViewModel : WizardScreenEditorViewModel
{
    public WelcomePageEditorViewModel(string appName, string appVersion, IProjectAssetService assetService, string? projectFilePath)
        : base("ShowWelcomePage", LocalizationManager.Instance["WizardScreenWelcome"], "Document", assetService, projectFilePath)
    {
        // De voorvertoning benadert Inno Setup's eigen (Engelstalige) standaardtekst voor deze
        // pagina, niet de UI-taal van Inno Setup Studio zelf: zie ScreenEditorPreviewDisclaimer.
        var displayName = string.IsNullOrWhiteSpace(appName) ? "Application" : appName;
        WelcomeTitle = $"Welcome to the {displayName} Setup Wizard";
        WelcomeBody = string.IsNullOrWhiteSpace(appVersion)
            ? $"This will install {displayName} on your computer.\n\nIt is recommended that you close all other applications before continuing."
            : $"This will install {displayName} version {appVersion} on your computer.\n\nIt is recommended that you close all other applications before continuing.";
    }

    public string WelcomeTitle { get; }

    public string WelcomeBody { get; }
}
