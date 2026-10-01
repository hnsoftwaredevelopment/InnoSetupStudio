using System.Windows;
using System.Windows.Controls;
using InnoSetupStudio.App.Localization;
using InnoSetupStudio.App.ViewModels;
using InnoSetupStudio.App.ViewModels.Screens;

namespace InnoSetupStudio.App.Views;

/// <summary>
/// Voormalig WizardEditorWindow (sectie 21, IDE-schil herontwerp): permanent onderdeel van
/// MainWindow in plaats van een apart dialoogvenster dat via een knop werd geopend. MainWindow
/// zet <see cref="ViewModel"/> zodra er een actief project is (nieuw, geopend, of na een
/// gewijzigde schermselectie/talenselectie in Projectinstellingen) en luistert naar
/// <see cref="SaveClicked"/> om de wijzigingen daadwerkelijk op te slaan — deze control kent het
/// actieve project of de opslaglogica zelf niet, dat blijft MainWindow's verantwoordelijkheid
/// (zelfde scheiding als voorheen: WizardEditorViewModel.ApplyTo(project) werd ook toen al van
/// buitenaf aangeroepen).
/// </summary>
public partial class ScreenEditorControl : UserControl
{
    public ScreenEditorControl()
    {
        InitializeComponent();
    }

    public WizardEditorViewModel? ViewModel
    {
        get => DataContext as WizardEditorViewModel;
        set => DataContext = value;
    }

    /// <summary>Vuurt wanneer de gebruiker op de inline Opslaan-knop klikt. MainWindow past de
    /// wijzigingen toe op het actieve project (ViewModel.ApplyTo), slaat op, en zet IsDirty pas
    /// na een geslaagde save terug op false.</summary>
    public event EventHandler? SaveClicked;

    private void SaveButton_Click(object sender, RoutedEventArgs e) => SaveClicked?.Invoke(this, EventArgs.Empty);

    // Herbert (2026-09-30): kon na het selecteren van een echt scherm nooit meer terug naar
    // Standaardscherm. Oorzaak: DefaultScreenListBox en ScreensListBox binden allebei two-way naar
    // dezelfde WizardEditorViewModel.SelectedScreen, maar WPF's Selector.SelectedItem negeert een
    // toewijzing die niet in de eigen ItemsSource voorkomt in plaats van de markering te wissen —
    // dus zodra je in ScreensListBox iets koos, bleef DefaultScreenListBox intern nog steeds
    // "Standaardscherm geselecteerd" denken (zichtbaar aan de blijvende markering), en een
    // volgende muisklik daarop gold voor WPF niet als een wijziging (het was toch al
    // "geselecteerd"), dus er kwam geen SelectionChanged en dus ook geen nieuwe
    // SelectedScreen-waarde. Losstaand van de eerdere §12.7-beslissing om het Standaardscherm
    // visueel als geen echt scherm te tonen (aparte rij/scheidingslijn) — die blijft ongewijzigd.
    //
    // Fix: bij een selectie in de ene lijst expliciet de SelectedItem van de andere lijst op null
    // zetten (dat wist altijd, ook als de lijst zelf niet "weet" van de nieuwe waarde) en
    // SelectedScreen daarna expliciet opnieuw zetten, met een guard tegen de heropvoerde
    // SelectionChanged die dat nullen zelf weer veroorzaakt.
    private bool _isSyncingScreenSelection;

    /// <summary>Zie BuildForDefaultScreenButton: een lege lijst onderdrukt de per-taal-
    /// vertalingensectie in ButtonPropertiesWindow voor de drie knoppen van het Standaardscherm,
    /// omdat die vertalingen daar (anders dan de Engelse Caption/Tooltip) niet cascaderen naar de
    /// echte schermen.</summary>
    private static readonly IReadOnlyList<string> NoLanguageOverridesOnDefaultScreen = [];

    private void ScreenListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isSyncingScreenSelection || e.AddedItems.Count == 0 || ViewModel is null)
        {
            return;
        }

        var selected = e.AddedItems[0];
        var other = ReferenceEquals(sender, DefaultScreenListBox) ? ScreensListBox : DefaultScreenListBox;

        _isSyncingScreenSelection = true;
        try
        {
            other.SelectedItem = null;
            ViewModel.SelectedScreen = selected;
        }
        finally
        {
            _isSyncingScreenSelection = false;
        }
    }

    // Knop-eigenschappenscherm (backlogitem 3, sectie 17; blijft een eigen venster, sectie 21):
    // het properties-knopje achter elk van de drie tekstvelden (Terug/Volgende/Annuleren) in
    // ButtonSettingsSectionTemplate roept dit aan met zijn Tag ("Back"/"Next"/"Cancel") en de
    // DataContext van dat tekstveld-rijtje, die via de gedeelde template zowel een
    // WizardScreenEditorViewModel (Welkom/Licentie/Bestemming, drielaagse Effective*-resolutie)
    // als een DefaultScreenEditorViewModel (het Standaardscherm zelf, tweelaags-eigen) kan zijn -
    // vandaar de pattern-match hieronder in plaats van één gedeelde basisklasse-aanroep.
    private void ButtonProperties_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: string kind } element)
        {
            return;
        }

        var viewModel = element.DataContext switch
        {
            WizardScreenEditorViewModel screenVm => BuildForScreenButton(screenVm, kind),
            DefaultScreenEditorViewModel defaultVm => BuildForDefaultScreenButton(defaultVm, kind),
            _ => null,
        };

        if (viewModel is not null)
        {
            ShowButtonPropertiesDialog(viewModel);
        }
    }

    // Bladerknop (SelectDestinationPageEditorViewModel): eigen, kleinere Click-handler in plaats
    // van de Tag-gebaseerde switch hierboven, want er is hier maar één knop.
    private void BrowseButtonProperties_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: SelectDestinationPageEditorViewModel browseVm })
        {
            ShowButtonPropertiesDialog(BuildForBrowseButton(browseVm));
        }
    }

    private void ShowButtonPropertiesDialog(ButtonPropertiesViewModel viewModel)
    {
        var window = new ButtonPropertiesWindow(viewModel) { Owner = Window.GetWindow(this) };
        window.ShowDialog();
    }

    private static string BuildDialogTitle(string sectionTitleKey) => string.Format(
        LocalizationManager.Instance["DialogButtonPropertiesTitleFormat"],
        LocalizationManager.Instance[sectionTitleKey]);

    private static ButtonPropertiesViewModel BuildForScreenButton(WizardScreenEditorViewModel vm, string kind) => kind switch
    {
        "Back" => new ButtonPropertiesViewModel(
            BuildDialogTitle("LabelBackButtonSection"), hasCaption: true, vm.HintButtonTriStateText,
            () => vm.BackButtonCaption, v => vm.BackButtonCaption = v, vm.EffectiveBackButtonCaption,
            () => vm.BackButtonEnabled, v => vm.BackButtonEnabled = v,
            () => vm.BackButtonVisible, v => vm.BackButtonVisible = v,
            () => vm.BackButtonTextColor, v => vm.BackButtonTextColor = v, vm.EffectiveBackButtonTextColor,
            () => vm.BackButtonFontFamily, v => vm.BackButtonFontFamily = v, vm.EffectiveBackButtonFontFamily,
            () => vm.BackButtonFontSize, v => vm.BackButtonFontSize = v, vm.EffectiveBackButtonFontSize,
            () => vm.BackButtonFontBold, v => vm.BackButtonFontBold = v, vm.EffectiveBackButtonFontBold,
            () => vm.BackButtonTooltip, v => vm.BackButtonTooltip = v, vm.EffectiveBackButtonTooltip,
            vm.NonEnglishLanguageIds,
            () => vm.BackButtonCaptionByLanguage, v => vm.BackButtonCaptionByLanguage = v,
            () => vm.BackButtonTooltipByLanguage, v => vm.BackButtonTooltipByLanguage = v),
        "Next" => new ButtonPropertiesViewModel(
            BuildDialogTitle("LabelNextButtonSection"), hasCaption: true, vm.HintButtonTriStateText,
            () => vm.NextButtonCaption, v => vm.NextButtonCaption = v, vm.EffectiveNextButtonCaption,
            () => vm.NextButtonEnabled, v => vm.NextButtonEnabled = v,
            () => vm.NextButtonVisible, v => vm.NextButtonVisible = v,
            () => vm.NextButtonTextColor, v => vm.NextButtonTextColor = v, vm.EffectiveNextButtonTextColor,
            () => vm.NextButtonFontFamily, v => vm.NextButtonFontFamily = v, vm.EffectiveNextButtonFontFamily,
            () => vm.NextButtonFontSize, v => vm.NextButtonFontSize = v, vm.EffectiveNextButtonFontSize,
            () => vm.NextButtonFontBold, v => vm.NextButtonFontBold = v, vm.EffectiveNextButtonFontBold,
            () => vm.NextButtonTooltip, v => vm.NextButtonTooltip = v, vm.EffectiveNextButtonTooltip,
            vm.NonEnglishLanguageIds,
            () => vm.NextButtonCaptionByLanguage, v => vm.NextButtonCaptionByLanguage = v,
            () => vm.NextButtonTooltipByLanguage, v => vm.NextButtonTooltipByLanguage = v),
        "Cancel" => new ButtonPropertiesViewModel(
            BuildDialogTitle("LabelCancelButtonSection"), hasCaption: true, vm.HintButtonTriStateText,
            () => vm.CancelButtonCaption, v => vm.CancelButtonCaption = v, vm.EffectiveCancelButtonCaption,
            () => vm.CancelButtonEnabled, v => vm.CancelButtonEnabled = v,
            () => vm.CancelButtonVisible, v => vm.CancelButtonVisible = v,
            () => vm.CancelButtonTextColor, v => vm.CancelButtonTextColor = v, vm.EffectiveCancelButtonTextColor,
            () => vm.CancelButtonFontFamily, v => vm.CancelButtonFontFamily = v, vm.EffectiveCancelButtonFontFamily,
            () => vm.CancelButtonFontSize, v => vm.CancelButtonFontSize = v, vm.EffectiveCancelButtonFontSize,
            () => vm.CancelButtonFontBold, v => vm.CancelButtonFontBold = v, vm.EffectiveCancelButtonFontBold,
            () => vm.CancelButtonTooltip, v => vm.CancelButtonTooltip = v, vm.EffectiveCancelButtonTooltip,
            vm.NonEnglishLanguageIds,
            () => vm.CancelButtonCaptionByLanguage, v => vm.CancelButtonCaptionByLanguage = v,
            () => vm.CancelButtonTooltipByLanguage, v => vm.CancelButtonTooltipByLanguage = v),
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Onbekende knop-Tag op het properties-knopje."),
    };

    // DefaultScreenEditorViewModel heeft geen EffectiveXxxButtonFontBold (geen cascade, dit
    // scherm ÍS de bron van de standaardwaarde), dus hier steeds "null" als effectiveFontBold.
    //
    // Meertalige knopteksten (sectie 14-backlogitem) krijgen hier bewust GEEN rijen
    // (NoLanguageOverridesOnDefaultScreen hieronder, in plaats van vm.NonEnglishLanguageIds zoals
    // BuildForScreenButton hierboven dat wél doorgeeft): CodeRabbit-opmerking op PR #21, de
    // per-taal-dictionaries op het Standaardscherm cascaderen (bewust, zie docs/Architectuur-en-
    // Ontwerp.md sectie 24) niet door naar de echte schermen zoals de Engelse Caption/Tooltip dat
    // wél doen - een hier ingevulde vertaling zou dus stil niets doen, terwijl de rest van dit
    // scherm (de gewone Caption/Tooltip-velden) juist wél overal als terugvalwaarde gebruikt
    // wordt. Rijen tonen die geen enkel effect hebben zou verwarrend zijn, dus die sectie blijft
    // hier verborgen (ButtonPropertiesViewModel.HasLanguageOverrides wordt dan false).
    private static ButtonPropertiesViewModel BuildForDefaultScreenButton(DefaultScreenEditorViewModel vm, string kind) => kind switch
    {
        "Back" => new ButtonPropertiesViewModel(
            BuildDialogTitle("LabelBackButtonSection"), hasCaption: true, vm.HintButtonTriStateText,
            () => vm.BackButtonCaption, v => vm.BackButtonCaption = v, vm.EffectiveBackButtonCaption,
            () => vm.BackButtonEnabled, v => vm.BackButtonEnabled = v,
            () => vm.BackButtonVisible, v => vm.BackButtonVisible = v,
            () => vm.BackButtonTextColor, v => vm.BackButtonTextColor = v, vm.EffectiveBackButtonTextColor,
            () => vm.BackButtonFontFamily, v => vm.BackButtonFontFamily = v, vm.EffectiveBackButtonFontFamily,
            () => vm.BackButtonFontSize, v => vm.BackButtonFontSize = v, vm.EffectiveBackButtonFontSize,
            () => vm.BackButtonFontBold, v => vm.BackButtonFontBold = v, null,
            () => vm.BackButtonTooltip, v => vm.BackButtonTooltip = v, vm.EffectiveBackButtonTooltip,
            NoLanguageOverridesOnDefaultScreen,
            () => vm.BackButtonCaptionByLanguage, v => vm.BackButtonCaptionByLanguage = v,
            () => vm.BackButtonTooltipByLanguage, v => vm.BackButtonTooltipByLanguage = v),
        "Next" => new ButtonPropertiesViewModel(
            BuildDialogTitle("LabelNextButtonSection"), hasCaption: true, vm.HintButtonTriStateText,
            () => vm.NextButtonCaption, v => vm.NextButtonCaption = v, vm.EffectiveNextButtonCaption,
            () => vm.NextButtonEnabled, v => vm.NextButtonEnabled = v,
            () => vm.NextButtonVisible, v => vm.NextButtonVisible = v,
            () => vm.NextButtonTextColor, v => vm.NextButtonTextColor = v, vm.EffectiveNextButtonTextColor,
            () => vm.NextButtonFontFamily, v => vm.NextButtonFontFamily = v, vm.EffectiveNextButtonFontFamily,
            () => vm.NextButtonFontSize, v => vm.NextButtonFontSize = v, vm.EffectiveNextButtonFontSize,
            () => vm.NextButtonFontBold, v => vm.NextButtonFontBold = v, null,
            () => vm.NextButtonTooltip, v => vm.NextButtonTooltip = v, vm.EffectiveNextButtonTooltip,
            NoLanguageOverridesOnDefaultScreen,
            () => vm.NextButtonCaptionByLanguage, v => vm.NextButtonCaptionByLanguage = v,
            () => vm.NextButtonTooltipByLanguage, v => vm.NextButtonTooltipByLanguage = v),
        "Cancel" => new ButtonPropertiesViewModel(
            BuildDialogTitle("LabelCancelButtonSection"), hasCaption: true, vm.HintButtonTriStateText,
            () => vm.CancelButtonCaption, v => vm.CancelButtonCaption = v, vm.EffectiveCancelButtonCaption,
            () => vm.CancelButtonEnabled, v => vm.CancelButtonEnabled = v,
            () => vm.CancelButtonVisible, v => vm.CancelButtonVisible = v,
            () => vm.CancelButtonTextColor, v => vm.CancelButtonTextColor = v, vm.EffectiveCancelButtonTextColor,
            () => vm.CancelButtonFontFamily, v => vm.CancelButtonFontFamily = v, vm.EffectiveCancelButtonFontFamily,
            () => vm.CancelButtonFontSize, v => vm.CancelButtonFontSize = v, vm.EffectiveCancelButtonFontSize,
            () => vm.CancelButtonFontBold, v => vm.CancelButtonFontBold = v, null,
            () => vm.CancelButtonTooltip, v => vm.CancelButtonTooltip = v, vm.EffectiveCancelButtonTooltip,
            NoLanguageOverridesOnDefaultScreen,
            () => vm.CancelButtonCaptionByLanguage, v => vm.CancelButtonCaptionByLanguage = v,
            () => vm.CancelButtonTooltipByLanguage, v => vm.CancelButtonTooltipByLanguage = v),
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Onbekende knop-Tag op het properties-knopje."),
    };

    private static ButtonPropertiesViewModel BuildForBrowseButton(SelectDestinationPageEditorViewModel vm) => new(
        BuildDialogTitle("SectionBrowseButton"), hasCaption: true,
        LocalizationManager.Instance["HintButtonTriStateDefaultScreen"],
        () => vm.BrowseButtonCaption, v => vm.BrowseButtonCaption = v, vm.EffectiveBrowseButtonCaption,
        () => vm.BrowseButtonEnabled, v => vm.BrowseButtonEnabled = v,
        () => vm.BrowseButtonVisible, v => vm.BrowseButtonVisible = v,
        () => vm.BrowseButtonTextColor, v => vm.BrowseButtonTextColor = v, string.Empty,
        () => vm.BrowseButtonFontFamily, v => vm.BrowseButtonFontFamily = v, string.Empty,
        () => vm.BrowseButtonFontSize, v => vm.BrowseButtonFontSize = v, null,
        () => vm.BrowseButtonFontBold, v => vm.BrowseButtonFontBold = v, null,
        () => vm.BrowseButtonTooltip, v => vm.BrowseButtonTooltip = v, string.Empty,
        vm.NonEnglishLanguageIds,
        () => vm.BrowseButtonCaptionByLanguage, v => vm.BrowseButtonCaptionByLanguage = v,
        () => vm.BrowseButtonTooltipByLanguage, v => vm.BrowseButtonTooltipByLanguage = v);
}
