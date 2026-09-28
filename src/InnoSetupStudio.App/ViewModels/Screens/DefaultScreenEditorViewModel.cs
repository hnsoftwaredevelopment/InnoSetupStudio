using System.IO;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InnoSetupStudio.App.Localization;
using InnoSetupStudio.App.Services;
using InnoSetupStudio.Core.Project;
using Microsoft.Win32;

namespace InnoSetupStudio.App.ViewModels.Screens;

/// <summary>
/// Het Standaardscherm (§12.6/§12.7 van de architectuurdoc): geen echt installerscherm — de
/// eindgebruiker krijgt dit nooit te zien — maar een aparte, visueel gescheiden plek bovenaan de
/// linkerlijst van de schermeditor waar de gebruiker in één keer standaardwaarden voor de
/// Terug-/Volgende-/Annuleren-knop vastlegt, en sinds backlogitem 1 (sectie 14) ook de twee
/// wizardafbeeldingen. Elk scherm dat zelf niets voor een knopveld instelt (lege Caption / null
/// Enabled/Visible) neemt de waarde hiervandaan over; zie <see cref="WizardScreenEditorViewModel"/>'s
/// Effective*/Is*-eigenschappen voor de drielaags-resolutie (eigen waarde → deze standaardwaarde →
/// Inno Setup's eigen ingebouwde standaard). De wizardafbeeldingen kennen die drielaags-resolutie
/// niet — Inno Setup's <c>WizardImageFile</c>/<c>WizardSmallImageFile</c> zijn altijd projectbreed
/// (§12.6), dus hier is geen "eigen waarde per scherm" mogelijk om naar terug te vallen; elk echt
/// scherm leest <see cref="WizardImage"/>/<see cref="WizardSmallImage"/> rechtstreeks van hier via
/// <see cref="WizardScreenEditorViewModel.Defaults"/>.
///
/// Erft bewust NIET van <see cref="WizardScreenEditorViewModel"/>: die basisklasse heeft de
/// Effective*/Is*-knopresolutie die hier niet van toepassing is (dit scherm ÍS de bron van de
/// standaardwaarde, het lost er zelf geen op), en het Standaardscherm heeft (nog) geen eigen
/// installervoorvertoning — §12.6 liet die vraag open, "geen voorvertoning" is voorlopig de
/// eenvoudigste van de twee genoemde opties. WizardEditorWindow.xaml toont in plaats daarvan een
/// toelichtende tekst wanneer dit scherm geselecteerd is, met de twee afbeeldingvelden zelf mét een
/// kleine thumbnail (zie <see cref="WizardImage"/>/<see cref="WizardSmallImage"/> hieronder) — dat
/// is geen volledige mockup-pagina, alleen een directe bevestiging van wat er gekozen is.
/// </summary>
public sealed partial class DefaultScreenEditorViewModel : ObservableObject
{
    // Alleen nodig voor BrowseForImage hieronder (WizardImageFile/WizardSmallImageFile gaan, net
    // als een licentiebestand, via IProjectAssetService naar de projectmap) — zelfde patroon als
    // LicensePageEditorViewModel._projectFilePath/_assetService.
    private readonly string? _projectFilePath;
    private readonly IProjectAssetService _assetService;

    public DefaultScreenEditorViewModel(WizardScreenButtonSettings settings, string wizardImageFile, string wizardSmallImageFile, string? projectFilePath, IProjectAssetService assetService)
    {
        _projectFilePath = projectFilePath;
        _assetService = assetService;
        _wizardImageFile = wizardImageFile;
        _wizardSmallImageFile = wizardSmallImageFile;
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

    // Wizardafbeeldingen (backlogitem 1, sectie 14: verplaatst hierheen vanuit de
    // projectinstellingen). Zelfde leeg-betekent-nog-niet-aangepast-gedrag als
    // InstallerProject.WizardImageFile/WizardSmallImageFile; WizardImage/WizardSmallImage
    // hieronder lossen dat leeg-is-standaard-gedrag op voor de thumbnail hier én, via
    // WizardScreenEditorViewModel.Defaults, voor de voorvertoning van elk echt scherm.

    [ObservableProperty]
    private string _wizardImageFile;

    [ObservableProperty]
    private string _wizardSmallImageFile;

    partial void OnWizardImageFileChanged(string value) => OnPropertyChanged(nameof(WizardImage));

    partial void OnWizardSmallImageFileChanged(string value) => OnPropertyChanged(nameof(WizardSmallImage));

    /// <summary>Opgeloste afbeelding voor de thumbnail naast <see cref="WizardImageFile"/> hier, en
    /// (via <see cref="WizardScreenEditorViewModel.Defaults"/>) voor de Welkomst-/Voltooid-pagina's
    /// in de echte voorvertoning. Valt terug op een meegeleverde standaardafbeelding zolang
    /// <see cref="WizardImageFile"/> leeg is, zie <see cref="WizardImageResolver"/>.</summary>
    public ImageSource WizardImage => WizardImageResolver.ResolveWizardImage(WizardImageFile);

    /// <summary>Zie <see cref="WizardImage"/>, maar dan de kleine afbeelding rechtsboven op de
    /// overige wizardpagina's.</summary>
    public ImageSource WizardSmallImage => WizardImageResolver.ResolveWizardSmallImage(WizardSmallImageFile);

    [RelayCommand]
    private void BrowseWizardImage() => WizardImageFile = BrowseForImage(WizardImageFile);

    [RelayCommand]
    private void BrowseWizardSmallImage() => WizardSmallImageFile = BrowseForImage(WizardSmallImageFile);

    // Kopieert de gekozen afbeelding naar de projectmap zodra die van elders komt (zie
    // IProjectAssetService), zelfde patroon als LicensePageEditorViewModel.Browse en de vroegere
    // ProjectSettingsViewModel.BrowseForImage (vóór backlogitem 1: verplaatst hierheen). Bij een
    // nog niet opgeslagen project (_projectFilePath leeg) geeft dit ongewijzigd het gekozen pad
    // terug.
    private string BrowseForImage(string currentPath)
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

    // EffectiveXxxButtonCaption hieronder: zelfde naam als WizardScreenEditorViewModel's
    // drielaags-resolutie (geen gedeelde basisklasse, zie de klassencommentaar hierboven), maar
    // hier maar twee lagen - dit scherm ÍS de bron van de standaardwaarde, dus eigen tekst indien
    // ingevuld, anders meteen Inno Setup's eigen ingebouwde tekst. Herberts feedback (2026-09-28):
    // ButtonSettingsSectionTemplate (WizardEditorWindow.xaml) is dezelfde template voor dit scherm
    // én de drie echte schermen, en gebruikt deze eigenschap als Placeholder.Text (zie dat
    // bestand) zodat ook hier zichtbaar is wat er geldt zolang het eigen veld leeg is - dus ook op
    // het Standaardscherm zelf, niet alleen in de schermen die ervan overerven.

    /// <summary>Wat er geldt op de Terug-knop zolang <see cref="BackButtonCaption"/> leeg is.</summary>
    public string EffectiveBackButtonCaption => ResolveCaption(BackButtonCaption, LocalizationManager.Instance["ButtonWizardBack"]);

    /// <summary>Zie <see cref="EffectiveBackButtonCaption"/>, maar dan voor de Volgende-knop.</summary>
    public string EffectiveNextButtonCaption => ResolveCaption(NextButtonCaption, LocalizationManager.Instance["ButtonWizardNext"]);

    /// <summary>Zie <see cref="EffectiveBackButtonCaption"/>, maar dan voor de Annuleren-knop.</summary>
    public string EffectiveCancelButtonCaption => ResolveCaption(CancelButtonCaption, LocalizationManager.Instance["ButtonWizardCancel"]);

    private static string ResolveCaption(string own, string builtIn) => !string.IsNullOrWhiteSpace(own) ? own : builtIn;

    // EffectiveXxxButtonTextColor/-FontFamily/-FontSize hieronder bestaan puur zodat
    // ButtonSettingsSectionTemplate dezelfde Placeholder.Text-bindingen kan gebruiken als op de
    // drie echte schermen (zie WizardScreenEditorViewModel), zonder WPF-bindingsfouten wanneer dit
    // scherm de DataContext is. Anders dan Caption hierboven hebben deze géén zinvolle
    // terugvalwaarde om te tonen: leeg hier betekent Inno Setup's eigen, niet als kleur/lettertype
    // te benoemen standaarduiterlijk, dus altijd leeg/null - geen placeholder zichtbaar.

    /// <summary>Altijd leeg: er is geen terugvaltekstkleur om te tonen op het Standaardscherm
    /// zelf.</summary>
    public string EffectiveBackButtonTextColor => string.Empty;

    /// <summary>Zie <see cref="EffectiveBackButtonTextColor"/>, maar dan voor de Volgende-knop.</summary>
    public string EffectiveNextButtonTextColor => string.Empty;

    /// <summary>Zie <see cref="EffectiveBackButtonTextColor"/>, maar dan voor de Annuleren-knop.</summary>
    public string EffectiveCancelButtonTextColor => string.Empty;

    /// <summary>Altijd leeg: geen terugvallettertype om te tonen op het Standaardscherm zelf.</summary>
    public string EffectiveBackButtonFontFamily => string.Empty;

    /// <summary>Zie <see cref="EffectiveBackButtonFontFamily"/>, maar dan voor de Volgende-knop.</summary>
    public string EffectiveNextButtonFontFamily => string.Empty;

    /// <summary>Zie <see cref="EffectiveBackButtonFontFamily"/>, maar dan voor de Annuleren-knop.</summary>
    public string EffectiveCancelButtonFontFamily => string.Empty;

    /// <summary>Altijd null: geen terugvallettergrootte om te tonen op het Standaardscherm
    /// zelf.</summary>
    public int? EffectiveBackButtonFontSize => null;

    /// <summary>Zie <see cref="EffectiveBackButtonFontSize"/>, maar dan voor de Volgende-knop.</summary>
    public int? EffectiveNextButtonFontSize => null;

    /// <summary>Zie <see cref="EffectiveBackButtonFontSize"/>, maar dan voor de Annuleren-knop.</summary>
    public int? EffectiveCancelButtonFontSize => null;

    // Alleen-witruimte-invoer terugbrengen naar leeg (Herberts melding, 2026-09-28): zonder dit
    // bleven de echte spaties in de TextBox staan terwijl de spooktekst ("Volgende >" enz.) er
    // toch al overheen werd getoond (ResolveCaption behandelt witruimte al als "niet ingevuld"),
    // wat een verwarrende invoegcursor middenin de spooktekst gaf (bv. "Vo|lgende >" i.p.v. aan
    // het begin) — die spaties waren immers nog altijd echte, klikbare tekst. Roept de generated
    // setter opnieuw aan (leeg voldoet niet meer aan de voorwaarde), dus geen oneindige lus.
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
