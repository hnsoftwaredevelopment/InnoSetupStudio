using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InnoSetupStudio.App.Localization;
using InnoSetupStudio.Core.Project;

namespace InnoSetupStudio.App.ViewModels;

/// <summary>
/// ViewModel voor het wizardschermen-overzicht (fase 3): één rij per standaard Inno
/// Setup-wizardscherm, met een vinkje om het scherm in de installer op te nemen. Geeft geen
/// pixel-perfecte voorvertoning van elk scherm (dat is de eigen WPF-nabootsing uit fase 4), maar
/// een herkenbaar icoon en naam per scherm.
/// </summary>
public sealed partial class WizardScreensViewModel : ObservableObject
{
    public WizardScreensViewModel(WizardScreenSelection selection)
    {
        Screens =
        [
            CreateRow("ShowWelcomePage", "WizardScreenWelcome", "Document", selection.ShowWelcomePage),
            CreateRow("ShowLicensePage", "WizardScreenLicense", "Document", selection.ShowLicensePage),
            CreateRow("ShowInfoBeforePage", "WizardScreenInfoBefore", "Document", selection.ShowInfoBeforePage),
            CreateRow("ShowUserInfoPage", "WizardScreenUserInfo", "Document", selection.ShowUserInfoPage),
            CreateRow("ShowSelectDestinationPage", "WizardScreenSelectDestination", "Folder", selection.ShowSelectDestinationPage),
            CreateRow("ShowSelectComponentsPage", "WizardScreenSelectComponents", "List", selection.ShowSelectComponentsPage),
            CreateRow("ShowSelectProgramGroupPage", "WizardScreenSelectProgramGroup", "Folder", selection.ShowSelectProgramGroupPage),
            CreateRow("ShowSelectTasksPage", "WizardScreenSelectTasks", "List", selection.ShowSelectTasksPage),
            CreateRow("ShowReadyPage", "WizardScreenReady", "Check", selection.ShowReadyPage),
            CreateRow("ShowInfoAfterPage", "WizardScreenInfoAfter", "Document", selection.ShowInfoAfterPage),
            CreateRow("ShowFinishedPage", "WizardScreenFinished", "Check", selection.ShowFinishedPage),
        ];
    }

    /// <summary>De elf standaard wizardschermen, in de volgorde waarin Inno Setup ze toont.</summary>
    public IReadOnlyList<WizardScreenRow> Screens { get; }

    /// <summary>Vuurt wanneer het venster moet sluiten: true bij Opslaan, false bij Annuleren.</summary>
    public event EventHandler<bool>? RequestClose;

    [RelayCommand]
    private void Save() => RequestClose?.Invoke(this, true);

    [RelayCommand]
    private void Cancel() => RequestClose?.Invoke(this, false);

    /// <summary>Bouwt een nieuwe <see cref="WizardScreenSelection"/> met de huidige vinkjes.</summary>
    public WizardScreenSelection ToSelection()
    {
        bool IsEnabled(string id) => Screens.First(s => s.Id == id).IsEnabled;

        return new WizardScreenSelection
        {
            ShowWelcomePage = IsEnabled("ShowWelcomePage"),
            ShowLicensePage = IsEnabled("ShowLicensePage"),
            ShowInfoBeforePage = IsEnabled("ShowInfoBeforePage"),
            ShowUserInfoPage = IsEnabled("ShowUserInfoPage"),
            ShowSelectDestinationPage = IsEnabled("ShowSelectDestinationPage"),
            ShowSelectComponentsPage = IsEnabled("ShowSelectComponentsPage"),
            ShowSelectProgramGroupPage = IsEnabled("ShowSelectProgramGroupPage"),
            ShowSelectTasksPage = IsEnabled("ShowSelectTasksPage"),
            ShowReadyPage = IsEnabled("ShowReadyPage"),
            ShowInfoAfterPage = IsEnabled("ShowInfoAfterPage"),
            ShowFinishedPage = IsEnabled("ShowFinishedPage"),
        };
    }

    private static WizardScreenRow CreateRow(string id, string resourceKey, string iconKey, bool isEnabled) =>
        new(id, LocalizationManager.Instance[resourceKey], iconKey, isEnabled);
}
