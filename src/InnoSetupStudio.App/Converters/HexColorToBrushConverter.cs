using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace InnoSetupStudio.App.Converters;

/// <summary>
/// Zet WizardScreenEditorViewModel's hex-tekst kleurvelden (EffectiveXxxTextColor/
/// EffectiveXxxBackgroundColor, zie WizardScreenButtonSettings voor waarom dit hex-tekst is in
/// plaats van System.Windows.Media.Color) om naar een Brush voor de Terug-/Volgende-/Annuleren-
/// knoppen in de voorvertoning.
///
/// Geeft bewust DependencyProperty.UnsetValue terug bij een lege of ongeldige waarde, niet
/// bijvoorbeeld Brushes.Transparent: dat laatste zou de knop zichtbaar doorzichtig maken (geen
/// override wordt dan verward met "maak transparant"), terwijl UnsetValue de eigenschap gewoon op
/// zijn eigen stijl-standaardwaarde laat staan — precies wat "leeg = geen wijziging" (zie
/// HintButtonColorFormat) in de voorvertoning moet betekenen.
/// </summary>
public sealed class HexColorToBrushConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        ToBrushOrUnset(value as string);

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();

    // Losse static helper (in plaats van alleen de IValueConverter.Convert-instantiemethode) zodat
    // ButtonBackgroundConverter (bitmap-met-kleur-terugval, zie dat bestand) dezelfde hex-naar-Brush
    // logica kan hergebruiken zonder een IValueConverter te hoeven instantiëren.
    public static object ToBrushOrUnset(string? hex)
    {
        if (string.IsNullOrWhiteSpace(hex))
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
            // Gebruiker is nog aan het typen, of heeft iets ongeldigs ingevuld: geen crash van de
            // voorvertoning, gewoon terugvallen op de stijl-standaardwaarde alsof het veld leeg is.
        }

        return DependencyProperty.UnsetValue;
    }
}
