using InnoSetupStudio.Core.Project;

namespace InnoSetupStudio.Core.Generation;

/// <summary>De drie gedeelde knoppen onder in de wizard.</summary>
public enum WizardButton
{
    Back,
    Next,
    Cancel,
}

/// <summary>
/// De waarden van één knop op één scherm nadat de drie lagen zijn toegepast (eigen waarde op het
/// scherm, anders het Standaardscherm). Een lege tekst of een <c>null</c> betekent: niet ingesteld,
/// Setup's eigen gedrag blijft. De twee dictionaries bevatten alleen niet-lege vertalingen, per taal
/// bepaald met <see cref="ButtonSettingsResolver.ResolveTranslation"/> (eigen vertaling, anders de
/// vertaling van het Standaardscherm, tenzij het scherm zelf een tekst heeft).
/// </summary>
public sealed record EffectiveButtonSettings(
    string Caption,
    bool? Enabled,
    bool? Visible,
    string FontFamily,
    int? FontSize,
    bool? FontBold,
    string Tooltip,
    IReadOnlyDictionary<string, string> CaptionByLanguage,
    IReadOnlyDictionary<string, string> TooltipByLanguage);

/// <summary>
/// Bepaalt welke waarde voor een knop geldt: dezelfde regels als de Effective*-eigenschappen van de
/// schermeditor (zie sectie 5 van docs/Ontwerp-Knopinstellingen-Generator.md). Tekstvelden: eigen
/// waarde als die niet leeg is of alleen uit spaties bestaat, anders die van het Standaardscherm.
/// Lettergrootte, vet, ingeschakeld en zichtbaar: eigen waarde als die is ingesteld, anders die van
/// het Standaardscherm. Vertalingen per taal: zie <see cref="ResolveTranslation"/>. Puur en zonder
/// toestand, zodat de generator er rechtstreeks van uit kan gaan.
/// </summary>
public static class ButtonSettingsResolver
{
    /// <summary>
    /// Effectieve waarden van <paramref name="button"/> op een scherm. <paramref name="screen"/> en
    /// <paramref name="defaults"/> mogen <c>null</c> zijn en gelden dan als "niets ingesteld".
    /// </summary>
    public static EffectiveButtonSettings Resolve(
        WizardScreenButtonSettings? screen,
        WizardScreenButtonSettings? defaults,
        WizardButton button)
    {
        var own = Fields.From(screen, button);
        var fallback = Fields.From(defaults, button);

        return new EffectiveButtonSettings(
            Text(own.Caption, fallback.Caption),
            own.Enabled ?? fallback.Enabled,
            own.Visible ?? fallback.Visible,
            Text(own.FontFamily, fallback.FontFamily),
            own.FontSize ?? fallback.FontSize,
            own.FontBold ?? fallback.FontBold,
            Text(own.Tooltip, fallback.Tooltip),
            Translations(own.Caption, own.CaptionByLanguage, fallback.CaptionByLanguage),
            Translations(own.Tooltip, own.TooltipByLanguage, fallback.TooltipByLanguage));
    }

    /// <summary>
    /// De vertaling die voor één taal geldt, of een lege tekst als er geen vertaling is en de
    /// universele tekst blijft gelden. Eerste regel die van toepassing is: een eigen vertaling van
    /// het scherm; een eigen tekst van het scherm (die geldt dan voor deze taal, zie sectie 2 van
    /// docs/Ontwerp-Vertalingen-Standaardscherm.md); de vertaling van het Standaardscherm.
    /// Tekst met alleen spaties telt als leeg. Publiek zodat de editor dezelfde regel gebruikt.
    /// </summary>
    public static string ResolveTranslation(string? ownText, string? ownTranslation, string? defaultTranslation)
    {
        if (!string.IsNullOrWhiteSpace(ownTranslation))
        {
            return ownTranslation;
        }

        if (!string.IsNullOrWhiteSpace(ownText))
        {
            return string.Empty;
        }

        return string.IsNullOrWhiteSpace(defaultTranslation) ? string.Empty : defaultTranslation;
    }

    /// <summary>Effectieve waarden van een Bladeren-knop: geen Standaardscherm, dus geen cascade.</summary>
    public static EffectiveButtonSettings Resolve(BrowseButtonSettings? browse)
    {
        if (browse is null)
        {
            return Resolve(null, null, WizardButton.Back);
        }

        return new EffectiveButtonSettings(
            Text(browse.Caption, string.Empty),
            browse.Enabled,
            browse.Visible,
            Text(browse.FontFamily, string.Empty),
            browse.FontSize,
            browse.FontBold,
            Text(browse.Tooltip, string.Empty),
            NonBlank(browse.CaptionByLanguage),
            NonBlank(browse.TooltipByLanguage));
    }

    private static string Text(string? own, string? fromDefaults)
    {
        if (!string.IsNullOrWhiteSpace(own))
        {
            return own;
        }

        return string.IsNullOrWhiteSpace(fromDefaults) ? string.Empty : fromDefaults;
    }

    // Per taal die in het scherm of in het Standaardscherm voorkomt de geldende vertaling
    // (ResolveTranslation). Alleen talen met een vertaling komen in het resultaat.
    private static Dictionary<string, string> Translations(
        string? ownText,
        Dictionary<string, string>? own,
        Dictionary<string, string>? fromDefaults)
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        var languages = (own?.Keys ?? Enumerable.Empty<string>()).Concat(fromDefaults?.Keys ?? Enumerable.Empty<string>());

        foreach (var language in languages)
        {
            var text = ResolveTranslation(
                ownText,
                own is not null && own.TryGetValue(language, out var ownTranslation) ? ownTranslation : null,
                fromDefaults is not null && fromDefaults.TryGetValue(language, out var defaultTranslation) ? defaultTranslation : null);

            if (text.Length > 0)
            {
                result[language] = text;
            }
        }

        return result;
    }

    private static Dictionary<string, string> NonBlank(Dictionary<string, string>? translations)
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        if (translations is null)
        {
            return result;
        }

        foreach (var (language, text) in translations)
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                result[language] = text;
            }
        }

        return result;
    }

    // De acht velden en twee vertalingenlijsten van één knop, losgemaakt van de knopnaam.
    private readonly record struct Fields(
        string? Caption,
        bool? Enabled,
        bool? Visible,
        string? FontFamily,
        int? FontSize,
        bool? FontBold,
        string? Tooltip,
        Dictionary<string, string>? CaptionByLanguage,
        Dictionary<string, string>? TooltipByLanguage)
    {
        public static Fields From(WizardScreenButtonSettings? s, WizardButton button)
        {
            if (s is null)
            {
                return default;
            }

            return button switch
            {
                WizardButton.Back => new Fields(
                    s.BackButtonCaption, s.BackButtonEnabled, s.BackButtonVisible,
                    s.BackButtonFontFamily, s.BackButtonFontSize, s.BackButtonFontBold, s.BackButtonTooltip,
                    s.BackButtonCaptionByLanguage, s.BackButtonTooltipByLanguage),
                WizardButton.Next => new Fields(
                    s.NextButtonCaption, s.NextButtonEnabled, s.NextButtonVisible,
                    s.NextButtonFontFamily, s.NextButtonFontSize, s.NextButtonFontBold, s.NextButtonTooltip,
                    s.NextButtonCaptionByLanguage, s.NextButtonTooltipByLanguage),
                WizardButton.Cancel => new Fields(
                    s.CancelButtonCaption, s.CancelButtonEnabled, s.CancelButtonVisible,
                    s.CancelButtonFontFamily, s.CancelButtonFontSize, s.CancelButtonFontBold, s.CancelButtonTooltip,
                    s.CancelButtonCaptionByLanguage, s.CancelButtonTooltipByLanguage),
                _ => throw new ArgumentOutOfRangeException(nameof(button), button, null),
            };
        }
    }
}
