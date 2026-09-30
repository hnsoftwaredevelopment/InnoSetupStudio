using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace InnoSetupStudio.Wizard.Converters;

/// <summary>
/// Kopie van InnoSetupStudio.App.Converters.HexColorToBrushConverter. Het Wizard-project mag niet
/// naar het App-project verwijzen (dat zou een circulaire referentie geven, zie de projectopzet
/// in Architectuur-en-Ontwerp.md), dus deze kleine converter is hier gedupliceerd in plaats van
/// hergebruikt.
/// </summary>
public sealed class HexColorToBrushConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string hex || string.IsNullOrWhiteSpace(hex))
        {
            return DependencyProperty.UnsetValue;
        }

        try
        {
            if (ColorConverter.ConvertFromString(hex) is Color color)
            {
                var brush = new SolidColorBrush(color);
                brush.Freeze();
                return brush;
            }
        }
        catch (FormatException)
        {
            // Gebruiker is nog aan het typen, of heeft iets ongeldigs ingevuld: dan gewoon de
            // standaardkleur van de besturingselement laten staan in plaats van te crashen.
        }

        return DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
