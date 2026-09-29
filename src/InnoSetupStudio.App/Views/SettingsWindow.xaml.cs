using System.Windows;
using System.Windows.Controls;
using InnoSetupStudio.App.Localization;
using InnoSetupStudio.App.Themes;

namespace InnoSetupStudio.App.Views;

/// <summary>
/// Instellingenvenster voor de IDE zelf (taal, thema): losgetrokken uit MainWindow (sectie 21).
/// Vrijwel een letterlijke verhuizing van wat MainWindow.xaml.cs hiervoor zelf deed — alleen de
/// twee ComboBoxen en hun opslaglogica, verder ongewijzigd.
/// </summary>
public partial class SettingsWindow : Window
{
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

    public SettingsWindow()
    {
        InitializeComponent();

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
}
