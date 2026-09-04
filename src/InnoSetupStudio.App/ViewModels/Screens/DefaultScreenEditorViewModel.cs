using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InnoSetupStudio.App.Localization;
using InnoSetupStudio.Core.Project;
using Microsoft.Win32;

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
    private readonly IProjectAssetService _assetService;
    private readonly string? _projectFilePath;

    public DefaultScreenEditorViewModel(WizardScreenButtonSettings settings, IProjectAssetService assetService, string? projectFilePath)
    {
        _assetService = assetService;
        _projectFilePath = projectFilePath;
        _backButtonCaption = settings.BackButtonCaption;
        _backButtonEnabled = settings.BackButtonEnabled;
        _backButtonVisible = settings.BackButtonVisible;
        _backButtonTextColor = settings.BackButtonTextColor;
        _backButtonBackgroundColor = settings.BackButtonBackgroundColor;
        _backButtonBitmapFilePath = settings.BackButtonBitmapFilePath;
        _nextButtonCaption = settings.NextButtonCaption;
        _nextButtonEnabled = settings.NextButtonEnabled;
        _nextButtonVisible = settings.NextButtonVisible;
        _nextButtonTextColor = settings.NextButtonTextColor;
        _nextButtonBackgroundColor = settings.NextButtonBackgroundColor;
        _nextButtonBitmapFilePath = settings.NextButtonBitmapFilePath;
        _cancelButtonCaption = settings.CancelButtonCaption;
        _cancelButtonEnabled = settings.CancelButtonEnabled;
        _cancelButtonVisible = settings.CancelButtonVisible;
        _cancelButtonTextColor = settings.CancelButtonTextColor;
        _cancelButtonBackgroundColor = settings.CancelButtonBackgroundColor;
        _cancelButtonBitmapFilePath = settings.CancelButtonBitmapFilePath;
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

    // Zelfde negen kleuren-/bitmapvelden als WizardScreenEditorViewModel (backlogitem 3, sectie
    // 14), ook hier zonder Effective*-resolutie: dit scherm ÍS de bron van de standaardwaarde.

    [ObservableProperty]
    private string _backButtonTextColor;

    [ObservableProperty]
    private string _backButtonBackgroundColor;

    [ObservableProperty]
    private string _backButtonBitmapFilePath;

    [ObservableProperty]
    private string _nextButtonTextColor;

    [ObservableProperty]
    private string _nextButtonBackgroundColor;

    [ObservableProperty]
    private string _nextButtonBitmapFilePath;

    [ObservableProperty]
    private string _cancelButtonTextColor;

    [ObservableProperty]
    private string _cancelButtonBackgroundColor;

    [ObservableProperty]
    private string _cancelButtonBitmapFilePath;

    // Zelfde Bladerknop-patroon als WizardScreenEditorViewModel.BrowseForBitmap hierboven; geen
    // gedeelde basisklasse (zie de klassencommentaar), dus hier een eigen, verder identieke kopie.

    [RelayCommand]
    private void BrowseBackButtonBitmap() => BackButtonBitmapFilePath = BrowseForBitmap(BackButtonBitmapFilePath);

    [RelayCommand]
    private void BrowseNextButtonBitmap() => NextButtonBitmapFilePath = BrowseForBitmap(NextButtonBitmapFilePath);

    [RelayCommand]
    private void BrowseCancelButtonBitmap() => CancelButtonBitmapFilePath = BrowseForBitmap(CancelButtonBitmapFilePath);

    private string BrowseForBitmap(string currentPath)
    {
        var dialog = new OpenFileDialog
        {
            Filter = LocalizationManager.Instance["DialogFilterImageFiles"],
        };

        if (!string.IsNullOrWhiteSpace(currentPath))
        {
            dialog.InitialDirectory = Path.GetDirectoryName(currentPath);
        }

        return dialog.ShowDialog() == true ? _assetService.Import(_projectFilePath, dialog.FileName) : currentPath;
    }

    /// <summary>Tegenhanger van de constructor: leest de negen velden terug in een nieuwe
    /// <see cref="WizardScreenButtonSettings"/>, gebruikt door WizardEditorViewModel.ApplyTo.</summary>
    public WizardScreenButtonSettings ReadButtonSettings() => new()
    {
        BackButtonCaption = BackButtonCaption,
        BackButtonEnabled = BackButtonEnabled,
        BackButtonVisible = BackButtonVisible,
        BackButtonTextColor = BackButtonTextColor,
        BackButtonBackgroundColor = BackButtonBackgroundColor,
        BackButtonBitmapFilePath = BackButtonBitmapFilePath,
        NextButtonCaption = NextButtonCaption,
        NextButtonEnabled = NextButtonEnabled,
        NextButtonVisible = NextButtonVisible,
        NextButtonTextColor = NextButtonTextColor,
        NextButtonBackgroundColor = NextButtonBackgroundColor,
        NextButtonBitmapFilePath = NextButtonBitmapFilePath,
        CancelButtonCaption = CancelButtonCaption,
        CancelButtonEnabled = CancelButtonEnabled,
        CancelButtonVisible = CancelButtonVisible,
        CancelButtonTextColor = CancelButtonTextColor,
        CancelButtonBackgroundColor = CancelButtonBackgroundColor,
        CancelButtonBitmapFilePath = CancelButtonBitmapFilePath,
    };
}
