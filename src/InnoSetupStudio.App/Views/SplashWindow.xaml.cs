using System.Reflection;
using System.Windows;

namespace InnoSetupStudio.App.Views;

public partial class SplashWindow : Window
{
    public SplashWindow()
    {
        InitializeComponent();

        var informationalVersion = Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        VersionText.Text = string.IsNullOrWhiteSpace(informationalVersion)
            ? string.Empty
            : $"Release {informationalVersion}";
    }
}
