using System.IO;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InnoSetupStudio.App.Localization;
using InnoSetupStudio.Core.Project;
using Microsoft.Win32;

namespace InnoSetupStudio.App.ViewModels.Screens;

/// <summary>
/// Bestemmingspagina: de standaard installatiemap die Inno Setup voorstelt, en of de gebruiker
/// die mag wijzigen. Komt overeen met InstallerProject.DefaultDirName/AllowUserToChangeDir.
/// </summary>
public sealed partial class SelectDestinationPageEditorViewModel : WizardScreenEditorViewModel
{
    private readonly string _appName;

    public SelectDestinationPageEditorViewModel(string appName, string defaultDirName, bool allowUserToChangeDir, BrowseButtonSettings browseButtonSettings)
        : base("ShowSelectDestinationPage", LocalizationManager.Instance["WizardScreenSelectDestination"], "Folder")
    {
        _appName = appName;
        _defaultDirName = defaultDirName;
        _allowUserToChangeDir = allowUserToChangeDir;
        _browseButtonEnabled = browseButtonSettings.Enabled;
        _browseButtonVisible = browseButtonSettings.Visible;
        _browseButtonTextColor = browseButtonSettings.TextColor;
        _browseButtonFontFamily = browseButtonSettings.FontFamily;
        _browseButtonFontSize = browseButtonSettings.FontSize;
        _browseButtonFontBold = browseButtonSettings.FontBold;
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

    [RelayCommand]
    private void Browse()
    {
        // Zelfde patroon als ProjectSettingsViewModel.BrowseForFolder: DefaultDirName is vaak
        // geen bestaand pad op deze machine maar een Inno Setup-constante zoals "{autopf}\App"
        // (zie LabelDefaultDirNameHint), dus InitialDirectory alleen zetten als het veld toevallig
        // wél een echte, bestaande map bevat. De dialoog opent anders gewoon zonder voorkeurspad.
        var dialog = new OpenFolderDialog();
        if (!string.IsNullOrWhiteSpace(DefaultDirName) && Directory.Exists(DefaultDirName))
        {
            dialog.InitialDirectory = DefaultDirName;
        }

        if (dialog.ShowDialog() == true)
        {
            DefaultDirName = dialog.FolderName;
        }
    }

    // Eigenschappen van de schermspecifieke "Bladeren"-knop zelf (Inno Setup's
    // WizardForm.DirBrowseButton) — niet te verwarren met de Browse()-opdracht hierboven, die de
    // knop is in Inno Setup Studio's EIGEN UI om een map te kiezen voor DefaultDirName. Zie
    // BrowseButtonSettings voor waarom dit los staat van de drie gedeelde Terug-/Volgende-/
    // Annuleren-knoppen: deze knop komt maar op dit ene scherm voor, dus geen Effective*-resolutie
    // via het Standaardscherm, en bewust geen Caption (Herbert heeft dat veld niet gevraagd).

    [ObservableProperty]
    private bool? _browseButtonEnabled;

    [ObservableProperty]
    private bool? _browseButtonVisible;

    [ObservableProperty]
    private string _browseButtonTextColor;

    [ObservableProperty]
    private string _browseButtonFontFamily;

    [ObservableProperty]
    private int? _browseButtonFontSize;

    [ObservableProperty]
    private bool? _browseButtonFontBold;

    /// <summary>Tegenhanger van de Bladerknop-velden in de constructor, gebruikt door
    /// WizardEditorViewModel.ApplyTo.</summary>
    public BrowseButtonSettings ReadBrowseButtonSettings() => new()
    {
        Enabled = BrowseButtonEnabled,
        Visible = BrowseButtonVisible,
        TextColor = BrowseButtonTextColor,
        FontFamily = BrowseButtonFontFamily,
        FontSize = BrowseButtonFontSize,
        FontBold = BrowseButtonFontBold,
    };
}
