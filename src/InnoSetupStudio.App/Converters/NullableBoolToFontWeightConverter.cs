using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace InnoSetupStudio.App.Converters;

/// <summary>
/// Zet WizardScreenEditorViewModel's EffectiveXxxFontBold (bool?, zie WizardScreenButtonSettings)
/// om naar een System.Windows.FontWeight voor de Terug-/Volgende-/Annuleren-knoppen in de
/// voorvertoning. Zelfde UnsetValue-bij-null-conventie als HexColorToBrushConverter: null
/// betekent "geen override", de knop valt dan terug op zijn eigen stijl-standaardgewicht.
/// </summary>
public sealed class NullableBoolToFontWeightConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value switch
    {
        true => FontWeights.Bold,
        false => FontWeights.Normal,
        _ => DependencyProperty.UnsetValue,
    };

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
