namespace InnoSetupStudio.Core.Generation;

/// <summary>Het resultaat van <see cref="IssGenerator.Generate"/>: de scripttekst en de meldingen.</summary>
public sealed class GenerationResult
{
    public GenerationResult(string script, IReadOnlyList<GenerationIssue> issues)
    {
        Script = script;
        Issues = issues;
    }

    /// <summary>De tekst van het .iss-bestand, met CRLF-regeleinden. Bij een fout (zie
    /// <see cref="HasErrors"/>) is dit een onvolledig bestand dat de aanroeper niet wegschrijft.</summary>
    public string Script { get; }

    public IReadOnlyList<GenerationIssue> Issues { get; }

    /// <summary>True als minstens één melding <see cref="GenerationSeverity.Error"/> is.</summary>
    public bool HasErrors => Issues.Any(issue => issue.Severity == GenerationSeverity.Error);
}
