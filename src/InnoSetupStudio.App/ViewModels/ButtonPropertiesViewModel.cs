using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace InnoSetupStudio.App.ViewModels;

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
    private readonly Action<string> _setTextColor;
    private readonly Action<string> _setFontFamily;
    private readonly Action<int?> _setFontSize;
    private readonly Action<bool?> _setFontBold;
    private readonly Action<string> _setTooltip;

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
        Func<string> getTextColor, Action<string> setTextColor, string effectiveTextColor,
        Func<string> getFontFamily, Action<string> setFontFamily, string effectiveFontFamily,
        Func<int?> getFontSize, Action<int?> setFontSize, int? effectiveFontSize,
        Func<bool?> getFontBold, Action<bool?> setFontBold,
        Func<string> getTooltip, Action<string> setTooltip, string effectiveTooltip)
    {
        DialogTitle = dialogTitle;
        HasCaption = hasCaption;
        TriStateHint = triStateHint;
        _setCaption = setCaption;
        _setEnabled = setEnabled;
        _setVisible = setVisible;
        _setTextColor = setTextColor;
        _setFontFamily = setFontFamily;
        _setFontSize = setFontSize;
        _setFontBold = setFontBold;
        _setTooltip = setTooltip;

        EffectiveCaption = effectiveCaption;
        EffectiveTextColor = effectiveTextColor;
        EffectiveFontFamily = effectiveFontFamily;
        EffectiveFontSize = effectiveFontSize;
        EffectiveTooltip = effectiveTooltip;

        BeginInit();
        _caption = getCaption();
        _buttonEnabled = getEnabled();
        _buttonVisible = getVisible();
        _textColor = getTextColor();
        _fontFamily = getFontFamily();
        _fontSize = getFontSize();
        _fontBold = getFontBold();
        _tooltip = getTooltip();
        EndInit();
    }

    public string DialogTitle { get; }

    /// <summary>False voor de Bladerknop (SelectDestinationPageEditorViewModel): die heeft geen
    /// Caption-eigenschap (Herbert heeft dat veld destijds bewust niet gevraagd, zie
    /// BrowseButtonSettings), dus de View verbergt het Knoptekst-veld dan volledig.</summary>
    public bool HasCaption { get; }

    /// <summary>Toelichting onder de Ingeschakeld/Zichtbaar-checkboxes, exact overgenomen van de
    /// aanroepende schermeditor-ViewModel (HintButtonTriStateText, of voor de Bladerknop
    /// rechtstreeks de sleutel HintButtonTriStateDefaultScreen): welke tekst hier klopt, hangt af
    /// van welk van de drie cascaderingsniveaus deze knop gebruikt (drielaags voor Terug/Volgende/
    /// Annuleren op een echt scherm, tweelaags-eigen voor diezelfde knoppen op het
    /// Standaardscherm, geen cascade voor de Bladerknop) - zie WizardEditorWindow.xaml.cs voor hoe
    /// dit per knop wordt bepaald.</summary>
    public string TriStateHint { get; }

    public string EffectiveCaption { get; }

    public string EffectiveTextColor { get; }

    public string EffectiveFontFamily { get; }

    public int? EffectiveFontSize { get; }

    public string EffectiveTooltip { get; }

    /// <summary>Wat de voorvertoning onderaan daadwerkelijk als knoptekst toont: eigen tekst,
    /// anders de meegegeven terugvaltekst (Inno Setup's standaardtekst voor Terug/Volgende/
    /// Annuleren, of leeg bij de Bladerknop, die geen Caption heeft). Zelfde ResolveCaption-patroon
    /// als WizardScreenEditorViewModel/DefaultScreenEditorViewModel.</summary>
    public string PreviewCaption => ResolveEffective(Caption, EffectiveCaption);

    /// <summary>Wat de voorvertoning onderaan als knoptekst toont wanneer <see cref="HasCaption"/>
    /// false is (de Bladerknop): dezelfde letterlijke, niet-vertaalde tekst ("Browse...") als de
    /// echte voorvertoning in InnoSetupStudio.Wizard/Screens/SelectDestinationPagePreview.xaml -
    /// dat is Inno Setup's eigen knoptekst, geen tekst van deze app zelf, dus bewust niet via
    /// LocalizationManager.</summary>
    public string PreviewButtonText => HasCaption ? PreviewCaption : "Browse...";

    /// <summary>Zie <see cref="PreviewCaption"/>, maar dan voor de tekstkleur van de
    /// voorvertoning.</summary>
    public string PreviewTextColor => ResolveEffective(TextColor, EffectiveTextColor);

    /// <summary>Zie <see cref="PreviewCaption"/>, maar dan voor het lettertype van de
    /// voorvertoning.</summary>
    public string PreviewFontFamily => ResolveEffective(FontFamily, EffectiveFontFamily);

    /// <summary>Zie <see cref="PreviewCaption"/>, maar dan voor de lettergrootte van de
    /// voorvertoning.</summary>
    public int? PreviewFontSize => FontSize ?? EffectiveFontSize;

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
    private string _textColor = string.Empty;

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
        MarkDirty();
    }

    partial void OnButtonEnabledChanged(bool? value) => MarkDirty();

    partial void OnButtonVisibleChanged(bool? value) => MarkDirty();

    partial void OnTextColorChanged(string value)
    {
        NormalizeWhitespaceOnly(value, v => TextColor = v);
        OnPropertyChanged(nameof(PreviewTextColor));
        MarkDirty();
    }

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

    partial void OnFontBoldChanged(bool? value) => MarkDirty();

    partial void OnTooltipChanged(string value)
    {
        NormalizeWhitespaceOnly(value, v => Tooltip = v);
        OnPropertyChanged(nameof(PreviewTooltip));
        MarkDirty();
    }

    private static void NormalizeWhitespaceOnly(string? value, Action<string> setter)
    {
        if (!string.IsNullOrEmpty(value) && string.IsNullOrWhiteSpace(value))
        {
            setter(string.Empty);
        }
    }

    // Zelfde kleurenkiezer als WizardScreenEditorViewModel.PickColor/DefaultScreenEditorViewModel
    // (zie daar voor de reden: Herberts feedback 2026-09-04 over foutgevoelige hex-invoer); geen
    // gedeelde basisklasse (deze klasse erft van DirtyTrackingViewModel, niet van
    // WizardScreenEditorViewModel), dus hier een eigen, verder identieke kopie - zelfde patroon
    // dat DefaultScreenEditorViewModel al toepast.
    private static string PickColor(string currentHex)
    {
        using var dialog = new System.Windows.Forms.ColorDialog { FullOpen = true };

        if (!string.IsNullOrWhiteSpace(currentHex))
        {
            try
            {
                if (ColorConverter.ConvertFromString(currentHex) is Color current)
                {
                    dialog.Color = System.Drawing.Color.FromArgb(current.A, current.R, current.G, current.B);
                }
            }
            catch (FormatException)
            {
                // Huidige waarde is (nog) geen geldige hex-kleur: dialoog opent dan gewoon met
                // zijn eigen standaardkleur, geen crash.
            }
        }

        if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
        {
            return currentHex;
        }

        var picked = dialog.Color;
        return $"#{picked.R:X2}{picked.G:X2}{picked.B:X2}";
    }

    [RelayCommand]
    private void PickTextColor() => TextColor = PickColor(TextColor);

    [RelayCommand]
    private void Save()
    {
        _setCaption(Caption);
        _setEnabled(ButtonEnabled);
        _setVisible(ButtonVisible);
        _setTextColor(TextColor);
        _setFontFamily(FontFamily);
        _setFontSize(FontSize);
        _setFontBold(FontBold);
        _setTooltip(Tooltip);
        RequestClose?.Invoke(this, true);
    }

    [RelayCommand]
    private void Cancel() => RequestClose?.Invoke(this, false);
}
