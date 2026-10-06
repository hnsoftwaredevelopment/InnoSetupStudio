using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace InnoSetupStudio.Wizard.Converters;

/// <summary>
/// Kopie van InnoSetupStudio.App.Converters.FontSizeOrUnsetConverter. Het Wizard-project
/// mag niet naar het App-project verwijzen, vandaar een aparte kopie.
/// </summary>
public sealed class FontSizeOrUnsetConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is int size && size > 0 ? (double)size : DependencyProperty.UnsetValue;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
