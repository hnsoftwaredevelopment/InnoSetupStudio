using System.IO;

namespace InnoSetupStudio.App.Services;

/// <summary>
/// Leest de Syncfusion-licentiesleutel van schijf en registreert die bij het opstarten.
/// De sleutel staat bewust BUITEN de repo (%LocalAppData%\InnoSetupStudio\license), zodat hij
/// nooit per ongeluk meegecomit kan worden — een regel in .gitignore is hooguit een extra
/// vangnet, niet de eerste verdedigingslinie.
/// </summary>
public static class LicenseService
{
    public static string LicenseFilePath { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "InnoSetupStudio", "license", "syncfusionlicense.txt");

    /// <summary>
    /// Registreert de Syncfusion-licentie als het bestand bestaat. Ontbreekt het, dan start de
    /// app gewoon door: Syncfusion-controls tonen dan een watermerk totdat de licentie alsnog op
    /// zijn plek staat.
    /// </summary>
    public static bool TryRegisterSyncfusionLicense()
    {
        if (!File.Exists(LicenseFilePath))
        {
            return false;
        }

        var key = File.ReadAllText(LicenseFilePath).Trim();
        if (string.IsNullOrWhiteSpace(key))
        {
            return false;
        }

        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(key);
        return true;
    }
}
