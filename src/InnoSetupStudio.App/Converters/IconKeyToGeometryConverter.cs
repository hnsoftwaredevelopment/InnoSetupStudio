using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace InnoSetupStudio.App.Converters;

/// <summary>
/// Zet een iconsleutel (bijvoorbeeld "Folder") om naar de bijbehorende <see cref="Geometry"/> uit
/// Resources/Icons.xaml, zodat een data-gebonden lijst (zoals het wizardschermen-overzicht) per
/// rij een ander icoon kan tonen zonder voor elk icoon een aparte DataTemplate te schrijven.
/// </summary>
public sealed class IconKeyToGeometryConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string key)
        {
            return null;
        }

        return Application.Current.TryFindResource(key) as Geometry;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
