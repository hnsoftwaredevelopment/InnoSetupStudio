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

    // Bijgehouden zodra een project succesvol is opgeslagen via ProjectSettingsWindow, zodat
    // toekomstige functionaliteit (zoals "Installer bouwen") weet welk project actief is zonder
    // het bestand opnieuw van schijf te hoeven laden.
    private InstallerProject? _activeProject;
    private string? _activeProjectFilePath;

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

    private async void WizardScreensButton_Click(object sender, RoutedEventArgs e)
    {
        if (_activeProject is null)
        {
            return;
        }

        var viewModel = new WizardScreensViewModel(_activeProject.WizardScreens);
        var window = new WizardScreensWindow(viewModel) { Owner = this };

        if (window.ShowDialog() != true)
        {
            return;
        }

        _activeProject.WizardScreens = viewModel.ToSelection();

        if (string.IsNullOrWhiteSpace(_activeProjectFilePath))
        {
            return;
        }

        // Knop uitschakelen tijdens het opslaan: zonder deze guard kan een tweede klik tijdens de
        // lopende await hetzelfde .tmp-tijdelijke bestand gebruiken als de eerste, wat tot een
        // conflict tussen beide schrijfacties kan leiden.
        WizardScreensButton.IsEnabled = false;
        try
        {
            await _projectService.SaveAsync(_activeProjectFilePath, _activeProject);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Inno Setup Studio", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            WizardScreensButton.IsEnabled = true;
        }
    }

    private async void OpenProjectButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = LocalizationManager.Instance["DialogFilterProjectFiles"],
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

        // Een geopend (dus al bestaand, geldig opgeslagen) project is meteen het actieve project,
        // ook als de gebruiker het zojuist geopende projectinstellingen-scherm annuleert: die
        // annulering betekent alleen dat de algemene instellingen niet gewijzigd zijn, niet dat
        // het project niet meer "open" is.
        SetActiveProject(project, dialog.FileName);

        OpenProjectSettings(project, dialog.FileName);
    }

    private void OpenProjectSettings(InstallerProject project, string? projectFilePath)
    {
        var viewModel = new ProjectSettingsViewModel(project, _projectService, projectFilePath);
        var window = new ProjectSettingsWindow(viewModel) { Owner = this };

        if (window.ShowDialog() == true)
        {
            // Alleen bij een succesvolle Opslaan (DialogResult true) zijn SavedProject en
            // SavedProjectFilePath gevuld.
            SetActiveProject(viewModel.SavedProject, viewModel.SavedProjectFilePath);
        }
        else if (!string.IsNullOrWhiteSpace(projectFilePath))
        {
            // Een al bestaand project sluit dit scherm via de knop die nu "Openen" heet in plaats
            // van "Annuleren" (zie ProjectSettingsViewModel.CancelButtonText): het project wordt
            // dan niet verworpen, het blijft gewoon actief met de instellingen zoals ze op schijf
            // stonden vóór dit scherm werd geopend. Alleen bij een nieuw, nog niet opgeslagen
            // project (projectFilePath null) betekent Annuleren wél het project verwerpen, dus
            // blijft er dan geen actief project achter.
            SetActiveProject(project, projectFilePath);
        }
    }

    private void SetActiveProject(InstallerProject? project, string? projectFilePath)
    {
        _activeProject = project;
        _activeProjectFilePath = projectFilePath;
        WizardScreensButton.IsEnabled = _activeProject is not null;
    }
}
