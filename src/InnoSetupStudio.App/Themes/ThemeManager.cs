using System.Windows;

namespace InnoSetupStudio.App.Themes;

/// <summary>
/// Wisselt het actieve kleurthema door de bijbehorende ResourceDictionary te verwisselen in
/// Application.Resources.MergedDictionaries. Werkt zonder herstart omdat alle stijlen
/// DynamicResource gebruiken voor kleuren.
/// </summary>
public static class ThemeManager
{
    private static readonly Dictionary<string, string> ThemePaths = new()
    {
        ["Light"] = "Themes/Colors.Light.xaml",
        ["Dark"] = "Themes/Colors.Dark.xaml",
        ["LightBlue"] = "Themes/Colors.LightBlue.xaml",
        ["DarkBlue"] = "Themes/Colors.DarkBlue.xaml",
        ["Red"] = "Themes/Colors.Red.xaml",
        ["DarkRed"] = "Themes/Colors.DarkRed.xaml",
        ["Green"] = "Themes/Colors.Green.xaml",
        ["DarkGreen"] = "Themes/Colors.DarkGreen.xaml",
        ["Sepia"] = "Themes/Colors.Sepia.xaml"
    };

    public static IReadOnlyCollection<string> AvailableThemes => ThemePaths.Keys;

    public static void ApplyTheme(string themeName)
    {
        if (!ThemePaths.TryGetValue(themeName, out var relativePath))
        {
            relativePath = ThemePaths["Light"];
        }

        var newDictionary = new ResourceDictionary
        {
            Source = new Uri(relativePath, UriKind.Relative)
        };

        var mergedDictionaries = Application.Current.Resources.MergedDictionaries;

        var existingColorDictionary = mergedDictionaries.FirstOrDefault(d =>
            d.Source is not null && d.Source.OriginalString.Contains("Themes/Colors."));

        if (existingColorDictionary is not null)
        {
            var index = mergedDictionaries.IndexOf(existingColorDictionary);
            mergedDictionaries[index] = newDictionary;
        }
        else
        {
            mergedDictionaries.Insert(0, newDictionary);
        }
    }
}
