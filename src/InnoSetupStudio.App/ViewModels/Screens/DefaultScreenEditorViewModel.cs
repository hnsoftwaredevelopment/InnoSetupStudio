using CommunityToolkit.Mvvm.ComponentModel;
using InnoSetupStudio.App.Localization;
using InnoSetupStudio.Core.Project;

namespace InnoSetupStudio.App.ViewModels.Screens;

/// <summary>
/// Het Standaardscherm (§12.6/§12.7 van de architectuurdoc): geen echt installerscherm — de
/// eindgebruiker krijgt dit nooit te zien — maar een aparte, visueel gescheiden plek bovenaan de
/// linkerlijst van de schermeditor waar de gebruiker in één keer standaardwaarden voor de
/// Terug-/Volgende-/Annuleren-knop vastlegt. Elk scherm dat zelf niets voor een veld instelt (lege
/// Caption / null Enabled/Visible) neemt de waarde hiervandaan over; zie
/// <see cref="WizardScreenEditorViewModel"/>'s Effective*/Is*-eigenschappen voor de tweelaags-
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
        _nextButtonCaption = settings.NextButtonCaption;
        _nextButtonEnabled = settings.NextButtonEnabled;
        _nextButtonVisible = settings.NextButtonVisible;
        _cancelButtonCaption = settings.CancelButtonCaption;
        _cancelButtonEnabled = settings.CancelButtonEnabled;
        _cancelButtonVisible = settings.CancelButtonVisible;
    }

    /// <summary>Vertaalde naam, getoond in de linkerlijst (eigen rij boven de scheidingslijn).</summary>
    public string Title { get; } = LocalizationManager.Instance["WizardScreenDefault"];

    /// <summary>Iconsleutel uit Icons.xaml. Bewust een ander icoon dan de echte schermen
    /// (Document/Folder), zodat de rij ook visueel meteen als "anders" herkenbaar is.</summary>
    public string IconKey => "Edit";

    // Zelfde negen velden en zelfde leeg/null-is-nog-niet-aangepast-betekenis als op
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

    /// <summary>Tegenhanger van de constructor: leest de negen velden terug in een nieuwe
    /// <see cref="WizardScreenButtonSettings"/>, gebruikt door WizardEditorViewModel.ApplyTo.</summary>
    public WizardScreenButtonSettings ReadButtonSettings() => new()
    {
        BackButtonCaption = BackButtonCaption,
        BackButtonEnabled = BackButtonEnabled,
        BackButtonVisible = BackButtonVisible,
        NextButtonCaption = NextButtonCaption,
        NextButtonEnabled = NextButtonEnabled,
        NextButtonVisible = NextButtonVisible,
        CancelButtonCaption = CancelButtonCaption,
        CancelButtonEnabled = CancelButtonEnabled,
        CancelButtonVisible = CancelButtonVisible,
    };
}
