using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace InnoSetupStudio.App.Services;

/// <summary>
/// Vertaalt een InstallerProject.WizardImageFile/WizardSmallImageFile-pad (of een leeg pad) naar
/// een bindbare ImageSource voor de schermeditor-voorvertoning. Leeg, of een bestand dat niet
/// (meer) bestaat of niet als afbeelding leesbaar is, valt terug op een meegeleverde
/// standaardafbeelding uit InnoSetupStudio.Wizard (zie de Assets-map en het bijbehorende
/// csproj-commentaar daar), zodat de voorvertoning nooit leeg blijft en nooit crasht op een
/// kapot of verplaatst pad.
/// </summary>
public static class WizardImageResolver
{
    private const string DefaultWizardImageUri =
        "pack://application:,,,/InnoSetupStudio.Wizard;component/Assets/WizardImage-Default.bmp";

    private const string DefaultWizardSmallImageUri =
        "pack://application:,,,/InnoSetupStudio.Wizard;component/Assets/WizardSmallImage-Default.bmp";

    // Inno Setup's eigen afmetingen voor WizardImageFile (164:314) en WizardSmallImageFile
    // (55x55): gebruikt als DecodePixelHeight zodat een door de gebruiker gekozen bronbestand
    // (bijvoorbeeld een foto op volle resolutie) niet eerst volledig gedecodeerd wordt voordat de
    // voorvertoning hem verkleind toont. Eén decode-eigenschap (hoogte) volstaat en behoudt de
    // beeldverhouding; beide worden hier vast op de hoogte gezet, niet op de eigen preview-
    // afmeting, zodat dit onafhankelijk blijft van eventuele toekomstige lay-outwijzigingen.
    private const int WizardImageDecodeHeight = 314;
    private const int WizardSmallImageDecodeHeight = 55;

    /// <summary>Voor WizardImageFile: de volledige-hoogte afbeelding links op Welkomst-/Voltooid-pagina's.</summary>
    public static ImageSource ResolveWizardImage(string wizardImageFile) =>
        Resolve(wizardImageFile, DefaultWizardImageUri, WizardImageDecodeHeight);

    /// <summary>Voor WizardSmallImageFile: de kleine afbeelding rechtsboven op de overige pagina's.</summary>
    public static ImageSource ResolveWizardSmallImage(string wizardSmallImageFile) =>
        Resolve(wizardSmallImageFile, DefaultWizardSmallImageUri, WizardSmallImageDecodeHeight);

    private static ImageSource Resolve(string path, string defaultResourceUri, int decodePixelHeight)
    {
        if (!string.IsNullOrWhiteSpace(path) && !IsUncOrDevicePath(path) && File.Exists(path))
        {
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.DecodePixelHeight = decodePixelHeight;
                bitmap.UriSource = new Uri(path, UriKind.Absolute);
                bitmap.EndInit();
                bitmap.Freeze();
                return bitmap;
            }
            catch (Exception ex) when (ex is NotSupportedException or IOException or UnauthorizedAccessException)
            {
                // Bestand bestaat, maar is (nog) niet leesbaar of geen geldig afbeeldingsformaat:
                // terugvallen op de standaardafbeelding in plaats van de schermeditor te laten
                // crashen, zelfde aanpak als LicensePageEditorViewModel.LoadLicenseText.
            }
        }

        var defaultImage = new BitmapImage(new Uri(defaultResourceUri, UriKind.Absolute));
        defaultImage.Freeze();
        return defaultImage;
    }

    // WizardImageFile/WizardSmallImageFile komen niet alleen uit de eigen bladerdialoog van de
    // gebruiker, maar ook rechtstreeks uit een geladen .issproj-projectbestand. Zonder deze check
    // zou het openen van een projectbestand met een UNC-pad (\\host\share\...) hier automatisch,
    // zonder verdere gebruikersactie, een SMB-verbinding naar die host opzetten — zelfde risico en
    // zelfde oplossing als LicensePageEditorViewModel.IsUncOrDevicePath.
    private static bool IsUncOrDevicePath(string path) => path.StartsWith(@"\\", StringComparison.Ordinal);
}
