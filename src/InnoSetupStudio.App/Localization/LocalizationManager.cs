using System.ComponentModel;
using System.Globalization;
using InnoSetupStudio.App.Resources;

namespace InnoSetupStudio.App.Localization;

/// <summary>
/// Houdt de actieve UI-taal bij en stelt vertaalde teksten beschikbaar via een indexer,
/// zodat XAML-bindings automatisch verversen wanneer de taal wisselt (geen herstart nodig).
/// </summary>
public sealed class LocalizationManager : INotifyPropertyChanged
{
    public static LocalizationManager Instance { get; } = new();

    /// <summary>
    /// Expliciet bijgehouden actieve cultuur, bewust NIET rechtstreeks
    /// <see cref="CultureInfo.CurrentUICulture"/> in de indexer: dat is een ambient,
    /// per-thread eigenschap die op een achtergrondthread een andere waarde kan hebben dan wat
    /// <see cref="SetLanguage"/> heeft ingesteld. Door een eigen veld te gebruiken dat
    /// uitsluitend door <see cref="SetLanguage"/> wordt gewijzigd, is het resultaat altijd de
    /// laatst ingestelde taal, ongeacht op welke thread of op welk moment een binding wordt
    /// geëvalueerd.
    /// </summary>
    private CultureInfo _activeCulture = CultureInfo.CurrentUICulture;

    private LocalizationManager()
    {
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Haalt de vertaalde tekst voor <paramref name="key"/> op in de actieve taal.</summary>
    public string this[string key] => Strings.ResourceManager.GetString(key, _activeCulture) ?? key;

    public string CurrentLanguage => _activeCulture.Name;

    /// <summary>De enige cultuurnamen waarvoor dit project vertalingen bijhoudt (§5, Strings*.resx).</summary>
    private static readonly HashSet<string> SupportedCultures = new(StringComparer.OrdinalIgnoreCase)
    {
        "nl-NL", "en-US", "de-DE",
    };

    private const string FallbackCultureName = "nl-NL";

    /// <summary>Wisselt de actieve UI-taal en vernieuwt alle gebonden teksten in de UI.</summary>
    public void SetLanguage(string cultureName)
    {
        // Een onbekende of beschadigde instelling (bijvoorbeeld handmatig aangepast
        // settings.json) mag nooit een CultureInfo-exception geven; val dan terug op Nederlands.
        var culture = SupportedCultures.Contains(cultureName)
            ? new CultureInfo(cultureName)
            : new CultureInfo(FallbackCultureName);
        _activeCulture = culture;

        CultureInfo.CurrentUICulture = culture;
        CultureInfo.CurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        CultureInfo.DefaultThreadCurrentCulture = culture;

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentLanguage)));
    }
}
