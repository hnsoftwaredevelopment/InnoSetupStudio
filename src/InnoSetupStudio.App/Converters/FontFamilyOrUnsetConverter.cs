using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace InnoSetupStudio.App.Converters;

/// <summary>
/// Zet WizardScreenEditorViewModel's EffectiveXxxFontFamily (zie WizardScreenButtonSettings) om
/// naar een System.Windows.Media.FontFamily voor de Terug-/Volgende-/Annuleren-knoppen in de
/// voorvertoning. Zelfde UnsetValue-bij-leeg-conventie als HexColorToBrushConverter: een lege
/// waarde betekent "geen override", niet "gebruik een lege/standaard lettertypenaam" — de knop
/// valt dan terug op zijn eigen stijl-standaardlettertype.
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
        catch (ArgumentException)
        {
            // Ongeldige lettertypenaam (bijvoorbeeld nog aan het typen): geen crash van de
            // voorvertoning, gewoon terugvallen op het stijl-standaardlettertype.
            return DependencyProperty.UnsetValue;
        }
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
