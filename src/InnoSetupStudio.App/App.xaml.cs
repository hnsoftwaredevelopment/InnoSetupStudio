using System.Windows;
using System.Windows.Threading;
using InnoSetupStudio.App.Localization;
using InnoSetupStudio.App.Services;
using InnoSetupStudio.App.Themes;
using InnoSetupStudio.App.Views;
using InnoSetupStudio.Core.Settings;

namespace InnoSetupStudio.App;

/// <summary>
/// Compositieroot van de applicatie: registreert de Syncfusion-licentie, laadt instellingen,
/// past taal/thema toe en toont het splashscreen vóórdat het hoofdvenster verschijnt.
/// </summary>
public partial class App : Application
{
    public static ISettingsService Settings { get; private set; } = new JsonSettingsService();

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Vangnet: zonder deze handler laat WPF elke onverwachte fout op de UI-thread de hele
        // app hard crashen. Toon de fout en blijf draaien in plaats van af te sluiten.
        DispatcherUnhandledException += OnDispatcherUnhandledException;

        // Zolang er nog geen enkel venster staat, mag WPF niet zelf afsluiten zodra het
        // splashscreen straks weer dichtgaat (standaard ShutdownMode.OnLastWindowClose). Zonder
        // dit kan een trage of falende opstartstap de app onzichtbaar laten verdwijnen voordat
        // het hoofdvenster ooit verschijnt.
        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        // Alles vóór een zichtbaar hoofdvenster valt onder dezelfde foutafhandeling: ook een
        // fout in het construeren/tonen van SplashWindow of MainWindow zelf (bijvoorbeeld een
        // XAML- of resource-fout) mag nooit een onzichtbaar "zombie"-proces opleveren. Zolang
        // ShutdownMode op OnExplicitShutdown staat, moet elk pad hier eindigen in ofwel een
        // zichtbaar hoofdvenster, ofwel een expliciete Shutdown(-1).
        try
        {
            LicenseService.TryRegisterSyncfusionLicense();

            await Settings.LoadAsync();

            // Taal/thema moeten vaststaan VOORDAT er iets wordt getoond, anders rendert de eerste
            // frame nog met de systeemstandaard totdat de gebruiker zelf iets wisselt.
            ThemeManager.ApplyTheme(Settings.Current.Theme);
            LocalizationManager.Instance.SetLanguage(Settings.Current.Language);

            var splash = new SplashWindow();
            splash.Show();

            // Plek voor toekomstige zwaardere opstart-stappen (project laden, etc.); voor de
            // scaffolding-fase is er niets te wachten, dus het splashscreen is kort zichtbaar.
            await Task.Delay(600);

            var mainWindow = new MainWindow();
            MainWindow = mainWindow;
            mainWindow.Show();

            splash.Close();

            // Vanaf hier is er een hoofdvenster: normaal gedrag herstellen zodat de app afsluit
            // zodra de gebruiker dat venster sluit.
            ShutdownMode = ShutdownMode.OnMainWindowClose;
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.Message,
                "Inno Setup Studio",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown(-1);
        }
    }

    private static void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        e.Handled = true;
        try
        {
            // Tot nu toe zag de gebruiker (en wijzelf bij een bugreport) alleen e.Exception.Message
            // in de MessageBox hieronder — zonder stacktrace of inner exceptions was root-causen
            // van een onverwachte fout achteraf vrijwel onmogelijk. Dit bestand staat naast de
            // .exe, dus het pad blijft geldig ongeacht waar de app geïnstalleerd/uitgepakt is —
            // BEHALVE als die map (bijvoorbeeld Program Files) niet schrijfbaar is voor de
            // huidige gebruiker: dan valt dit terug op de per-gebruiker AppData-map, die altijd
            // schrijfbaar is. Zonder deze fallback verdween het crashlogboek stilletjes in precies
            // het scenario waarin je het het hardst nodig hebt (een geïnstalleerde, niet-portable
            // versie van de app).
            var logContent = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}{Environment.NewLine}{e.Exception}";
            try
            {
                var logPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "crash-log.txt");
                System.IO.File.WriteAllText(logPath, logContent);
            }
            catch (Exception)
            {
                var fallbackDir = System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "InnoSetupStudio");
                System.IO.Directory.CreateDirectory(fallbackDir);
                System.IO.File.WriteAllText(System.IO.Path.Combine(fallbackDir, "crash-log.txt"), logContent);
            }
        }
        catch
        {
            // Diagnostisch vangnet zelf mag nooit een nieuwe onverwerkte fout opleveren.
        }
        MessageBox.Show(
            e.Exception.Message,
            LocalizationManager.Instance["AppTitle"],
            MessageBoxButton.OK,
            MessageBoxImage.Warning);
    }
}
