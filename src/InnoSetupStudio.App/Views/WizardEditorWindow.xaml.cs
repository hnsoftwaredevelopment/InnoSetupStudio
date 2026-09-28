using System.Windows;
using InnoSetupStudio.App.Localization;
using InnoSetupStudio.App.ViewModels;
using InnoSetupStudio.App.ViewModels.Screens;

namespace InnoSetupStudio.App.Views;

public partial class WizardEditorWindow : Window
{
    public WizardEditorViewModel ViewModel { get; }

    public WizardEditorWindow(WizardEditorViewModel viewModel)
    {
        InitializeComponent();

        ViewModel = viewModel;
        DataContext = viewModel;
        viewModel.RequestClose += OnRequestClose;
    }

    private void OnRequestClose(object? sender, bool saved)
    {
        DialogResult = saved;
        Close();
    }

    // Knop-eigenschappenscherm (backlogitem 3, sectie 17): het properties-knopje achter elk van
    // de drie tekstvelden (Terug/Volgende/Annuleren) in ButtonSettingsSectionTemplate roept dit
    // aan met zijn Tag ("Back"/"Next"/"Cancel") en de DataContext van dat tekstveld-rijtje, die
    // via de gedeelde template zowel een WizardScreenEditorViewModel (Welkom/Licentie/Bestemming,
    // drielaagse Effective*-resolutie) als een DefaultScreenEditorViewModel (het Standaardscherm
    // zelf, tweelaags-eigen) kan zijn - vandaar de pattern-match hieronder in plaats van één
    // gedeelde basisklasse-aanroep (zie ButtonPropertiesViewModel voor waarom dat bewust niet is
    // geherstructureerd).
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

    // Bladerknop (SelectDestinationPageEditorViewModel, geen Caption): eigen, kleinere Click-
    // handler in plaats van de Tag-gebaseerde switch hierboven, want er is hier maar één knop.
    private void BrowseButtonProperties_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: SelectDestinationPageEditorViewModel browseVm } )
        {
            ShowButtonPropertiesDialog(BuildForBrowseButton(browseVm));
        }
    }

    private void ShowButtonPropertiesDialog(ButtonPropertiesViewModel viewModel)
    {
        var window = new ButtonPropertiesWindow(viewModel) { Owner = this };
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
            () => vm.BackButtonFontBold, v => vm.BackButtonFontBold = v,
            () => vm.BackButtonTooltip, v => vm.BackButtonTooltip = v, vm.EffectiveBackButtonTooltip),
        "Next" => new ButtonPropertiesViewModel(
            BuildDialogTitle("LabelNextButtonSection"), hasCaption: true, vm.HintButtonTriStateText,
            () => vm.NextButtonCaption, v => vm.NextButtonCaption = v, vm.EffectiveNextButtonCaption,
            () => vm.NextButtonEnabled, v => vm.NextButtonEnabled = v,
            () => vm.NextButtonVisible, v => vm.NextButtonVisible = v,
            () => vm.NextButtonTextColor, v => vm.NextButtonTextColor = v, vm.EffectiveNextButtonTextColor,
            () => vm.NextButtonFontFamily, v => vm.NextButtonFontFamily = v, vm.EffectiveNextButtonFontFamily,
            () => vm.NextButtonFontSize, v => vm.NextButtonFontSize = v, vm.EffectiveNextButtonFontSize,
            () => vm.NextButtonFontBold, v => vm.NextButtonFontBold = v,
            () => vm.NextButtonTooltip, v => vm.NextButtonTooltip = v, vm.EffectiveNextButtonTooltip),
        "Cancel" => new ButtonPropertiesViewModel(
            BuildDialogTitle("LabelCancelButtonSection"), hasCaption: true, vm.HintButtonTriStateText,
            () => vm.CancelButtonCaption, v => vm.CancelButtonCaption = v, vm.EffectiveCancelButtonCaption,
            () => vm.CancelButtonEnabled, v => vm.CancelButtonEnabled = v,
            () => vm.CancelButtonVisible, v => vm.CancelButtonVisible = v,
            () => vm.CancelButtonTextColor, v => vm.CancelButtonTextColor = v, vm.EffectiveCancelButtonTextColor,
            () => vm.CancelButtonFontFamily, v => vm.CancelButtonFontFamily = v, vm.EffectiveCancelButtonFontFamily,
            () => vm.CancelButtonFontSize, v => vm.CancelButtonFontSize = v, vm.EffectiveCancelButtonFontSize,
            () => vm.CancelButtonFontBold, v => vm.CancelButtonFontBold = v,
            () => vm.CancelButtonTooltip, v => vm.CancelButtonTooltip = v, vm.EffectiveCancelButtonTooltip),
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Onbekende knop-Tag op het properties-knopje."),
    };

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
            () => vm.BackButtonFontBold, v => vm.BackButtonFontBold = v,
            () => vm.BackButtonTooltip, v => vm.BackButtonTooltip = v, vm.EffectiveBackButtonTooltip),
        "Next" => new ButtonPropertiesViewModel(
            BuildDialogTitle("LabelNextButtonSection"), hasCaption: true, vm.HintButtonTriStateText,
            () => vm.NextButtonCaption, v => vm.NextButtonCaption = v, vm.EffectiveNextButtonCaption,
            () => vm.NextButtonEnabled, v => vm.NextButtonEnabled = v,
            () => vm.NextButtonVisible, v => vm.NextButtonVisible = v,
            () => vm.NextButtonTextColor, v => vm.NextButtonTextColor = v, vm.EffectiveNextButtonTextColor,
            () => vm.NextButtonFontFamily, v => vm.NextButtonFontFamily = v, vm.EffectiveNextButtonFontFamily,
            () => vm.NextButtonFontSize, v => vm.NextButtonFontSize = v, vm.EffectiveNextButtonFontSize,
            () => vm.NextButtonFontBold, v => vm.NextButtonFontBold = v,
            () => vm.NextButtonTooltip, v => vm.NextButtonTooltip = v, vm.EffectiveNextButtonTooltip),
        "Cancel" => new ButtonPropertiesViewModel(
            BuildDialogTitle("LabelCancelButtonSection"), hasCaption: true, vm.HintButtonTriStateText,
            () => vm.CancelButtonCaption, v => vm.CancelButtonCaption = v, vm.EffectiveCancelButtonCaption,
            () => vm.CancelButtonEnabled, v => vm.CancelButtonEnabled = v,
            () => vm.CancelButtonVisible, v => vm.CancelButtonVisible = v,
            () => vm.CancelButtonTextColor, v => vm.CancelButtonTextColor = v, vm.EffectiveCancelButtonTextColor,
            () => vm.CancelButtonFontFamily, v => vm.CancelButtonFontFamily = v, vm.EffectiveCancelButtonFontFamily,
            () => vm.CancelButtonFontSize, v => vm.CancelButtonFontSize = v, vm.EffectiveCancelButtonFontSize,
            () => vm.CancelButtonFontBold, v => vm.CancelButtonFontBold = v,
            () => vm.CancelButtonTooltip, v => vm.CancelButtonTooltip = v, vm.EffectiveCancelButtonTooltip),
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Onbekende knop-Tag op het properties-knopje."),
    };

    // Bladerknop (BrowseButtonSettings): geen Caption en geen enkele Effective*-cascade (zie
    // SelectDestinationPageEditorViewModel - dit is de knop zijn eigen, hoogste niveau, net als de
    // Terug/Volgende/Annuleren-velden op het Standaardscherm zelf), dus lege/null terugvalwaarden
    // voor de Effective*-parameters (geen grijze hint-tekst te tonen) en de
    // HintButtonTriStateDefaultScreen-bewoording rechtstreeks (zelfde reden als het
    // Standaardscherm: "onbepaald" betekent hier rechtstreeks Inno Setup's eigen standaardgedrag,
    // niet "neemt de waarde van het Standaardscherm over").
    private static ButtonPropertiesViewModel BuildForBrowseButton(SelectDestinationPageEditorViewModel vm) => new(
        BuildDialogTitle("SectionBrowseButton"), hasCaption: false,
        LocalizationManager.Instance["HintButtonTriStateDefaultScreen"],
        () => string.Empty, _ => { }, string.Empty,
        () => vm.BrowseButtonEnabled, v => vm.BrowseButtonEnabled = v,
        () => vm.BrowseButtonVisible, v => vm.BrowseButtonVisible = v,
        () => vm.BrowseButtonTextColor, v => vm.BrowseButtonTextColor = v, string.Empty,
        () => vm.BrowseButtonFontFamily, v => vm.BrowseButtonFontFamily = v, string.Empty,
        () => vm.BrowseButtonFontSize, v => vm.BrowseButtonFontSize = v, null,
        () => vm.BrowseButtonFontBold, v => vm.BrowseButtonFontBold = v,
        () => vm.BrowseButtonTooltip, v => vm.BrowseButtonTooltip = v, string.Empty);
}
