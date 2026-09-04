using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace InnoSetupStudio.App.Converters;

/// <summary>
/// Combineert EffectiveXxxBackgroundColor en EffectiveXxxBitmapFilePath (zie
/// WizardScreenButtonSettings) tot één Background-Brush voor de Terug-/Volgende-/Annuleren-
/// knoppen in de voorvertoning. Een bitmap wint van een achtergrondkleur: dat is ook hoe Inno
/// Setup het zelf zou doen — een zelf-getekende TNewButton met een eigen afbeelding vervangt het
/// hele knopuiterlijk, een kleur en een afbeelding worden niet over elkaar heen gemengd.
///
/// Anders dan WizardImageResolver (WizardImageFile/WizardSmallImageFile) heeft dit geen
/// standaardafbeelding-terugval: een lege/ongeldige bitmap hier betekent "geen override", niet
/// "toon een placeholder" — WizardImageResolver's twee velden zijn altijd zichtbaar op het scherm,
/// deze knopbitmap alleen als de gebruiker er zelf een gekozen heeft.
/// </summary>
public sealed class ButtonBackgroundConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object? parameter, CultureInfo culture)
    {
        var backgroundColorHex = values.Length > 0 ? values[0] as string : null;
        var bitmapPath = values.Length > 1 ? values[1] as string : null;

        if (!string.IsNullOrWhiteSpace(bitmapPath) && !IsUncOrDevicePath(bitmapPath) && File.Exists(bitmapPath))
        {
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = new Uri(bitmapPath, UriKind.Absolute);
                bitmap.EndInit();
                bitmap.Freeze();

                var imageBrush = new ImageBrush(bitmap) { Stretch = Stretch.UniformToFill };
                imageBrush.Freeze();
                return imageBrush;
            }
            catch (Exception ex) when (ex is NotSupportedException or IOException or UnauthorizedAccessException)
            {
                // Bestand bestaat, maar is (nog) niet leesbaar of geen geldig afbeeldingsformaat:
                // terugvallen op de achtergrondkleur hieronder, zelfde aanpak als
                // WizardImageResolver.Resolve en LicensePageEditorViewModel.LoadLicenseText.
            }
        }

        return HexColorToBrushConverter.ToBrushOrUnset(backgroundColorHex);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();

    // Zelfde reden en zelfde oplossing als WizardImageResolver.IsUncOrDevicePath /
    // LicensePageEditorViewModel.IsUncOrDevicePath: BitmapFilePath kan rechtstreeks uit een geladen
    // .issproj-projectbestand komen, dus een UNC-pad moet geblokkeerd worden vóór bestandstoegang.
    private static bool IsUncOrDevicePath(string path) => path.StartsWith(@"\\", StringComparison.Ordinal);
}
