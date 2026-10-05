namespace InnoSetupStudio.Core.Generation;

/// <summary>
/// De twee soorten escaping die een .iss-bestand kent. Inno Setup leest in bijna elke waarde
/// <c>{...}</c> als constante, en in een parameter tussen aanhalingstekens betekent een
/// verdubbeld aanhalingsteken een letterlijk aanhalingsteken.
/// </summary>
public static class IssEscape
{
    /// <summary>
    /// Maakt van letterlijke tekst een waarde zonder constanten: elke <c>{</c> wordt <c>{{</c>.
    /// Een sluitende accolade hoeft niet verdubbeld te worden. Zo wordt een AppId als
    /// <c>{GUID}</c> in het .iss <c>{{GUID}</c>, de vorm die Inno Setup's eigen scripts gebruiken.
    /// </summary>
    public static string Constants(string value) => value.Replace("{", "{{", StringComparison.Ordinal);

    /// <summary>Maakt tekst geschikt voor tussen aanhalingstekens in een parameterregel:
    /// <c>"</c> wordt <c>""</c>.</summary>
    public static string Quoted(string value) => value.Replace("\"", "\"\"", StringComparison.Ordinal);

    /// <summary>True als de tekst een regeleinde bevat. Zo'n waarde kan niet in één regel van het
    /// .iss en zou het bestand beschadigen.</summary>
    public static bool ContainsLineBreak(string value) => value.AsSpan().IndexOfAny('\r', '\n') >= 0;

    /// <summary>
    /// Maakt van een naam een geldige bestands- of mapnaam: tekens die Windows daarin niet toestaat
    /// (zoals <c>"</c>, <c>:</c> en <c>\</c>) worden <c>_</c>, en punten en spaties aan het einde
    /// vallen weg. Nodig voor namen die Inno Setup als map of snelkoppeling aanmaakt: de compiler
    /// weigert bijvoorbeeld een <c>"</c> in de Name-parameter van een [Icons]-regel.
    /// </summary>
    public static string FileSystemName(string value)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var chars = value.Select(c => Array.IndexOf(invalid, c) >= 0 ? '_' : c).ToArray();
        return new string(chars).TrimEnd('.', ' ');
    }
}