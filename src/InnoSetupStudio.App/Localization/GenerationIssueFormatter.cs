using System.Globalization;
using InnoSetupStudio.App.ViewModels;
using InnoSetupStudio.Core.Generation;

namespace InnoSetupStudio.App.Localization;

/// <summary>
/// Zet de meldingen van de generator om naar tekst in de actieve UI-taal. De generator zelf levert
/// alleen een code met argumenten (zie <see cref="GenerationIssue"/>), zodat Core geen resx-bestanden
/// nodig heeft. Elke <see cref="GenerationIssueCode"/> heeft een resx-sleutel
/// <c>GenIssue_&lt;Code&gt;</c> en elke <see cref="GenerationSeverity"/> een sleutel
/// <c>GenSeverity_&lt;Severity&gt;</c>; de plaatsaanduidingen <c>{0}</c>, <c>{1}</c> volgen de volgorde
/// van <see cref="GenerationIssue.Arguments"/>. Een unittest controleert dat alle sleutels in NL, EN
/// en DE bestaan.
/// </summary>
public static class GenerationIssueFormatter
{
    public static string Message(GenerationIssue issue)
    {
        var template = LocalizationManager.Instance["GenIssue_" + issue.Code];
        try
        {
            return string.Format(CultureInfo.CurrentUICulture, template, issue.Arguments.Cast<object>().ToArray());
        }
        catch (FormatException)
        {
            // Een beschadigde vertaling mag het resultaatvenster niet laten crashen.
            return template;
        }
    }

    public static string SeverityLabel(GenerationSeverity severity)
        => LocalizationManager.Instance["GenSeverity_" + severity];

    /// <summary>De meldingen van een resultaat als rijen voor het resultaatvenster: fouten eerst,
    /// dan waarschuwingen, dan info, binnen een ernst in de volgorde van de generator.</summary>
    public static IReadOnlyList<GenerationIssueRow> Rows(GenerationResult result)
        => result.Issues
            .Select((issue, index) => (issue, index))
            .OrderByDescending(item => item.issue.Severity)
            .ThenBy(item => item.index)
            .Select(item => new GenerationIssueRow(item.issue.Severity, SeverityLabel(item.issue.Severity), Message(item.issue)))
            .ToList();
}
