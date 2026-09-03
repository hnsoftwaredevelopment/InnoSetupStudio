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

    /// <summary>Voor WizardImageFile: de volledige-hoogte afbeelding links op Welkomst-/Voltooid-pagina's.</summary>
    public static ImageSource ResolveWizardImage(string wizardImageFile) =>
        Resolve(wizardImageFile, DefaultWizardImageUri);

    /// <summary>Voor WizardSmallImageFile: de kleine afbeelding rechtsboven op de overige pagina's.</summary>
    public static ImageSource ResolveWizardSmallImage(string wizardSmallImageFile) =>
        Resolve(wizardSmallImageFile, DefaultWizardSmallImageUri);

    private static ImageSource Resolve(string path, string defaultResourceUri)
    {
        if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
        {
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
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
}
