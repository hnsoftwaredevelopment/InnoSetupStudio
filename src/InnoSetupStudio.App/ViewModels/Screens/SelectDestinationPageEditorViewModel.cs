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
        _browseButtonCaption = browseButtonSettings.Caption;
        _browseButtonEnabled = browseButtonSettings.Enabled;
        _browseButtonVisible = browseButtonSettings.Visible;
        _browseButtonTextColor = browseButtonSettings.TextColor;
        _browseButtonFontFamily = browseButtonSettings.FontFamily;
        _browseButtonFontSize = browseButtonSettings.FontSize;
        _browseButtonFontBold = browseButtonSettings.FontBold;
        _browseButtonTooltip = browseButtonSettings.Tooltip;
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

    partial void OnAllowUserToChangeDirChanged(bool value)
    {
        OnPropertyChanged(nameof(ChangeDirHintVisibility));
        OnPropertyChanged(nameof(IsBrowseButtonEnabledInPreview));
    }

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
    // via het Standaardscherm. Caption is sinds 2026-09-29 wél een veld (zie BrowseButtonSettings)
    // — tweelaags net als hieronder, alleen met Inno Setup's eigen ingebouwde knoptekst als
    // terugvalwaarde in plaats van een derde, Standaardscherm-laag.

    [ObservableProperty]
    private string _browseButtonCaption;

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

    [ObservableProperty]
    private string _browseButtonTooltip;

    // Hergebruikt de kleurenkiezer van de basisklasse (WizardScreenEditorViewModel.PickColor,
    // protected static): geen eigen kopie nodig, deze klasse erft al van die basisklasse (anders
    // dan DefaultScreenEditorViewModel, die geen gedeelde basisklasse heeft).
    [RelayCommand]
    private void PickBrowseButtonTextColor() => BrowseButtonTextColor = PickColor(BrowseButtonTextColor);

    /// <summary>Inno Setup's eigen standaardtekst voor de Bladeren-knop, gebruikt zolang
    /// <see cref="BrowseButtonCaption"/> leeg is. Zelfde "toon de studio's eigen UI-taal, niet
    /// Inno Setup's vaste Engelse tekst"-aanpak als DefaultBackButtonCaption e.a. in
    /// WizardScreenEditorViewModel.</summary>
    private static string DefaultBrowseButtonCaption => LocalizationManager.Instance["ButtonWizardBrowse"];

    /// <summary>Wat de voorvertoning daadwerkelijk op de Bladeren-knop toont: eigen tekst, anders
    /// <see cref="DefaultBrowseButtonCaption"/>. Tweelaags (geen Standaardscherm-laag, zie
    /// BrowseButtonSettings): dezelfde ResolveCaption-aanpak als de basisklasse, maar die methode
    /// is daar private, dus hier een eigen, verder identieke regel.</summary>
    public string EffectiveBrowseButtonCaption =>
        !string.IsNullOrWhiteSpace(BrowseButtonCaption) ? BrowseButtonCaption : DefaultBrowseButtonCaption;

    partial void OnBrowseButtonCaptionChanged(string value)
    {
        if (!string.IsNullOrEmpty(value) && string.IsNullOrWhiteSpace(value))
        {
            BrowseButtonCaption = string.Empty;
            return;
        }

        OnPropertyChanged(nameof(EffectiveBrowseButtonCaption));
    }

    // Herbert (2026-09-30): alle knoppen moeten dezelfde bewerkingsfunctionaliteiten krijgen als
    // Terug/Volgende/Annuleren, tenzij Inno Setup dat niet ondersteunt. Enabled/Visible stonden
    // hier al als velden (zie hierboven, ButtonPropertiesWindow kon ze al instellen), maar zonder
    // de leeg-is-true-terugvalwaarde die Terug/Volgende/Annuleren wél hebben (IsXxxButtonVisible/
    // IsXxxButtonEnabled in WizardScreenEditorViewModel) — en de voorvertoning hieronder gebruikte
    // ze zelfs helemaal niet: alleen AllowUserToChangeDir bepaalde IsEnabled, dus een expliciete
    // "Bladeren-knop uitschakelen"-instelling had zichtbaar geen enkel effect. Tweelaags, geen
    // Standaardscherm-cascade — zelfde reden als bij Caption hierboven (deze knop komt maar op één
    // scherm voor).

    /// <summary>True tenzij de Bladeren-knop expliciet op onzichtbaar gezet is.</summary>
    public bool IsBrowseButtonVisible => BrowseButtonVisible ?? true;

    /// <summary>True tenzij de Bladeren-knop expliciet op uitgeschakeld gezet is. Gebruikt de
    /// voorvertoning niet rechtstreeks — zie <see cref="IsBrowseButtonEnabledInPreview"/>, die dit
    /// combineert met Inno Setup's eigen ingebouwde gedrag.</summary>
    public bool IsBrowseButtonEnabled => BrowseButtonEnabled ?? true;

    /// <summary>Wat de voorvertoning daadwerkelijk als IsEnabled van de Bladeren-knop gebruikt:
    /// zowel Inno Setup's eigen ingebouwde gedrag (de knop gaat sowieso uit zodra de gebruiker de
    /// map niet mag wijzigen, zie AllowUserToChangeDir/ChangeDirHintVisibility) als de knop-eigen
    /// Enabled-instelling moeten allebei "aan" staan.</summary>
    public bool IsBrowseButtonEnabledInPreview => AllowUserToChangeDir && IsBrowseButtonEnabled;

    partial void OnBrowseButtonVisibleChanged(bool? value) => OnPropertyChanged(nameof(IsBrowseButtonVisible));

    partial void OnBrowseButtonEnabledChanged(bool? value)
    {
        OnPropertyChanged(nameof(IsBrowseButtonEnabled));
        OnPropertyChanged(nameof(IsBrowseButtonEnabledInPreview));
    }

    /// <summary>Tegenhanger van de Bladerknop-velden in de constructor, gebruikt door
    /// WizardEditorViewModel.ApplyTo.</summary>
    public BrowseButtonSettings ReadBrowseButtonSettings() => new()
    {
        Caption = BrowseButtonCaption,
        Enabled = BrowseButtonEnabled,
        Visible = BrowseButtonVisible,
        TextColor = BrowseButtonTextColor,
        FontFamily = BrowseButtonFontFamily,
        FontSize = BrowseButtonFontSize,
        FontBold = BrowseButtonFontBold,
        Tooltip = BrowseButtonTooltip,
    };
}
