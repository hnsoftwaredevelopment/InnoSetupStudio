using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InnoSetupStudio.Core.Generation;
using InnoSetupStudio.Core.Project;

namespace InnoSetupStudio.App.ViewModels;

/// <summary>
/// Eén rij in de meertalige-vertalingenlijst onderaan het Knop-eigenschappenscherm (sectie
/// 14-backlogitem "meertalige knopteksten"): Caption/Tooltip-overschrijving voor precies één
/// niet-Engelse, geselecteerde taal van het project. Zelfde eenvoudige rij-object-aanpak als
/// LanguageRow (geen [ObservableProperty] op de bevattende ViewModel zelf), geabonneerd op
/// PropertyChanged om ButtonPropertiesViewModel.MarkDirty() aan te roepen.
/// </summary>
public sealed partial class LanguageOverrideRow : ObservableObject
{
    public LanguageOverrideRow(string languageId, string displayName, string caption, string tooltip)
    {
        LanguageId = languageId;
        DisplayName = displayName;
        _caption = caption;
        _tooltip = tooltip;
    }

    /// <summary>Taal-id uit InnoLanguageCatalog (bijvoorbeeld "dutch"), de sleutel waaronder deze
    /// overschrijving in BackButtonCaptionByLanguage e.d. terechtkomt.</summary>
    public string LanguageId { get; }

    /// <summary>Leesbare naam voor deze rij, zelfde Engelse eigennaam als InnoLanguageCatalog
    /// elders in de app gebruikt (zie LanguageRow/LanguagesViewModel).</summary>
    public string DisplayName { get; }

    [ObservableProperty]
    private string _caption;

    [ObservableProperty]
    private string _tooltip;

    /// <summary>Grijze voorinvulling van <see cref="Caption"/> zolang die leeg is: wat er voor deze
    /// taal geldt als de rij leeg blijft. Zie ButtonPropertiesViewModel.UpdateLanguagePlaceholders.</summary>
    [ObservableProperty]
    private string _captionPlaceholder = string.Empty;

    /// <summary>Zie <see cref="CaptionPlaceholder"/>, maar dan voor de tooltip.</summary>
    [ObservableProperty]
    private string _tooltipPlaceholder = string.Empty;
}

/// <summary>
/// Wat een knop op een gewoon scherm van het Standaardscherm kan erven: de universele tekst en
/// tooltip en hun vertalingen per taal. Alleen gebruikt voor de voorinvulling van de vertaalrijen in
/// het venster Knopeigenschappen (zie docs/Ontwerp-Vertalingen-Standaardscherm.md). Voor het
/// Standaardscherm zelf en voor de Bladeren-knoppen, die niets erven, is dat <see cref="None"/>.
/// </summary>
public sealed record InheritedTranslations(
    string Caption,
    IReadOnlyDictionary<string, string> CaptionByLanguage,
    string Tooltip,
    IReadOnlyDictionary<string, string> TooltipByLanguage)
{
    public static InheritedTranslations None { get; } = new(
        string.Empty, new Dictionary<string, string>(), string.Empty, new Dictionary<string, string>());
}

/// <summary>
/// ViewModel voor het Knop-eigenschappenscherm (backlogitem 3, sectie 17): één herbruikbaar
/// dialoogvenster voor de eigenschappen van een willekeurige knop (Terug/Volgende/Annuleren op elk
/// van de vier schermtypen, of de Bladerknop op het Bestemmingsscherm), in plaats van al die
/// eigenschappen rechtstreeks in het Schermeditor-paneel te tonen zoals tot nu toe (zie
/// backlogitem 3-mockup, properties.pdf). Bevat zelf geen enkele kennis van
/// WizardScreenEditorViewModel/DefaultScreenEditorViewModel/SelectDestinationPageEditorViewModel:
/// werkt als een "adapter" die per veld een get/set-delegatenpaar meekrijgt, zodat elk van die
/// drie klassen deze ene dialoog kan hergebruiken voor hun eigen knop-specifieke eigenschappen
/// (zie WizardEditorWindow.xaml.cs voor hoe de delegates per knop worden opgebouwd).
///
/// Werkt met een momentopname in plaats van live tweerichtingsbinding op de onderliggende
/// eigenschap: de get-delegates worden één keer aangeroepen bij het openen (in de constructor), de
/// set-delegates pas bij een geslaagde Opslaan. Dat is bewust dezelfde aanpak als
/// WizardEditorViewModel.ApplyTo (pas écht wegschrijven bij Opslaan; Annuleren/Sluiten gooit de
/// lokale wijzigingen dan gewoon weg) - zie DirtyTrackingViewModel voor de reden waarom dat
/// voldoende is om "Annuleren" zijn naam waar te laten maken, zonder dat hier een aparte
/// revert-per-veld nodig is.
/// </summary>
public sealed partial class ButtonPropertiesViewModel : DirtyTrackingViewModel
{
    private readonly Action<string> _setCaption;
    private readonly Action<bool?> _setEnabled;
    private readonly Action<bool?> _setVisible;
    private readonly Action<string> _setFontFamily;
    private readonly Action<int?> _setFontSize;
    private readonly Action<bool?> _setFontBold;
    private readonly Action<string> _setTooltip;
    private readonly Action<Dictionary<string, string>> _setCaptionByLanguage;
    private readonly Action<Dictionary<string, string>> _setTooltipByLanguage;

    // Bewaard om in Save() tegen te mergen (CodeRabbit, PR #21): LanguageOverrides bevat alleen
    // rijen voor de talen die BIJ HET OPENEN van dit scherm geselecteerd waren. Zonder deze
    // originelen zou Save() de hele dictionary herbouwen uit louter die rijen, en zo een
    // vertaling voor een taal die ná het invullen weer uitgevinkt is in de Talen-tab stilzwijgend wegschrijven
    // bij de eerstvolgende Opslaan van DIT scherm, ook als de gebruiker die taal helemaal niet
    // aanraakte. Zie MergeLanguageOverrides hieronder.
    private readonly Dictionary<string, string> _originalCaptionByLanguage;
    private readonly Dictionary<string, string> _originalTooltipByLanguage;
    private readonly InheritedTranslations _inherited;

    /// <summary>Gevuurd zodra Opslaan of Sluiten/Annuleren is gekozen; het venster (zie
    /// ButtonPropertiesWindow.xaml.cs) sluit zichzelf hierop met het meegegeven DialogResult,
    /// zelfde patroon als ProjectSettingsViewModel/WizardScreensViewModel.</summary>
    public event EventHandler<bool>? RequestClose;

    public ButtonPropertiesViewModel(
        string dialogTitle,
        bool hasCaption,
        string triStateHint,
        Func<string> getCaption, Action<string> setCaption, string effectiveCaption,
        Func<bool?> getEnabled, Action<bool?> setEnabled,
        Func<bool?> getVisible, Action<bool?> setVisible,
        Func<string> getFontFamily, Action<string> setFontFamily, string effectiveFontFamily,
        Func<int?> getFontSize, Action<int?> setFontSize, int? effectiveFontSize,
        Func<bool?> getFontBold, Action<bool?> setFontBold, bool? effectiveFontBold,
        Func<string> getTooltip, Action<string> setTooltip, string effectiveTooltip,
        IReadOnlyList<string> nonEnglishLanguageIds,
        Func<Dictionary<string, string>> getCaptionByLanguage, Action<Dictionary<string, string>> setCaptionByLanguage,
        Func<Dictionary<string, string>> getTooltipByLanguage, Action<Dictionary<string, string>> setTooltipByLanguage,
        string languageOverridesHint, InheritedTranslations inherited)
    {
        DialogTitle = dialogTitle;
        HasCaption = hasCaption;
        TriStateHint = triStateHint;
        _setCaption = setCaption;
        _setEnabled = setEnabled;
        _setVisible = setVisible;
        _setFontFamily = setFontFamily;
        _setFontSize = setFontSize;
        _setFontBold = setFontBold;
        _setTooltip = setTooltip;
        _setCaptionByLanguage = setCaptionByLanguage;
        _setTooltipByLanguage = setTooltipByLanguage;
        _inherited = inherited;
        LanguageOverridesHint = languageOverridesHint;

        EffectiveCaption = effectiveCaption;
        EffectiveFontFamily = effectiveFontFamily;
        EffectiveFontSize = effectiveFontSize;
        EffectiveFontBold = effectiveFontBold;
        EffectiveTooltip = effectiveTooltip;

        BeginInit();
        _caption = getCaption();
        _buttonEnabled = getEnabled();
        _buttonVisible = getVisible();
        _fontFamily = getFontFamily();
        _fontSize = getFontSize();
        _fontBold = getFontBold();
        _tooltip = getTooltip();

        // Meertalige knopteksten (sectie 14-backlogitem): één rij per niet-Engelse, geselecteerde
        // taal van het project, in InnoLanguageCatalog-volgorde (zelfde volgorde als de Talen-tab
        // in Projectinstellingen). De catalogus bepaalt de volgorde, nonEnglishLanguageIds alleen
        // welke talen meedoen — Where/IndexOf i.p.v. nonEnglishLanguageIds zelf doorlopen, zodat
        // een handmatig bewerkt projectbestand met talen in een afwijkende volgorde hier toch
        // netjes gesorteerd verschijnt, net als LanguagesViewModel dat al voor de Talen-tab doet.
        _originalCaptionByLanguage = new Dictionary<string, string>(getCaptionByLanguage());
        _originalTooltipByLanguage = new Dictionary<string, string>(getTooltipByLanguage());
        LanguageOverrides = InnoLanguageCatalog.Languages
            .Where(l => nonEnglishLanguageIds.Contains(l.Id))
            .Select(l => new LanguageOverrideRow(
                l.Id,
                l.DisplayName,
                _originalCaptionByLanguage.GetValueOrDefault(l.Id, string.Empty),
                _originalTooltipByLanguage.GetValueOrDefault(l.Id, string.Empty)))
            .ToList();

        UpdateLanguagePlaceholders();

        foreach (var row in LanguageOverrides)
        {
            row.PropertyChanged += (_, _) => MarkDirty();
        }

        EndInit();
    }

    public string DialogTitle { get; }

    /// <summary>Sinds 2026-09-29 altijd true: elke knop die dit scherm gebruikt (Terug/Volgende/
    /// Annuleren op elk schermtype, en de Bladerknop op het Bestemmingsscherm) heeft een eigen
    /// Caption. Blijft een los veld (in plaats van de View-binding te verwijderen) mocht een
    /// toekomstige knop ooit wél zonder Caption nodig zijn — zie BrowseButtonSettings voor de
    /// eerdere aanname dat de Bladerknop er geen zou hebben.</summary>
    public bool HasCaption { get; }

    /// <summary>Eén rij per niet-Engelse, geselecteerde taal van het project (sectie
    /// 14-backlogitem "meertalige knopteksten"), leeg voor een eentalig project. Opgebouwd in de
    /// constructor, zie daar voor de volgorde/herkomst.</summary>
    public IReadOnlyList<LanguageOverrideRow> LanguageOverrides { get; }

    /// <summary>True zodra er tenminste één rij in <see cref="LanguageOverrides"/> staat: bepaalt
    /// in ButtonPropertiesWindow.xaml of de hele sectie getoond wordt. Een eentalig project (het
    /// gebruikelijke geval) laat deze sectie dus gewoon weg, in plaats van een lege lijst te
    /// tonen.</summary>
    public bool HasLanguageOverrides => LanguageOverrides.Count > 0;

    /// <summary>Toelichting onder "Vertalingen per taal". Op een gewoon scherm noemt die ook het
    /// Standaardscherm, op het Standaardscherm en bij de Bladeren-knoppen niet (die erven niets).</summary>
    public string LanguageOverridesHint { get; }

    // Wat er voor een taal geldt als de rij leeg blijft (docs/Ontwerp-Vertalingen-Standaardscherm.md,
    // sectie 3): de vertaling van het Standaardscherm, tenzij dit scherm zelf een tekst heeft, en
    // anders die tekst, of de universele tekst van het Standaardscherm. Dezelfde regel als de
    // generator (ButtonSettingsResolver.ResolveTranslation). Leeg als er niets geldt: dan houdt
    // Setup zijn eigen tekst, die de studio niet per taal kent.
    private void UpdateLanguagePlaceholders()
    {
        foreach (var row in LanguageOverrides)
        {
            row.CaptionPlaceholder = PlaceholderFor(Caption, _inherited.Caption, _inherited.CaptionByLanguage, row.LanguageId);
            row.TooltipPlaceholder = PlaceholderFor(Tooltip, _inherited.Tooltip, _inherited.TooltipByLanguage, row.LanguageId);
        }
    }

    private static string PlaceholderFor(
        string ownText, string inheritedText, IReadOnlyDictionary<string, string> inheritedByLanguage, string languageId)
    {
        var translation = ButtonSettingsResolver.ResolveTranslation(
            ownText, null, inheritedByLanguage.GetValueOrDefault(languageId));
        if (translation.Length > 0)
        {
            return translation;
        }

        return !string.IsNullOrWhiteSpace(ownText) ? ownText : inheritedText;
    }

    /// <summary>Toelichting onder de Ingeschakeld/Zichtbaar-checkboxes, exact overgenomen van de
    /// aanroepende schermeditor-ViewModel (HintButtonTriStateText, of voor de Bladerknop
    /// rechtstreeks de sleutel HintButtonTriStateDefaultScreen): welke tekst hier klopt, hangt af
    /// van welk van de drie cascaderingsniveaus deze knop gebruikt (drielaags voor Terug/Volgende/
    /// Annuleren op een echt scherm, tweelaags-eigen voor diezelfde knoppen op het
    /// Standaardscherm, geen cascade voor de Bladerknop) - zie WizardEditorWindow.xaml.cs voor hoe
    /// dit per knop wordt bepaald.</summary>
    public string TriStateHint { get; }

    public string EffectiveCaption { get; }

    public string EffectiveFontFamily { get; }

    public int? EffectiveFontSize { get; }

    /// <summary>Zie <see cref="EffectiveFontSize"/>, maar dan voor vetgedrukt. CodeRabbit-
    /// opmerking op PR #17 (2026-09-28): zonder deze terugvalwaarde toonde de voorvertoning
    /// hieronder een inconsistent beeld met de echte voorvertoning in WizardEditorWindow.xaml -
    /// die laatste gebruikt wél EffectiveXxxButtonFontBold (drielaagse cascade), dus als het
    /// Standaardscherm op vet staat en dit scherm's eigen FontBold op null (onbepaald), toonde de
    /// echte voorvertoning vet terwijl deze dialoog gewone tekst toonde.</summary>
    public bool? EffectiveFontBold { get; }

    public string EffectiveTooltip { get; }

    /// <summary>Wat de voorvertoning onderaan daadwerkelijk als knoptekst toont: eigen tekst,
    /// anders de meegegeven terugvaltekst (Inno Setup's standaardtekst voor Terug/Volgende/
    /// Annuleren/Bladeren). Zelfde ResolveCaption-patroon als
    /// WizardScreenEditorViewModel/DefaultScreenEditorViewModel.</summary>
    public string PreviewCaption => ResolveEffective(Caption, EffectiveCaption);

    /// <summary>Wat de voorvertoning onderaan als knoptekst toont. Losstaand van
    /// <see cref="PreviewCaption"/> gehouden (in plaats van rechtstreeks daaraan te binden) voor
    /// het geval <see cref="HasCaption"/> ooit weer false wordt voor een toekomstige knop zonder
    /// Caption — zie <see cref="HasCaption"/>.</summary>
    public string PreviewButtonText => HasCaption ? PreviewCaption : "Browse...";

    /// <summary>Zie <see cref="PreviewCaption"/>, maar dan voor het lettertype van de
    /// voorvertoning.</summary>
    public string PreviewFontFamily => ResolveEffective(FontFamily, EffectiveFontFamily);

    /// <summary>Zie <see cref="PreviewCaption"/>, maar dan voor de lettergrootte van de
    /// voorvertoning.</summary>
    public int? PreviewFontSize => FontSize ?? EffectiveFontSize;

    /// <summary>Zie <see cref="PreviewFontSize"/>, maar dan voor vetgedrukt.</summary>
    public bool? PreviewFontBold => FontBold ?? EffectiveFontBold;

    /// <summary>Zie <see cref="PreviewCaption"/>, maar dan voor de tooltip van de
    /// voorvertoning.</summary>
    public string PreviewTooltip => ResolveEffective(Tooltip, EffectiveTooltip);

    private static string ResolveEffective(string own, string effective) =>
        !string.IsNullOrWhiteSpace(own) ? own : effective;

    [ObservableProperty]
    private string _caption = string.Empty;

    [ObservableProperty]
    private bool? _buttonEnabled;

    [ObservableProperty]
    private bool? _buttonVisible;

    [ObservableProperty]
    private string _fontFamily = string.Empty;

    [ObservableProperty]
    private int? _fontSize;

    [ObservableProperty]
    private bool? _fontBold;

    [ObservableProperty]
    private string _tooltip = string.Empty;

    // Zelfde alleen-witruimte-normalisatie/live-Effective*-doormelding als
    // WizardScreenEditorViewModel/DefaultScreenEditorViewModel (zie daar voor de reden); hier
    // Preview* genoemd in plaats van Effective*, en MarkDirty() erbij zodat de
    // Opslaan/Sluiten-Annuleren-knop (DirtyTrackingViewModel) meteen reageert op de eerste
    // wijziging.

    partial void OnCaptionChanged(string value)
    {
        NormalizeWhitespaceOnly(value, v => Caption = v);
        OnPropertyChanged(nameof(PreviewCaption));
        OnPropertyChanged(nameof(PreviewButtonText));
        UpdateLanguagePlaceholders();
        MarkDirty();
    }

    partial void OnButtonEnabledChanged(bool? value) => MarkDirty();

    partial void OnButtonVisibleChanged(bool? value) => MarkDirty();

    partial void OnFontFamilyChanged(string value)
    {
        NormalizeWhitespaceOnly(value, v => FontFamily = v);
        OnPropertyChanged(nameof(PreviewFontFamily));
        MarkDirty();
    }

    partial void OnFontSizeChanged(int? value)
    {
        OnPropertyChanged(nameof(PreviewFontSize));
        MarkDirty();
    }

    partial void OnFontBoldChanged(bool? value)
    {
        OnPropertyChanged(nameof(PreviewFontBold));
        MarkDirty();
    }

    partial void OnTooltipChanged(string value)
    {
        NormalizeWhitespaceOnly(value, v => Tooltip = v);
        OnPropertyChanged(nameof(PreviewTooltip));
        UpdateLanguagePlaceholders();
        MarkDirty();
    }

    private static void NormalizeWhitespaceOnly(string? value, Action<string> setter)
    {
        if (!string.IsNullOrEmpty(value) && string.IsNullOrWhiteSpace(value))
        {
            setter(string.Empty);
        }
    }

    [RelayCommand]
    private void Save()
    {
        _setCaption(Caption);
        _setEnabled(ButtonEnabled);
        _setVisible(ButtonVisible);
        _setFontFamily(FontFamily);
        _setFontSize(FontSize);
        _setFontBold(FontBold);
        _setTooltip(Tooltip);

        // Meertalige knopteksten (sectie 14-backlogitem): MergeLanguageOverrides tegen de
        // originelen in plaats van de dictionary volledig uit LanguageOverrides te herbouwen
        // (CodeRabbit, PR #21) - zie _originalCaptionByLanguage/_originalTooltipByLanguage
        // hierboven voor waarom. Zelfde leeg-is-onveranderd-conventie als de overige velden (zie
        // WizardScreenButtonSettings.BackButtonCaptionByLanguage): een leeggemaakte rij verwijdert
        // zijn sleutel, in plaats van er met een lege string in te blijven staan.
        _setCaptionByLanguage(MergeLanguageOverrides(_originalCaptionByLanguage, LanguageOverrides, row => row.Caption));
        _setTooltipByLanguage(MergeLanguageOverrides(_originalTooltipByLanguage, LanguageOverrides, row => row.Tooltip));

        RequestClose?.Invoke(this, true);
    }

    /// <summary>Past alleen de talen aan die als rij zichtbaar waren (<paramref name="rows"/>,
    /// zie <see cref="LanguageOverrides"/>) - elke andere sleutel in <paramref name="original"/>
    /// (een taal die intussen uitgevinkt is in de Talen-tab, of - op het Standaardscherm - elke
    /// taal, zie BuildForDefaultScreenButton) blijft ongewijzigd staan. Zie
    /// _originalCaptionByLanguage hierboven voor de reden.</summary>
    private static Dictionary<string, string> MergeLanguageOverrides(
        Dictionary<string, string> original, IReadOnlyList<LanguageOverrideRow> rows, Func<LanguageOverrideRow, string> selector)
    {
        var merged = new Dictionary<string, string>(original);

        foreach (var row in rows)
        {
            var value = selector(row);
            if (string.IsNullOrWhiteSpace(value))
            {
                merged.Remove(row.LanguageId);
            }
            else
            {
                merged[row.LanguageId] = value;
            }
        }

        return merged;
    }

    [RelayCommand]
    private void Cancel() => RequestClose?.Invoke(this, false);
}
