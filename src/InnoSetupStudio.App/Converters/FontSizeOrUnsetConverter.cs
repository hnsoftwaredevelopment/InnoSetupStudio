using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace InnoSetupStudio.App.Converters;

/// <summary>
/// Zet WizardScreenEditorViewModel's EffectiveXxxFontSize (int?, zie WizardScreenButtonSettings)
/// om naar een double voor WPF's Control.FontSize in de voorvertoning. Zelfde UnsetValue-bij-
/// leeg-conventie als HexColorToBrushConverter: null betekent "geen override", de knop valt dan
/// terug op zijn eigen stijl-standaardgrootte.
/// </summary>
public sealed class FontSizeOrUnsetConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is int size && size > 0 ? (double)size : DependencyProperty.UnsetValue;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
