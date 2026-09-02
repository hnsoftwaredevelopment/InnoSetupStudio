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
    }

    private static void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        e.Handled = true;
        MessageBox.Show(
            e.Exception.Message,
            LocalizationManager.Instance["AppTitle"],
            MessageBoxButton.OK,
            MessageBoxImage.Warning);
    }
}
