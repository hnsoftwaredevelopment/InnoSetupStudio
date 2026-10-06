using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InnoSetupStudio.App.Localization;
using InnoSetupStudio.Core.Project;

namespace InnoSetupStudio.App.ViewModels.Screens;

/// <summary>
/// Basisklasse voor een enkel scherm in de schermeditor (fase 4). Eén instantie is tegelijk de
/// DataContext voor drie plekken: de rij in de linkerlijst van <see cref="WizardEditorViewModel"/>,
/// de voorvertoning in het midden (een DataTemplate in WizardEditorWindow kiest op basis van het
/// type de bijbehorende UserControl uit InnoSetupStudio.Wizard) en het instellingenpaneel rechts
/// (een DataTemplate die in InnoSetupStudio.App zelf staat, omdat die de LocalizationManager
/// gebruikt). <see cref="WizardEditorViewModel"/> abonneert zich op PropertyChanged van elke
/// instantie om zijn eigen dirty-status bij te houden, hetzelfde patroon als
/// <see cref="WizardScreensViewModel"/> gebruikt voor zijn rijen in fase 3.
/// </summary>
public abstract partial class WizardScreenEditorViewModel : ObservableObject
{
    protected WizardScreenEditorViewModel(string id, string title, string iconKey)
    {
        Id = id;
        Title = title;
        IconKey = iconKey;
    }

    /// <summary>Komt overeen met de bijbehorende Show*Page-eigenschap in WizardScreenSelection.</summary>
    public string Id { get; }

    /// <summary>Vertaalde naam, getoond in de linkerlijst.</summary>
    public string Title { get; }

    /// <summary>Iconsleutel uit Icons.xaml, getoond naast de naam in de linkerlijst.</summary>
    public string IconKey { get; }

    // Zelfde naam-zonder-gedeelde-basisklasse-patroon als Title/IconKey/EffectiveXxx: het
    // Standaardscherm heeft een eigen versie van deze twee hieronder (DefaultScreenEditorViewModel)
    // met andere tekst, want dat scherm kan niet van zichzelf erven — leeg/onbepaald daar valt
    // direct terug op Inno Setup's eigen standaard, niet op "het Standaardscherm". Gebonden in
    // plaats van {loc:Loc ...} in ButtonSettingsSectionTemplate, zodat die ene gedeelde template
    // voor alle vier de schermtypen de juiste tekst toont.

    /// <summary>Toelichting onder de drie knopvelden bij een lege Caption.</summary>
    public string HintButtonCaptionEmptyText => LocalizationManager.Instance["HintButtonCaptionEmpty"];

    /// <summary>Toelichting onder de drie knopvelden bij een onbepaalde (null) Enabled/Visible.</summary>
    public string HintButtonTriStateText => LocalizationManager.Instance["HintButtonTriState"];

    // De twee wizardafbeeldingen staan hier op de basisklasse (in plaats van alleen op de
    // schermen die ze nodig hebben) omdat het projectbrede instellingen zijn (Inno Setup's
    // WizardImageFile/WizardSmallImageFile), niet iets per scherm — zie DefaultScreenEditorViewModel,
    // dat sinds backlogitem 1 (sectie 14) de daadwerkelijke eigenaar is. Berekend via Defaults in
    // plaats van required init (zoals vóór backlogitem 1): de gebruiker kan de afbeelding nu
    // tijdens dezelfde schermeditor-sessie wijzigen op het Standaardscherm, dus dit moet live
    // meeveranderen in elk scherm dat het gebruikt — zelfde aanpak als EffectiveBackButtonCaption
    // hieronder, alleen zonder de drielaags-resolutie (er is geen "eigen waarde per scherm" voor
    // een projectbrede afbeelding om naar terug te vallen). RaiseEffectivePropertiesChanged
    // hieronder meldt de wijziging door.
    public ImageSource WizardImage => Defaults.WizardImage;

    /// <summary>Zie <see cref="WizardImage"/>, maar dan de kleine afbeelding rechtsboven.</summary>
    public ImageSource WizardSmallImage => Defaults.WizardSmallImage;

    /// <summary>
    /// Schrijfalleen init-eigenschap zodat WizardEditorViewModel de knopvelden hieronder in één
    /// keer kan meegeven via object-initializer-syntax (<c>new XPageEditorViewModel(...) {
    /// ButtonSettings = ... }</c>), in plaats van losse constructorparameters per veld.
    /// <see langword="required"/> zodat een nieuw schermtype dit nooit per ongeluk leeg laat staan.
    /// </summary>
    public required WizardScreenButtonSettings ButtonSettings
    {
        init
        {
            BackButtonCaption = value.BackButtonCaption;
            BackButtonEnabled = value.BackButtonEnabled;
            BackButtonVisible = value.BackButtonVisible;
            BackButtonFontFamily = value.BackButtonFontFamily;
            BackButtonFontSize = value.BackButtonFontSize;
            BackButtonFontBold = value.BackButtonFontBold;
            BackButtonTooltip = value.BackButtonTooltip;
            NextButtonCaption = value.NextButtonCaption;
            NextButtonEnabled = value.NextButtonEnabled;
            NextButtonVisible = value.NextButtonVisible;
            NextButtonFontFamily = value.NextButtonFontFamily;
            NextButtonFontSize = value.NextButtonFontSize;
            NextButtonFontBold = value.NextButtonFontBold;
            NextButtonTooltip = value.NextButtonTooltip;
            CancelButtonCaption = value.CancelButtonCaption;
            CancelButtonEnabled = value.CancelButtonEnabled;
            CancelButtonVisible = value.CancelButtonVisible;
            CancelButtonFontFamily = value.CancelButtonFontFamily;
            CancelButtonFontSize = value.CancelButtonFontSize;
            CancelButtonFontBold = value.CancelButtonFontBold;
            CancelButtonTooltip = value.CancelButtonTooltip;
            // Meertalige knopteksten (sectie 14-backlogitem, zie WizardScreenButtonSettings):
            // nieuwe dictionary-waarden kopiëren naar eigen veld, net als de tien velden
            // hierboven, niet rechtstreeks de instantie van value overnemen - zelfde reden als de
            // rest van dit blok (ButtonSettings is init-only, dit is de ene kopieerkans).
            BackButtonCaptionByLanguage = new Dictionary<string, string>(value.BackButtonCaptionByLanguage);
            BackButtonTooltipByLanguage = new Dictionary<string, string>(value.BackButtonTooltipByLanguage);
            NextButtonCaptionByLanguage = new Dictionary<string, string>(value.NextButtonCaptionByLanguage);
            NextButtonTooltipByLanguage = new Dictionary<string, string>(value.NextButtonTooltipByLanguage);
            CancelButtonCaptionByLanguage = new Dictionary<string, string>(value.CancelButtonCaptionByLanguage);
            CancelButtonTooltipByLanguage = new Dictionary<string, string>(value.CancelButtonTooltipByLanguage);
        }
    }

    /// <summary>
    /// De geselecteerde talen van het project, min Engels (zie InnoLanguageCatalog.EnglishId),
    /// in catalogusvolgorde. Required init, door WizardEditorViewModel aan elk scherm meegegeven
    /// vanuit InstallerProject.SupportedLanguageIds, net als <see cref="Defaults"/> hierboven.
    /// Gebruikt door ButtonPropertiesViewModel (via ScreenEditorControl.BuildForScreenButton) om
    /// de per-taal-vertalingsrijen op te bouwen - zie BackButtonCaptionByLanguage hierboven. Leeg
    /// voor een eentalig project, waarvoor die rijen dan ook niet getoond worden.
    /// </summary>
    public required IReadOnlyList<string> NonEnglishLanguageIds { get; init; }

    private DefaultScreenEditorViewModel? _defaults;

    /// <summary>
    /// Het Standaardscherm van dezelfde schermeditor-sessie (zie WizardEditorViewModel), de
    /// tweede laag van de Effective*/Is*-resolutie hieronder. Required init, net als WizardImage/
    /// WizardSmallImage, zodat een nieuw schermtype dit nooit vergeet. Anders dan die twee heeft
    /// dit wél een custom init-accessor: WizardEditorViewModel maakt één DefaultScreenEditorViewModel
    /// voor de hele sessie en geeft dezelfde (levende) instantie aan elk scherm door, dus een
    /// wijziging op het Standaardscherm moet híer meteen de afgeleide Effective*/Is*-waarden
    /// bijwerken — vandaar het abonneren op PropertyChanged in plaats van alleen de velden
    /// eenmalig te kopiëren (zoals ButtonSettings hierboven wél doet, want dat IS een kopie van
    /// het scherm-eigen, niet-cascaderende deel).
    /// </summary>
    public required DefaultScreenEditorViewModel Defaults
    {
        get => _defaults!;
        init
        {
            _defaults = value;
            value.PropertyChanged += (_, _) => RaiseEffectivePropertiesChanged();
        }
    }

    private void RaiseEffectivePropertiesChanged()
    {
        OnPropertyChanged(nameof(WizardImage));
        OnPropertyChanged(nameof(WizardSmallImage));
        OnPropertyChanged(nameof(EffectiveBackButtonCaption));
        OnPropertyChanged(nameof(EffectiveNextButtonCaption));
        OnPropertyChanged(nameof(EffectiveCancelButtonCaption));
        OnPropertyChanged(nameof(IsBackButtonVisible));
        OnPropertyChanged(nameof(IsNextButtonVisible));
        OnPropertyChanged(nameof(IsCancelButtonVisible));
        OnPropertyChanged(nameof(IsBackButtonEnabled));
        OnPropertyChanged(nameof(IsNextButtonEnabled));
        OnPropertyChanged(nameof(IsCancelButtonEnabled));
        OnPropertyChanged(nameof(EffectiveBackButtonFontFamily));
        OnPropertyChanged(nameof(EffectiveNextButtonFontFamily));
        OnPropertyChanged(nameof(EffectiveCancelButtonFontFamily));
        OnPropertyChanged(nameof(EffectiveBackButtonFontSize));
        OnPropertyChanged(nameof(EffectiveNextButtonFontSize));
        OnPropertyChanged(nameof(EffectiveCancelButtonFontSize));
        OnPropertyChanged(nameof(EffectiveBackButtonFontBold));
        OnPropertyChanged(nameof(EffectiveNextButtonFontBold));
        OnPropertyChanged(nameof(EffectiveCancelButtonFontBold));
        OnPropertyChanged(nameof(EffectiveBackButtonTooltip));
        OnPropertyChanged(nameof(EffectiveNextButtonTooltip));
        OnPropertyChanged(nameof(EffectiveCancelButtonTooltip));
    }

    // De velden hieronder staan, anders dan WizardImage/WizardSmallImage, wél op de
    // basisklasse als gewone (niet required init) [ObservableProperty]'s: dit zijn per-scherm
    // gegevens (elk scherm heeft zijn eigen WizardScreenButtonSettings, zie
    // WizardEditorViewModel), niet één projectbrede waarde die overal hetzelfde is. Lege
    // Caption/null Enabled/Visible betekenen "Inno Setup's eigen standaardgedrag", zie
    // WizardScreenButtonSettings; de Effective*-eigenschappen hieronder lossen dat leeg-is-
    // standaard-gedrag op voor de voorvertoning.

    [ObservableProperty]
    private string _backButtonCaption = string.Empty;

    [ObservableProperty]
    private bool? _backButtonEnabled;

    [ObservableProperty]
    private bool? _backButtonVisible;

    [ObservableProperty]
    private string _nextButtonCaption = string.Empty;

    [ObservableProperty]
    private bool? _nextButtonEnabled;

    [ObservableProperty]
    private bool? _nextButtonVisible;

    [ObservableProperty]
    private string _cancelButtonCaption = string.Empty;

    [ObservableProperty]
    private bool? _cancelButtonEnabled;

    [ObservableProperty]
    private bool? _cancelButtonVisible;

    // Lettertype (backlogitem 3, sectie 14 — herzien op 2026-09-04: achtergrondkleur
    // en bitmap zijn bewust geschrapt, zie WizardScreenButtonSettings). Zelfde leeg-is-nog-niet-
    // aangepast-conventie als Caption hierboven.

    [ObservableProperty]
    private string _backButtonFontFamily = string.Empty;

    [ObservableProperty]
    private int? _backButtonFontSize;

    [ObservableProperty]
    private bool? _backButtonFontBold;

    [ObservableProperty]
    private string _nextButtonFontFamily = string.Empty;

    [ObservableProperty]
    private int? _nextButtonFontSize;

    [ObservableProperty]
    private bool? _nextButtonFontBold;

    [ObservableProperty]
    private string _cancelButtonFontFamily = string.Empty;

    [ObservableProperty]
    private int? _cancelButtonFontSize;

    [ObservableProperty]
    private bool? _cancelButtonFontBold;

    // Tooltip (backlogitem 3, sectie 17, uit het Knop-eigenschappenscherm-mockup): TNewButton is
    // een gewone TControl-afstammeling, dus Hint/ShowHint werken net zo rechtstreeks als de
    // Font-eigenschappen (zie WizardScreenButtonSettings). Tweelaags net als FontFamily
    // hieronder: eigen tekst, anders het Standaardscherm — er bestaat geen "Inno-ingebouwde"
    // derde laag voor een tooltip.

    [ObservableProperty]
    private string _backButtonTooltip = string.Empty;

    [ObservableProperty]
    private string _nextButtonTooltip = string.Empty;

    [ObservableProperty]
    private string _cancelButtonTooltip = string.Empty;

    // Meertalige knopteksten (sectie 14-backlogitem): per-taal-overschrijvingen van Caption/
    // Tooltip hierboven, zie WizardScreenButtonSettings.BackButtonCaptionByLanguage voor de
    // volledige toelichting (sleutel = taal-id, Engels gebruikt altijd gewoon de velden
    // hierboven, geen cascade via het Standaardscherm). Gewone [ObservableProperty]'s, geen
    // Effective*-tegenhanger: ButtonPropertiesViewModel bouwt de per-taal-rijen zelf op (met
    // NonEnglishLanguageIds hierboven) en leest/schrijft deze dictionaries rechtstreeks via de
    // get/set-delegate-aanpak die ook de overige velden al gebruikt.

    [ObservableProperty]
    private Dictionary<string, string> _backButtonCaptionByLanguage = new();

    [ObservableProperty]
    private Dictionary<string, string> _backButtonTooltipByLanguage = new();

    [ObservableProperty]
    private Dictionary<string, string> _nextButtonCaptionByLanguage = new();

    [ObservableProperty]
    private Dictionary<string, string> _nextButtonTooltipByLanguage = new();

    [ObservableProperty]
    private Dictionary<string, string> _cancelButtonCaptionByLanguage = new();

    [ObservableProperty]
    private Dictionary<string, string> _cancelButtonTooltipByLanguage = new();

    /// <summary>Inno Setup's eigen standaardtekst voor de Terug-knop op dit scherm, gebruikt zolang
    /// <see cref="BackButtonCaption"/> leeg is. De schermeditor toont hier de studio's eigen
    /// UI-taal (net als de knoppen zelf al deden vóór dit veld bestond), niet Inno Setup's vaste
    /// Engelse standaardtekst — zie ScreenEditorPreviewDisclaimer. Virtual zodat een toekomstig
    /// scherm (bijvoorbeeld de Klaar-om-te-installeren-pagina, waar Inno Setup zelf al "Install"
    /// in plaats van "Next" toont) dit kan overschrijven.</summary>
    protected virtual string DefaultBackButtonCaption => LocalizationManager.Instance["ButtonWizardBack"];

    /// <summary>Zie <see cref="DefaultBackButtonCaption"/>, maar dan voor de Volgende-knop.</summary>
    protected virtual string DefaultNextButtonCaption => LocalizationManager.Instance["ButtonWizardNext"];

    /// <summary>Zie <see cref="DefaultBackButtonCaption"/>, maar dan voor de Annuleren-knop.</summary>
    protected virtual string DefaultCancelButtonCaption => LocalizationManager.Instance["ButtonWizardCancel"];

    // Drielaags-resolutie (§12.6/§12.7 van de architectuurdoc): eigen waarde op dit scherm, indien
    // ingevuld → anders de waarde van het Standaardscherm (Defaults), indien die op zijn beurt
    // ingevuld is → anders pas Inno Setup's eigen ingebouwde standaard (Default*ButtonCaption /
    // "true" voor Enabled/Visible). Vervangt de eerdere tweetraps EffectiveXxx/IsXxx-logica uit
    // PR #10, die alleen de eerste en de laatste laag kende.

    /// <summary>Wat de voorvertoning daadwerkelijk op de Terug-knop toont.</summary>
    public string EffectiveBackButtonCaption => ResolveCaption(BackButtonCaption, Defaults.BackButtonCaption, DefaultBackButtonCaption);

    /// <summary>Zie <see cref="EffectiveBackButtonCaption"/>, maar dan voor de Volgende-knop.</summary>
    public string EffectiveNextButtonCaption => ResolveCaption(NextButtonCaption, Defaults.NextButtonCaption, DefaultNextButtonCaption);

    /// <summary>Zie <see cref="EffectiveBackButtonCaption"/>, maar dan voor de Annuleren-knop.</summary>
    public string EffectiveCancelButtonCaption => ResolveCaption(CancelButtonCaption, Defaults.CancelButtonCaption, DefaultCancelButtonCaption);

    private static string ResolveCaption(string own, string fromDefaults, string builtIn) =>
        !string.IsNullOrWhiteSpace(own) ? own
        : !string.IsNullOrWhiteSpace(fromDefaults) ? fromDefaults
        : builtIn;

    // Zelfde drielaags-resolutie als EffectiveXxxCaption hierboven voor FontFamily
    // (ResolveCaption hergebruikt, "builtIn" is hier altijd een lege string: geen
    // lettertype instellen = Inno Setup's eigen knopuiterlijk). FontSize/FontBold zijn geen tekst,
    // dus die gebruiken gewone null-coalescing zonder ResolveCaption; er is geen derde laag omdat
    // Inno Setup's eigen standaard lettergrootte/vetgedrukt-status niet als waarde te bepalen valt
    // — null hier betekent gewoon "geen van beide lagen heeft iets ingesteld".

    /// <summary>Wat de voorvertoning daadwerkelijk als lettertype op de Terug-knop toont, leeg = geen
    /// override (Inno Setup's eigen lettertype blijft gelden).</summary>
    public string EffectiveBackButtonFontFamily => ResolveCaption(BackButtonFontFamily, Defaults.BackButtonFontFamily, string.Empty);

    /// <summary>Zie <see cref="EffectiveBackButtonFontFamily"/>, maar dan voor de Volgende-knop.</summary>
    public string EffectiveNextButtonFontFamily => ResolveCaption(NextButtonFontFamily, Defaults.NextButtonFontFamily, string.Empty);

    /// <summary>Zie <see cref="EffectiveBackButtonFontFamily"/>, maar dan voor de Annuleren-knop.</summary>
    public string EffectiveCancelButtonFontFamily => ResolveCaption(CancelButtonFontFamily, Defaults.CancelButtonFontFamily, string.Empty);

    /// <summary>Wat de voorvertoning daadwerkelijk als tooltip op de Terug-knop toont, leeg = geen
    /// tooltip. Tweelaags, geen "builtIn": Inno Setup heeft geen eigen standaardtooltip om naar
    /// terug te vallen.</summary>
    public string EffectiveBackButtonTooltip => ResolveCaption(BackButtonTooltip, Defaults.BackButtonTooltip, string.Empty);

    /// <summary>Zie <see cref="EffectiveBackButtonTooltip"/>, maar dan voor de Volgende-knop.</summary>
    public string EffectiveNextButtonTooltip => ResolveCaption(NextButtonTooltip, Defaults.NextButtonTooltip, string.Empty);

    /// <summary>Zie <see cref="EffectiveBackButtonTooltip"/>, maar dan voor de Annuleren-knop.</summary>
    public string EffectiveCancelButtonTooltip => ResolveCaption(CancelButtonTooltip, Defaults.CancelButtonTooltip, string.Empty);

    /// <summary>Zie <see cref="EffectiveBackButtonFontFamily"/>, maar dan voor de lettergrootte.</summary>
    public int? EffectiveBackButtonFontSize => BackButtonFontSize ?? Defaults.BackButtonFontSize;

    /// <summary>Zie <see cref="EffectiveBackButtonFontSize"/>, maar dan voor de Volgende-knop.</summary>
    public int? EffectiveNextButtonFontSize => NextButtonFontSize ?? Defaults.NextButtonFontSize;

    /// <summary>Zie <see cref="EffectiveBackButtonFontSize"/>, maar dan voor de Annuleren-knop.</summary>
    public int? EffectiveCancelButtonFontSize => CancelButtonFontSize ?? Defaults.CancelButtonFontSize;

    /// <summary>Zie <see cref="EffectiveBackButtonFontFamily"/>, maar dan voor vetgedrukt.</summary>
    public bool? EffectiveBackButtonFontBold => BackButtonFontBold ?? Defaults.BackButtonFontBold;

    /// <summary>Zie <see cref="EffectiveBackButtonFontBold"/>, maar dan voor de Volgende-knop.</summary>
    public bool? EffectiveNextButtonFontBold => NextButtonFontBold ?? Defaults.NextButtonFontBold;

    /// <summary>Zie <see cref="EffectiveBackButtonFontBold"/>, maar dan voor de Annuleren-knop.</summary>
    public bool? EffectiveCancelButtonFontBold => CancelButtonFontBold ?? Defaults.CancelButtonFontBold;

    /// <summary>True tenzij dit scherm, of anders het Standaardscherm, expliciet op onzichtbaar
    /// gezet is.</summary>
    public bool IsBackButtonVisible => BackButtonVisible ?? Defaults.BackButtonVisible ?? true;

    /// <summary>Zie <see cref="IsBackButtonVisible"/>, maar dan voor de Volgende-knop.</summary>
    public bool IsNextButtonVisible => NextButtonVisible ?? Defaults.NextButtonVisible ?? true;

    /// <summary>Zie <see cref="IsBackButtonVisible"/>, maar dan voor de Annuleren-knop.</summary>
    public bool IsCancelButtonVisible => CancelButtonVisible ?? Defaults.CancelButtonVisible ?? true;

    /// <summary>True tenzij dit scherm, of anders het Standaardscherm, expliciet op uitgeschakeld
    /// gezet is. Bepaalt in de voorvertoning alleen het gedimde uiterlijk (Opacity), niet de
    /// daadwerkelijke IsEnabled van de Terug/Volgende-knoppen: die blijven altijd echt klikbaar,
    /// want ze zijn ook de navigatie van de schermeditor zelf (zie
    /// WizardEditorViewModel.Back/Next). De Annuleren-knop in de voorvertoning heeft geen eigen
    /// functie en gebruikt dit wél als echte IsEnabled.</summary>
    public bool IsBackButtonEnabled => BackButtonEnabled ?? Defaults.BackButtonEnabled ?? true;

    /// <summary>Zie <see cref="IsBackButtonEnabled"/>, maar dan voor de Volgende-knop.</summary>
    public bool IsNextButtonEnabled => NextButtonEnabled ?? Defaults.NextButtonEnabled ?? true;

    /// <summary>Zie <see cref="IsBackButtonEnabled"/>, maar dan voor de Annuleren-knop.</summary>
    public bool IsCancelButtonEnabled => CancelButtonEnabled ?? Defaults.CancelButtonEnabled ?? true;

    // Alleen-witruimte-invoer terugbrengen naar leeg (Herberts melding, 2026-09-28): ResolveCaption/
    // EffectiveXxx behandelden zo'n waarde al als "niet ingevuld" en toonden dan de spooktekst
    // eroverheen, maar de TextBox zelf hield de echte spaties vast. Dat gaf een verwarrende
    // invoegcursor middenin de spooktekst (bv. "Vo|lgende >" i.p.v. aan het begin) zodra je erin
    // klikte, want de spaties waren nog altijd echte, klikbare tekst. Roept de generated setter
    // opnieuw aan (leeg voldoet niet meer aan de voorwaarde), dus geen oneindige lus.
    private static void NormalizeWhitespaceOnly(string? value, Action<string> setter)
    {
        if (!string.IsNullOrEmpty(value) && string.IsNullOrWhiteSpace(value))
        {
            setter(string.Empty);
        }
    }

    partial void OnBackButtonCaptionChanged(string value)
    {
        NormalizeWhitespaceOnly(value, v => BackButtonCaption = v);
        OnPropertyChanged(nameof(EffectiveBackButtonCaption));
    }

    partial void OnNextButtonCaptionChanged(string value)
    {
        NormalizeWhitespaceOnly(value, v => NextButtonCaption = v);
        OnPropertyChanged(nameof(EffectiveNextButtonCaption));
    }

    partial void OnCancelButtonCaptionChanged(string value)
    {
        NormalizeWhitespaceOnly(value, v => CancelButtonCaption = v);
        OnPropertyChanged(nameof(EffectiveCancelButtonCaption));
    }

    partial void OnBackButtonVisibleChanged(bool? value) => OnPropertyChanged(nameof(IsBackButtonVisible));

    partial void OnNextButtonVisibleChanged(bool? value) => OnPropertyChanged(nameof(IsNextButtonVisible));

    partial void OnCancelButtonVisibleChanged(bool? value) => OnPropertyChanged(nameof(IsCancelButtonVisible));

    partial void OnBackButtonEnabledChanged(bool? value) => OnPropertyChanged(nameof(IsBackButtonEnabled));

    partial void OnNextButtonEnabledChanged(bool? value) => OnPropertyChanged(nameof(IsNextButtonEnabled));

    partial void OnCancelButtonEnabledChanged(bool? value) => OnPropertyChanged(nameof(IsCancelButtonEnabled));

    partial void OnBackButtonFontFamilyChanged(string value)
    {
        NormalizeWhitespaceOnly(value, v => BackButtonFontFamily = v);
        OnPropertyChanged(nameof(EffectiveBackButtonFontFamily));
    }

    partial void OnNextButtonFontFamilyChanged(string value)
    {
        NormalizeWhitespaceOnly(value, v => NextButtonFontFamily = v);
        OnPropertyChanged(nameof(EffectiveNextButtonFontFamily));
    }

    partial void OnCancelButtonFontFamilyChanged(string value)
    {
        NormalizeWhitespaceOnly(value, v => CancelButtonFontFamily = v);
        OnPropertyChanged(nameof(EffectiveCancelButtonFontFamily));
    }

    partial void OnBackButtonTooltipChanged(string value)
    {
        NormalizeWhitespaceOnly(value, v => BackButtonTooltip = v);
        OnPropertyChanged(nameof(EffectiveBackButtonTooltip));
    }

    partial void OnNextButtonTooltipChanged(string value)
    {
        NormalizeWhitespaceOnly(value, v => NextButtonTooltip = v);
        OnPropertyChanged(nameof(EffectiveNextButtonTooltip));
    }

    partial void OnCancelButtonTooltipChanged(string value)
    {
        NormalizeWhitespaceOnly(value, v => CancelButtonTooltip = v);
        OnPropertyChanged(nameof(EffectiveCancelButtonTooltip));
    }

    partial void OnBackButtonFontSizeChanged(int? value) => OnPropertyChanged(nameof(EffectiveBackButtonFontSize));

    partial void OnNextButtonFontSizeChanged(int? value) => OnPropertyChanged(nameof(EffectiveNextButtonFontSize));

    partial void OnCancelButtonFontSizeChanged(int? value) => OnPropertyChanged(nameof(EffectiveCancelButtonFontSize));

    partial void OnBackButtonFontBoldChanged(bool? value) => OnPropertyChanged(nameof(EffectiveBackButtonFontBold));

    partial void OnNextButtonFontBoldChanged(bool? value) => OnPropertyChanged(nameof(EffectiveNextButtonFontBold));

    partial void OnCancelButtonFontBoldChanged(bool? value) => OnPropertyChanged(nameof(EffectiveCancelButtonFontBold));

    /// <summary>Tegenhanger van de <see cref="ButtonSettings"/>-init-eigenschap: leest de velden
    /// terug in een nieuwe <see cref="WizardScreenButtonSettings"/>, gebruikt door
    /// WizardEditorViewModel.ApplyTo.</summary>
    public WizardScreenButtonSettings ReadButtonSettings() => new()
    {
        BackButtonCaption = BackButtonCaption,
        BackButtonEnabled = BackButtonEnabled,
        BackButtonVisible = BackButtonVisible,
        BackButtonFontFamily = BackButtonFontFamily,
        BackButtonFontSize = BackButtonFontSize,
        BackButtonFontBold = BackButtonFontBold,
        BackButtonTooltip = BackButtonTooltip,
        NextButtonCaption = NextButtonCaption,
        NextButtonEnabled = NextButtonEnabled,
        NextButtonVisible = NextButtonVisible,
        NextButtonFontFamily = NextButtonFontFamily,
        NextButtonFontSize = NextButtonFontSize,
        NextButtonFontBold = NextButtonFontBold,
        NextButtonTooltip = NextButtonTooltip,
        CancelButtonCaption = CancelButtonCaption,
        CancelButtonEnabled = CancelButtonEnabled,
        CancelButtonVisible = CancelButtonVisible,
        CancelButtonFontFamily = CancelButtonFontFamily,
        CancelButtonFontSize = CancelButtonFontSize,
        CancelButtonFontBold = CancelButtonFontBold,
        CancelButtonTooltip = CancelButtonTooltip,
        BackButtonCaptionByLanguage = new Dictionary<string, string>(BackButtonCaptionByLanguage),
        BackButtonTooltipByLanguage = new Dictionary<string, string>(BackButtonTooltipByLanguage),
        NextButtonCaptionByLanguage = new Dictionary<string, string>(NextButtonCaptionByLanguage),
        NextButtonTooltipByLanguage = new Dictionary<string, string>(NextButtonTooltipByLanguage),
        CancelButtonCaptionByLanguage = new Dictionary<string, string>(CancelButtonCaptionByLanguage),
        CancelButtonTooltipByLanguage = new Dictionary<string, string>(CancelButtonTooltipByLanguage),
    };
}
