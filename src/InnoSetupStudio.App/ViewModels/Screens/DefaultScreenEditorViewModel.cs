using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InnoSetupStudio.App.Localization;
using InnoSetupStudio.Core.Project;

namespace InnoSetupStudio.App.ViewModels.Screens;

/// <summary>
/// Het Standaardscherm (§12.6/§12.7 van de architectuurdoc): geen echt installerscherm — de
/// eindgebruiker krijgt dit nooit te zien — maar een aparte, visueel gescheiden plek bovenaan de
/// linkerlijst van de schermeditor waar de gebruiker in één keer standaardwaarden voor de
/// Terug-/Volgende-/Annuleren-knop vastlegt. Elk scherm dat zelf niets voor een veld instelt (lege
/// Caption / null Enabled/Visible) neemt de waarde hiervandaan over; zie
/// <see cref="WizardScreenEditorViewModel"/>'s Effective*/Is*-eigenschappen voor de drielaags-
/// resolutie (eigen waarde → deze standaardwaarde → Inno Setup's eigen ingebouwde standaard).
///
/// Erft bewust NIET van <see cref="WizardScreenEditorViewModel"/>: die basisklasse vraagt om
/// WizardImage/WizardSmallImage voor een live installervoorvertoning, en het Standaardscherm heeft
/// (nog) geen voorvertoning — §12.6 liet die vraag open, "geen voorvertoning" is voorlopig de
/// eenvoudigste van de twee genoemde opties. WizardEditorWindow.xaml toont in plaats daarvan een
/// toelichtende tekst wanneer dit scherm geselecteerd is.
/// </summary>
public sealed partial class DefaultScreenEditorViewModel : ObservableObject
{
    public DefaultScreenEditorViewModel(WizardScreenButtonSettings settings)
    {
        _backButtonCaption = settings.BackButtonCaption;
        _backButtonEnabled = settings.BackButtonEnabled;
        _backButtonVisible = settings.BackButtonVisible;
        _backButtonTextColor = settings.BackButtonTextColor;
        _backButtonFontFamily = settings.BackButtonFontFamily;
        _backButtonFontSize = settings.BackButtonFontSize;
        _backButtonFontBold = settings.BackButtonFontBold;
        _nextButtonCaption = settings.NextButtonCaption;
        _nextButtonEnabled = settings.NextButtonEnabled;
        _nextButtonVisible = settings.NextButtonVisible;
        _nextButtonTextColor = settings.NextButtonTextColor;
        _nextButtonFontFamily = settings.NextButtonFontFamily;
        _nextButtonFontSize = settings.NextButtonFontSize;
        _nextButtonFontBold = settings.NextButtonFontBold;
        _cancelButtonCaption = settings.CancelButtonCaption;
        _cancelButtonEnabled = settings.CancelButtonEnabled;
        _cancelButtonVisible = settings.CancelButtonVisible;
        _cancelButtonTextColor = settings.CancelButtonTextColor;
        _cancelButtonFontFamily = settings.CancelButtonFontFamily;
        _cancelButtonFontSize = settings.CancelButtonFontSize;
        _cancelButtonFontBold = settings.CancelButtonFontBold;
    }

    /// <summary>Vertaalde naam, getoond in de linkerlijst (eigen rij boven de scheidingslijn).</summary>
    public string Title { get; } = LocalizationManager.Instance["WizardScreenDefault"];

    /// <summary>Iconsleutel uit Icons.xaml. Bewust een ander icoon dan de echte schermen
    /// (Document/Folder), zodat de rij ook visueel meteen als "anders" herkenbaar is.</summary>
    public string IconKey => "Edit";

    // Eigen versie van WizardScreenEditorViewModel.HintButtonCaptionEmptyText/HintButtonTriStateText
    // (zelfde naam, geen gedeelde basisklasse — zie dat commentaar): dit scherm ÍS het
    // Standaardscherm, dus "neemt de waarde van het Standaardscherm over" zou hier onzin zijn. Een
    // lege/onbepaalde waarde hier valt direct terug op Inno Setup's eigen standaard.

    /// <summary>Toelichting onder de drie knopvelden bij een lege Caption.</summary>
    public string HintButtonCaptionEmptyText => LocalizationManager.Instance["HintButtonCaptionEmptyDefaultScreen"];

    /// <summary>Toelichting onder de drie knopvelden bij een onbepaalde (null) Enabled/Visible.</summary>
    public string HintButtonTriStateText => LocalizationManager.Instance["HintButtonTriStateDefaultScreen"];

    // Zelfde velden en zelfde leeg/null-is-nog-niet-aangepast-betekenis als op
    // WizardScreenEditorViewModel, maar dan zonder de Effective*/Is*-resolutie: dit scherm ÍS de
    // bron van de standaardwaarde, het lost er zelf geen op (er is geen "standaard-standaard" om
    // naar terug te vallen, alleen Inno Setup's eigen ingebouwde tekst, en die kent alleen de
    // schermen die er daadwerkelijk naar verwijzen — zie WizardScreenEditorViewModel).

    [ObservableProperty]
    private string _backButtonCaption;

    [ObservableProperty]
    private bool? _backButtonEnabled;

    [ObservableProperty]
    private bool? _backButtonVisible;

    [ObservableProperty]
    private string _nextButtonCaption;

    [ObservableProperty]
    private bool? _nextButtonEnabled;

    [ObservableProperty]
    private bool? _nextButtonVisible;

    [ObservableProperty]
    private string _cancelButtonCaption;

    [ObservableProperty]
    private bool? _cancelButtonEnabled;

    [ObservableProperty]
    private bool? _cancelButtonVisible;

    // Zelfde tekstkleur-/lettertypevelden als WizardScreenEditorViewModel (backlogitem 3, sectie
    // 14), ook hier zonder Effective*-resolutie: dit scherm ÍS de bron van de standaardwaarde.
    // Achtergrondkleur en bitmap zijn bewust niet opgenomen (zie WizardScreenButtonSettings).

    [ObservableProperty]
    private string _backButtonTextColor;

    [ObservableProperty]
    private string _backButtonFontFamily;

    [ObservableProperty]
    private int? _backButtonFontSize;

    [ObservableProperty]
    private bool? _backButtonFontBold;

    [ObservableProperty]
    private string _nextButtonTextColor;

    [ObservableProperty]
    private string _nextButtonFontFamily;

    [ObservableProperty]
    private int? _nextButtonFontSize;

    [ObservableProperty]
    private bool? _nextButtonFontBold;

    [ObservableProperty]
    private string _cancelButtonTextColor;

    [ObservableProperty]
    private string _cancelButtonFontFamily;

    [ObservableProperty]
    private int? _cancelButtonFontSize;

    [ObservableProperty]
    private bool? _cancelButtonFontBold;

    // Zelfde kleurenkiezer als WizardScreenEditorViewModel.PickColor (zie daar voor de reden:
    // Herberts feedback 2026-09-04 over foutgevoelige hex-invoer); geen gedeelde basisklasse (zie
    // de klassencommentaar), dus hier een eigen, verder identieke kopie.

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
    private void PickBackButtonTextColor() => BackButtonTextColor = PickColor(BackButtonTextColor);

    [RelayCommand]
    private void PickNextButtonTextColor() => NextButtonTextColor = PickColor(NextButtonTextColor);

    [RelayCommand]
    private void PickCancelButtonTextColor() => CancelButtonTextColor = PickColor(CancelButtonTextColor);

    /// <summary>Tegenhanger van de constructor: leest de velden terug in een nieuwe
    /// <see cref="WizardScreenButtonSettings"/>, gebruikt door WizardEditorViewModel.ApplyTo.</summary>
    public WizardScreenButtonSettings ReadButtonSettings() => new()
    {
        BackButtonCaption = BackButtonCaption,
        BackButtonEnabled = BackButtonEnabled,
        BackButtonVisible = BackButtonVisible,
        BackButtonTextColor = BackButtonTextColor,
        BackButtonFontFamily = BackButtonFontFamily,
        BackButtonFontSize = BackButtonFontSize,
        BackButtonFontBold = BackButtonFontBold,
        NextButtonCaption = NextButtonCaption,
        NextButtonEnabled = NextButtonEnabled,
        NextButtonVisible = NextButtonVisible,
        NextButtonTextColor = NextButtonTextColor,
        NextButtonFontFamily = NextButtonFontFamily,
        NextButtonFontSize = NextButtonFontSize,
        NextButtonFontBold = NextButtonFontBold,
        CancelButtonCaption = CancelButtonCaption,
        CancelButtonEnabled = CancelButtonEnabled,
        CancelButtonVisible = CancelButtonVisible,
        CancelButtonTextColor = CancelButtonTextColor,
        CancelButtonFontFamily = CancelButtonFontFamily,
        CancelButtonFontSize = CancelButtonFontSize,
        CancelButtonFontBold = CancelButtonFontBold,
    };
}
