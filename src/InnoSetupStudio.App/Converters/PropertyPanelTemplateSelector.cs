using System.Windows;
using System.Windows.Controls;
using InnoSetupStudio.App.ViewModels.Screens;

namespace InnoSetupStudio.App.Converters;

/// <summary>
/// Kiest de instellingenpaneel-template voor het geselecteerde scherm in de schermeditor (fase 4,
/// zie WizardEditorWindow.xaml). Nodig omdat hetzelfde VM-type daar twee verschillende templates
/// heeft: één voor de voorvertoning in het midden (WPF's automatische, keyless DataType-matching
/// volstaat daarvoor) en één voor dit instellingenpaneel rechts. Met alleen keyless templates zou
/// de laatst-gevonden template voor een type overal gebruikt worden, niet per plek een andere.
/// </summary>
public sealed class PropertyPanelTemplateSelector : DataTemplateSelector
{
    public DataTemplate? WelcomeTemplate { get; set; }

    public DataTemplate? LicenseTemplate { get; set; }

    public DataTemplate? SelectDestinationTemplate { get; set; }

    public override DataTemplate? SelectTemplate(object? item, DependencyObject container) => item switch
    {
        WelcomePageEditorViewModel => WelcomeTemplate,
        LicensePageEditorViewModel => LicenseTemplate,
        SelectDestinationPageEditorViewModel => SelectDestinationTemplate,
        _ => null,
    };
}
