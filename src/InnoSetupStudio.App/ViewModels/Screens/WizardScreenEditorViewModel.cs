using System.IO;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InnoSetupStudio.App.Localization;
using InnoSetupStudio.Core.Project;
using Microsoft.Win32;

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
    private readonly IProjectAssetService _assetService;
    private readonly string? _projectFilePath;

    // assetService/projectFilePath staan hier op de basisklasse (in plaats van alleen op
    // LicensePageEditorViewModel, die ze al voor LicenseFilePath gebruikte) omdat de drie
    // Browse*ButtonBitmap-commando's hieronder gelden voor elk schermtype: elk scherm heeft zijn
    // eigen Terug-/Volgende-/Annuleren-knop, dus ook elk schermtype kan daar een bitmap voor
    // kiezen. LicensePageEditorViewModel geeft voortaan dezelfde twee waarden door aan base(...)
    // in plaats van ze zelf nog eens op te slaan.
    protected WizardScreenEditorViewModel(string id, string title, string iconKey, IProjectAssetService assetService, string? projectFilePath)
    {
        Id = id;
        Title = title;
        IconKey = iconKey;
        _assetService = assetService;
        _projectFilePath = projectFilePath;
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
    // WizardImageFile/WizardSmallImageFile), niet iets per scherm: WizardEditorViewModel bepaalt
    // ze één keer bij het openen van de schermeditor (zie WizardImageResolver) en geeft ze aan elk
    // scherm door, zodat een toekomstig scherm dat ze nodig heeft ze automatisch al beschikbaar
    // heeft. Alleen-lezen: binnen één schermeditor-sessie wijzigen deze niet, ze veranderen pas
    // wanneer de gebruiker in de projectinstellingen een andere afbeelding kiest en de
    // schermeditor opnieuw opent.
    public required ImageSource WizardImage { get; init; }

    /// <summary>Zie <see cref="WizardImage"/>, maar dan de kleine afbeelding rechtsboven.</summary>
    public required ImageSource WizardSmallImage { get; init; }

    /// <summary>
    /// Schrijfalleen init-eigenschap zodat WizardEditorViewModel de negen knopvelden hieronder in
    /// één keer kan meegeven via dezelfde object-initializer-syntax als WizardImage/
    /// WizardSmallImage (<c>new XPageEditorViewModel(...) { ButtonSettings = ... }</c>), in plaats
    /// van negen losse constructorparameters. <see langword="required"/> zodat een nieuw
    /// schermtype dit nooit per ongeluk leeg laat staan.
    /// </summary>
    public required WizardScreenButtonSettings ButtonSettings
    {
        init
        {
            BackButtonCaption = value.BackButtonCaption;
            BackButtonEnabled = value.BackButtonEnabled;
            BackButtonVisible = value.BackButtonVisible;
            BackButtonTextColor = value.BackButtonTextColor;
            BackButtonBackgroundColor = value.BackButtonBackgroundColor;
            BackButtonBitmapFilePath = value.BackButtonBitmapFilePath;
            NextButtonCaption = value.NextButtonCaption;
            NextButtonEnabled = value.NextButtonEnabled;
            NextButtonVisible = value.NextButtonVisible;
            NextButtonTextColor = value.NextButtonTextColor;
            NextButtonBackgroundColor = value.NextButtonBackgroundColor;
            NextButtonBitmapFilePath = value.NextButtonBitmapFilePath;
            CancelButtonCaption = value.CancelButtonCaption;
            CancelButtonEnabled = value.CancelButtonEnabled;
            CancelButtonVisible = value.CancelButtonVisible;
            CancelButtonTextColor = value.CancelButtonTextColor;
            CancelButtonBackgroundColor = value.CancelButtonBackgroundColor;
            CancelButtonBitmapFilePath = value.CancelButtonBitmapFilePath;
        }
    }

    private DefaultScreenEditorViewModel? _defaults;

    /// <summary>
    /// Het Standaardscherm van dezelfde schermeditor-sessie (zie WizardEditorViewModel), de
    /// tweede laag van de Effective*/Is*-resolutie hieronder. Required init, net als WizardImage/
    /// WizardSmallImage, zodat een nieuw schermtype dit nooit vergeet. Anders dan die twee heeft
    /// dit wél een custom init-accessor: WizardEditorViewModel maakt één DefaultScreenEditorViewModel
    /// voor de hele sessie en geeft dezelfde (levende) instantie aan elk scherm door, dus een
    /// wijziging op het Standaardscherm moet híer meteen de afgeleide Effective*/Is*-waarden
    /// bijwerken — vandaar het abonneren op PropertyChanged in plaats van alleen de negen velden
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
        OnPropertyChanged(nameof(EffectiveBackButtonCaption));
        OnPropertyChanged(nameof(EffectiveNextButtonCaption));
        OnPropertyChanged(nameof(EffectiveCancelButtonCaption));
        OnPropertyChanged(nameof(IsBackButtonVisible));
        OnPropertyChanged(nameof(IsNextButtonVisible));
        OnPropertyChanged(nameof(IsCancelButtonVisible));
        OnPropertyChanged(nameof(IsBackButtonEnabled));
        OnPropertyChanged(nameof(IsNextButtonEnabled));
        OnPropertyChanged(nameof(IsCancelButtonEnabled));
        OnPropertyChanged(nameof(EffectiveBackButtonTextColor));
        OnPropertyChanged(nameof(EffectiveNextButtonTextColor));
        OnPropertyChanged(nameof(EffectiveCancelButtonTextColor));
        OnPropertyChanged(nameof(EffectiveBackButtonBackgroundColor));
        OnPropertyChanged(nameof(EffectiveNextButtonBackgroundColor));
        OnPropertyChanged(nameof(EffectiveCancelButtonBackgroundColor));
        OnPropertyChanged(nameof(EffectiveBackButtonBitmapFilePath));
        OnPropertyChanged(nameof(EffectiveNextButtonBitmapFilePath));
        OnPropertyChanged(nameof(EffectiveCancelButtonBitmapFilePath));
    }

    // De negen velden hieronder staan, anders dan WizardImage/WizardSmallImage, wél op de
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

    // Kleuren/bitmap (backlogitem 3, sectie 14): zelfde leeg-is-nog-niet-aangepast-conventie als
    // Caption hierboven, geen apart tri-state type nodig. Zie WizardScreenButtonSettings voor
    // waarom dit hex-tekst is in plaats van System.Windows.Media.Color.

    [ObservableProperty]
    private string _backButtonTextColor = string.Empty;

    [ObservableProperty]
    private string _backButtonBackgroundColor = string.Empty;

    [ObservableProperty]
    private string _backButtonBitmapFilePath = string.Empty;

    [ObservableProperty]
    private string _nextButtonTextColor = string.Empty;

    [ObservableProperty]
    private string _nextButtonBackgroundColor = string.Empty;

    [ObservableProperty]
    private string _nextButtonBitmapFilePath = string.Empty;

    [ObservableProperty]
    private string _cancelButtonTextColor = string.Empty;

    [ObservableProperty]
    private string _cancelButtonBackgroundColor = string.Empty;

    [ObservableProperty]
    private string _cancelButtonBitmapFilePath = string.Empty;

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

    // Zelfde drielaags-resolutie als EffectiveXxxCaption hierboven, en dezelfde ResolveCaption-
    // helper: "builtIn" is hier altijd een lege string (geen kleur/bitmap instellen = Inno Setup's
    // eigen knopuiterlijk), er is geen studio-eigen vertaalde tekst zoals bij Caption.

    /// <summary>Wat de voorvertoning daadwerkelijk als tekstkleur op de Terug-knop toont, leeg =
    /// geen override (Inno Setup's eigen kleur blijft gelden).</summary>
    public string EffectiveBackButtonTextColor => ResolveCaption(BackButtonTextColor, Defaults.BackButtonTextColor, string.Empty);

    /// <summary>Zie <see cref="EffectiveBackButtonTextColor"/>, maar dan voor de Volgende-knop.</summary>
    public string EffectiveNextButtonTextColor => ResolveCaption(NextButtonTextColor, Defaults.NextButtonTextColor, string.Empty);

    /// <summary>Zie <see cref="EffectiveBackButtonTextColor"/>, maar dan voor de Annuleren-knop.</summary>
    public string EffectiveCancelButtonTextColor => ResolveCaption(CancelButtonTextColor, Defaults.CancelButtonTextColor, string.Empty);

    /// <summary>Zie <see cref="EffectiveBackButtonTextColor"/>, maar dan voor de achtergrondkleur.</summary>
    public string EffectiveBackButtonBackgroundColor => ResolveCaption(BackButtonBackgroundColor, Defaults.BackButtonBackgroundColor, string.Empty);

    /// <summary>Zie <see cref="EffectiveBackButtonBackgroundColor"/>, maar dan voor de Volgende-knop.</summary>
    public string EffectiveNextButtonBackgroundColor => ResolveCaption(NextButtonBackgroundColor, Defaults.NextButtonBackgroundColor, string.Empty);

    /// <summary>Zie <see cref="EffectiveBackButtonBackgroundColor"/>, maar dan voor de Annuleren-knop.</summary>
    public string EffectiveCancelButtonBackgroundColor => ResolveCaption(CancelButtonBackgroundColor, Defaults.CancelButtonBackgroundColor, string.Empty);

    /// <summary>Zie <see cref="EffectiveBackButtonTextColor"/>, maar dan voor de knopbitmap.</summary>
    public string EffectiveBackButtonBitmapFilePath => ResolveCaption(BackButtonBitmapFilePath, Defaults.BackButtonBitmapFilePath, string.Empty);

    /// <summary>Zie <see cref="EffectiveBackButtonBitmapFilePath"/>, maar dan voor de Volgende-knop.</summary>
    public string EffectiveNextButtonBitmapFilePath => ResolveCaption(NextButtonBitmapFilePath, Defaults.NextButtonBitmapFilePath, string.Empty);

    /// <summary>Zie <see cref="EffectiveBackButtonBitmapFilePath"/>, maar dan voor de Annuleren-knop.</summary>
    public string EffectiveCancelButtonBitmapFilePath => ResolveCaption(CancelButtonBitmapFilePath, Defaults.CancelButtonBitmapFilePath, string.Empty);

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

    partial void OnBackButtonCaptionChanged(string value) => OnPropertyChanged(nameof(EffectiveBackButtonCaption));

    partial void OnNextButtonCaptionChanged(string value) => OnPropertyChanged(nameof(EffectiveNextButtonCaption));

    partial void OnCancelButtonCaptionChanged(string value) => OnPropertyChanged(nameof(EffectiveCancelButtonCaption));

    partial void OnBackButtonVisibleChanged(bool? value) => OnPropertyChanged(nameof(IsBackButtonVisible));

    partial void OnNextButtonVisibleChanged(bool? value) => OnPropertyChanged(nameof(IsNextButtonVisible));

    partial void OnCancelButtonVisibleChanged(bool? value) => OnPropertyChanged(nameof(IsCancelButtonVisible));

    partial void OnBackButtonEnabledChanged(bool? value) => OnPropertyChanged(nameof(IsBackButtonEnabled));

    partial void OnNextButtonEnabledChanged(bool? value) => OnPropertyChanged(nameof(IsNextButtonEnabled));

    partial void OnCancelButtonEnabledChanged(bool? value) => OnPropertyChanged(nameof(IsCancelButtonEnabled));

    partial void OnBackButtonTextColorChanged(string value) => OnPropertyChanged(nameof(EffectiveBackButtonTextColor));

    partial void OnNextButtonTextColorChanged(string value) => OnPropertyChanged(nameof(EffectiveNextButtonTextColor));

    partial void OnCancelButtonTextColorChanged(string value) => OnPropertyChanged(nameof(EffectiveCancelButtonTextColor));

    partial void OnBackButtonBackgroundColorChanged(string value) => OnPropertyChanged(nameof(EffectiveBackButtonBackgroundColor));

    partial void OnNextButtonBackgroundColorChanged(string value) => OnPropertyChanged(nameof(EffectiveNextButtonBackgroundColor));

    partial void OnCancelButtonBackgroundColorChanged(string value) => OnPropertyChanged(nameof(EffectiveCancelButtonBackgroundColor));

    partial void OnBackButtonBitmapFilePathChanged(string value) => OnPropertyChanged(nameof(EffectiveBackButtonBitmapFilePath));

    partial void OnNextButtonBitmapFilePathChanged(string value) => OnPropertyChanged(nameof(EffectiveNextButtonBitmapFilePath));

    partial void OnCancelButtonBitmapFilePathChanged(string value) => OnPropertyChanged(nameof(EffectiveCancelButtonBitmapFilePath));

    // Bladerknoppen voor de drie knopbitmaps: zelfde patroon als LicensePageEditorViewModel.Browse
    // (OpenFileDialog + IProjectAssetService.Import zodat het project verplaatsbaar blijft), maar
    // hier één helpermethode voor alle drie in plaats van drie bijna-identieke commando's, want
    // die verschillen alleen in welk veld ze lezen/schrijven.

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

    /// <summary>Tegenhanger van de <see cref="ButtonSettings"/>-init-eigenschap: leest de negen
    /// velden terug in een nieuwe <see cref="WizardScreenButtonSettings"/>, gebruikt door
    /// WizardEditorViewModel.ApplyTo.</summary>
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
