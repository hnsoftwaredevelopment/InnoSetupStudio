using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace InnoSetupStudio.Wizard.Converters;

/// <summary>
/// Kopie van InnoSetupStudio.App.Converters.NullableBoolToFontWeightConverter. Het Wizard-project
/// mag niet naar het App-project verwijzen, vandaar een aparte kopie.
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
