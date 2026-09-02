using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using InnoSetupStudio.App.Localization;
using InnoSetupStudio.App.Themes;
using InnoSetupStudio.App.ViewModels;
using InnoSetupStudio.App.Views;
using InnoSetupStudio.Core.Project;
using Microsoft.Win32;

namespace InnoSetupStudio.App;

public partial class MainWindow : Window
{
    private readonly IInstallerProjectService _projectService = new JsonInstallerProjectService();

    private static readonly (string CultureName, string DisplayName)[] Languages =
    [
        ("nl-NL", "Nederlands"),
        ("en-US", "English"),
        ("de-DE", "Deutsch")
    ];

    private static readonly (string ThemeKey, string ResourceKey)[] ThemeLabels =
    [
        ("Light", "ThemeLight"),
        ("Dark", "ThemeDark"),
        ("LightBlue", "ThemeLightBlue"),
        ("DarkBlue", "ThemeDarkBlue"),
        ("Red", "ThemeRed"),
        ("DarkRed", "ThemeDarkRed"),
        ("Green", "ThemeGreen"),
        ("DarkGreen", "ThemeDarkGreen"),
        ("Sepia", "ThemeSepia")
    ];

    private bool _isInitializing = true;

    public MainWindow()
    {
        InitializeComponent();

        var informationalVersion = Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        VersionText.Text = string.IsNullOrWhiteSpace(informationalVersion) ? string.Empty : $"v{informationalVersion}";

        foreach (var (cultureName, displayName) in Languages)
        {
            LanguageComboBox.Items.Add(new ComboBoxItem { Content = displayName, Tag = cultureName });
        }

        foreach (var (themeKey, resourceKey) in ThemeLabels)
        {
            ThemeComboBox.Items.Add(new ComboBoxItem { Content = LocalizationManager.Instance[resourceKey], Tag = themeKey });
        }

        LanguageComboBox.SelectedItem = LanguageComboBox.Items.Cast<ComboBoxItem>()
            .FirstOrDefault(i => (string)i.Tag == App.Settings.Current.Language) ?? LanguageComboBox.Items[0];
        ThemeComboBox.SelectedItem = ThemeComboBox.Items.Cast<ComboBoxItem>()
            .FirstOrDefault(i => (string)i.Tag == App.Settings.Current.Theme) ?? ThemeComboBox.Items[0];

        _isInitializing = false;
    }

    private async void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isInitializing || LanguageComboBox.SelectedItem is not ComboBoxItem { Tag: string cultureName })
        {
            return;
        }

        LocalizationManager.Instance.SetLanguage(cultureName);
        App.Settings.Current.Language = cultureName;
        await App.Settings.SaveAsync();

        // Labels van het themadropdown zijn vertaald tekst, dus die na een taalwissel verversen.
        for (var i = 0; i < ThemeComboBox.Items.Count; i++)
        {
            ((ComboBoxItem)ThemeComboBox.Items[i]).Content = LocalizationManager.Instance[ThemeLabels[i].ResourceKey];
        }
    }

    private async void ThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isInitializing || ThemeComboBox.SelectedItem is not ComboBoxItem { Tag: string themeKey })
        {
            return;
        }

        ThemeManager.ApplyTheme(themeKey);
        App.Settings.Current.Theme = themeKey;
        await App.Settings.SaveAsync();
    }

    private void NewProjectButton_Click(object sender, RoutedEventArgs e) =>
        OpenProjectSettings(InstallerProject.CreateNew(), projectFilePath: null);

    private async void OpenProjectButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Inno Setup Studio-project (*.issproj)|*.issproj",
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        InstallerProject project;
        try
        {
            project = await _projectService.LoadAsync(dialog.FileName);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Inno Setup Studio", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        OpenProjectSettings(project, dialog.FileName);
    }

    private void OpenProjectSettings(InstallerProject project, string? projectFilePath)
    {
        var viewModel = new ProjectSettingsViewModel(project, _projectService, projectFilePath);
        var window = new ProjectSettingsWindow(viewModel) { Owner = this };
        window.ShowDialog();
    }
}
