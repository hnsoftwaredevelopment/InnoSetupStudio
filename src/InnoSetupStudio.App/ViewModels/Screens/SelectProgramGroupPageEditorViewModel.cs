using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InnoSetupStudio.App.Localization;
using InnoSetupStudio.Core.Project;

namespace InnoSetupStudio.App.ViewModels.Screens;

/// <summary>
/// Select Start Menu Folder-pagina (Inno Setup's eigen naam voor deze pagina; de interne
/// <c>WizardScreenSelection.ShowSelectProgramGroupPage</c>-vlag heet naar Inno Setup's
/// Pascal Script-kant "ProgramGroup", zie de toelichting daar). Net als de Bestemmingspagina een
/// eigen "Bladeren"-knop (Inno Setup's WizardForm.GroupBrowseButton, sinds 2026-10-02 bewerkbaar,
/// Herberts verzoek) naast de vooringevulde groepsnaam, twee vinkjes en een drie-waardige
/// paginazichtbaarheid.
/// </summary>
public sealed partial class SelectProgramGroupPageEditorViewModel : WizardScreenEditorViewModel
{
    private readonly string _appName;

    public SelectProgramGroupPageEditorViewModel(
        string appName,
        string defaultGroupName,
        bool appendDefaultGroupName,
        bool alwaysUsePersonalGroup,
        DisablePageMode groupPageMode,
        BrowseButtonSettings browseButtonSettings)
        : base("ShowSelectProgramGroupPage", LocalizationManager.Instance["WizardScreenSelectProgramGroup"], "Folder")
    {
        _appName = appName;
        _defaultGroupName = defaultGroupName;
        _appendDefaultGroupName = appendDefaultGroupName;
        _alwaysUsePersonalGroup = alwaysUsePersonalGroup;
        _groupPageMode = groupPageMode;
        _browseButtonCaption = browseButtonSettings.Caption;
        _browseButtonEnabled = browseButtonSettings.Enabled;
        _browseButtonVisible = browseButtonSettings.Visible;
        _browseButtonTextColor = browseButtonSettings.TextColor;
        _browseButtonFontFamily = browseButtonSettings.FontFamily;
        _browseButtonFontSize = browseButtonSettings.FontSize;
        _browseButtonFontBold = browseButtonSettings.FontBold;
        _browseButtonTooltip = browseButtonSettings.Tooltip;
        _browseButtonCaptionByLanguage = new Dictionary<string, string>(browseButtonSettings.CaptionByLanguage);
        _browseButtonTooltipByLanguage = new Dictionary<string, string>(browseButtonSettings.TooltipByLanguage);
    }

    [ObservableProperty]
    private string _defaultGroupName;

    [ObservableProperty]
    private bool _appendDefaultGroupName;

    [ObservableProperty]
    private bool _alwaysUsePersonalGroup;

    [ObservableProperty]
    private DisablePageMode _groupPageMode;

    /// <summary>Toont een toelichting zodra deze pagina nooit getoond wordt (de voorgestelde
    /// groepsnaam ligt dan vast) — zie <see cref="DisablePageMode"/>.</summary>
    public Visibility IsNeverShowHintVisible => GroupPageMode == DisablePageMode.NeverShow ? Visibility.Visible : Visibility.Collapsed;

    /// <summary>Toont een toelichting zodra deze pagina bij een update automatisch wordt
    /// overgeslagen (Herberts verzoek, 2026-10-02) — de voorvertoning simuleert hier bewust een
    /// eerste installatie, zie <see cref="DisablePageMode.AutoSkipIfKnown"/>.</summary>
    public Visibility IsAutoSkipHintVisible => GroupPageMode == DisablePageMode.AutoSkipIfKnown ? Visibility.Visible : Visibility.Collapsed;

    partial void OnGroupPageModeChanged(DisablePageMode value)
    {
        OnPropertyChanged(nameof(IsNeverShowHintVisible));
        OnPropertyChanged(nameof(IsAutoSkipHintVisible));
        OnPropertyChanged(nameof(IsBrowseButtonEnabledInPreview));
    }

    /// <summary>Wat de voorvertoning in het mapveld toont: de ingevulde groepsnaam, of anders
    /// Inno Setup's eigen terugvalwaarde (de toepassingsnaam, zie InstallerProject.DefaultGroupName)
    /// als voorbeeld — zelfde aanpak als SelectDestinationPageEditorViewModel.DisplayDirName
    /// (CodeRabbit, PR #23: een leeg veld op een nieuw project toonde hier niets, terwijl het
    /// model al een terugval naar AppName belooft).</summary>
    public string EffectiveGroupName => string.IsNullOrWhiteSpace(DefaultGroupName)
        ? (string.IsNullOrWhiteSpace(_appName) ? "App" : _appName)
        : DefaultGroupName;

    partial void OnDefaultGroupNameChanged(string value) => OnPropertyChanged(nameof(EffectiveGroupName));

    // Eigenschappen van de schermspecifieke "Bladeren"-knop zelf (Inno Setup's
    // WizardForm.GroupBrowseButton, geverifieerd tegen Setup.WizardForm.pas: FGroupBrowseButton:
    // TNewButton, net als FDirBrowseButton op de Bestemmingspagina) — deze pagina heeft zelf geen
    // Browse()-opdracht in Inno Setup Studio's eigen UI, de groepsnaam is altijd vrije tekst. Zie
    // BrowseButtonSettings voor waarom dit los staat van de drie gedeelde Terug-/Volgende-/
    // Annuleren-knoppen: deze knop komt maar op dit ene scherm voor, dus geen Effective*-resolutie
    // via het Standaardscherm. Zelfde model hergebruikt als SelectDestinationBrowseButton
    // (Herberts verzoek, 2026-10-02: dezelfde bewerkingsmogelijkheden als de Bestemmingspagina)
    // in plaats van een tweede, bijna identieke klasse.

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

    // Meertalige knopteksten (sectie 14-backlogitem): zelfde aanpak als BackButtonCaptionByLanguage
    // op WizardScreenEditorViewModel, zie daar voor de volledige toelichting. Ook hier geen
    // Effective*-tegenhanger: ButtonPropertiesViewModel bouwt de per-taal-rijen zelf op.

    [ObservableProperty]
    private Dictionary<string, string> _browseButtonCaptionByLanguage;

    [ObservableProperty]
    private Dictionary<string, string> _browseButtonTooltipByLanguage;

    // Hergebruikt de kleurenkiezer van de basisklasse (WizardScreenEditorViewModel.PickColor,
    // protected static): geen eigen kopie nodig, deze klasse erft al van die basisklasse.
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

    /// <summary>True tenzij de Bladeren-knop expliciet op onzichtbaar gezet is.</summary>
    public bool IsBrowseButtonVisible => BrowseButtonVisible ?? true;

    /// <summary>True tenzij de Bladeren-knop expliciet op uitgeschakeld gezet is. Gebruikt de
    /// voorvertoning niet rechtstreeks — zie <see cref="IsBrowseButtonEnabledInPreview"/>, die dit
    /// combineert met Inno Setup's eigen ingebouwde gedrag.</summary>
    public bool IsBrowseButtonEnabled => BrowseButtonEnabled ?? true;

    /// <summary>Wat de voorvertoning daadwerkelijk als IsEnabled van de Bladeren-knop gebruikt:
    /// zowel Inno Setup's eigen ingebouwde gedrag (de knop gaat uit zodra deze pagina nooit getoond
    /// wordt, zie GroupPageMode/IsNeverShowHintVisible — bij AutoSkipIfKnown toont de voorvertoning
    /// bewust het eerste-installatie-scenario, dus bewerkbaar) als de knop-eigen Enabled-instelling
    /// moeten allebei "aan" staan.</summary>
    public bool IsBrowseButtonEnabledInPreview => GroupPageMode != DisablePageMode.NeverShow && IsBrowseButtonEnabled;

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
        CaptionByLanguage = new Dictionary<string, string>(BrowseButtonCaptionByLanguage),
        TooltipByLanguage = new Dictionary<string, string>(BrowseButtonTooltipByLanguage),
    };
}
