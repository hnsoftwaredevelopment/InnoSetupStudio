using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using InnoSetupStudio.App.Localization;

namespace InnoSetupStudio.App.ViewModels.Screens;

/// <summary>
/// Bestemmingspagina: de standaard installatiemap die Inno Setup voorstelt, en of de gebruiker
/// die mag wijzigen. Komt overeen met InstallerProject.DefaultDirName/AllowUserToChangeDir.
/// </summary>
public sealed partial class SelectDestinationPageEditorViewModel : WizardScreenEditorViewModel
{
    private readonly string _appName;

    public SelectDestinationPageEditorViewModel(string appName, string defaultDirName, bool allowUserToChangeDir)
        : base("ShowSelectDestinationPage", LocalizationManager.Instance["WizardScreenSelectDestination"], "Folder")
    {
        _appName = appName;
        _defaultDirName = defaultDirName;
        _allowUserToChangeDir = allowUserToChangeDir;
    }

    [ObservableProperty]
    private string _defaultDirName;

    [ObservableProperty]
    private bool _allowUserToChangeDir;

    /// <summary>Voorvertoningstekst boven het map-veld, met de echte projectnaam erin.</summary>
    public string InstallIntroText =>
        $"Setup will install {(string.IsNullOrWhiteSpace(_appName) ? "the application" : _appName)} into the following folder.";

    /// <summary>Wat de voorvertoning in het map-veld toont: het ingevulde pad, of anders Inno
    /// Setup's eigen standaardvoorstel ({autopf}\AppName) als voorbeeld.</summary>
    public string DisplayDirName => string.IsNullOrWhiteSpace(DefaultDirName)
        ? $"{{autopf}}\\{(string.IsNullOrWhiteSpace(_appName) ? "App" : _appName)}"
        : DefaultDirName;

    /// <summary>Toont een toelichting in de voorvertoning zodra de gebruiker de map niet meer mag
    /// wijzigen tijdens de installatie, zodat duidelijk is waarom de Bladeren-knop daar uitstaat.</summary>
    public Visibility ChangeDirHintVisibility => AllowUserToChangeDir ? Visibility.Collapsed : Visibility.Visible;

    partial void OnDefaultDirNameChanged(string value) => OnPropertyChanged(nameof(DisplayDirName));

    partial void OnAllowUserToChangeDirChanged(bool value) => OnPropertyChanged(nameof(ChangeDirHintVisibility));
}
