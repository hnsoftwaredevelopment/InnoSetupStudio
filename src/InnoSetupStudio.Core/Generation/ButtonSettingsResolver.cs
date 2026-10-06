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
/// Setup's eigen gedrag blijft. De twee dictionaries bevatten alleen niet-lege vertalingen van het
/// scherm zelf; die cascaderen niet via het Standaardscherm.
/// </summary>
public sealed record EffectiveButtonSettings(
    string Caption,
    bool? Enabled,
    bool? Visible,
    string TextColor,
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
/// het Standaardscherm. Puur en zonder toestand, zodat de generator er rechtstreeks van uit kan gaan.
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
            Text(own.TextColor, fallback.TextColor),
            Text(own.FontFamily, fallback.FontFamily),
            own.FontSize ?? fallback.FontSize,
            own.FontBold ?? fallback.FontBold,
            Text(own.Tooltip, fallback.Tooltip),
            NonBlank(own.CaptionByLanguage),
            NonBlank(own.TooltipByLanguage));
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
            Text(browse.TextColor, string.Empty),
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
        string? TextColor,
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
                    s.BackButtonCaption, s.BackButtonEnabled, s.BackButtonVisible, s.BackButtonTextColor,
                    s.BackButtonFontFamily, s.BackButtonFontSize, s.BackButtonFontBold, s.BackButtonTooltip,
                    s.BackButtonCaptionByLanguage, s.BackButtonTooltipByLanguage),
                WizardButton.Next => new Fields(
                    s.NextButtonCaption, s.NextButtonEnabled, s.NextButtonVisible, s.NextButtonTextColor,
                    s.NextButtonFontFamily, s.NextButtonFontSize, s.NextButtonFontBold, s.NextButtonTooltip,
                    s.NextButtonCaptionByLanguage, s.NextButtonTooltipByLanguage),
                WizardButton.Cancel => new Fields(
                    s.CancelButtonCaption, s.CancelButtonEnabled, s.CancelButtonVisible, s.CancelButtonTextColor,
                    s.CancelButtonFontFamily, s.CancelButtonFontSize, s.CancelButtonFontBold, s.CancelButtonTooltip,
                    s.CancelButtonCaptionByLanguage, s.CancelButtonTooltipByLanguage),
                _ => throw new ArgumentOutOfRangeException(nameof(button), button, null),
            };
        }
    }
}
