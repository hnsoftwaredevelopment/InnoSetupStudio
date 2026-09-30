using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace InnoSetupStudio.Wizard.Converters;

/// <summary>
/// Kopie van InnoSetupStudio.App.Converters.FontFamilyOrUnsetConverter. Zie
/// HexColorToBrushConverter voor waarom dit een aparte kopie is in plaats van hergebruik.
/// </summary>
public sealed class FontFamilyOrUnsetConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string name || string.IsNullOrWhiteSpace(name))
        {
            return DependencyProperty.UnsetValue;
        }

        try
        {
            return new FontFamily(name);
        }
        catch (Exception ex) when (ex is ArgumentException or FormatException)
        {
            return DependencyProperty.UnsetValue;
        }
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
