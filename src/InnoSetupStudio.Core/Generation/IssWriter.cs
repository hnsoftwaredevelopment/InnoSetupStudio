using System.Text;

namespace InnoSetupStudio.Core.Generation;

/// <summary>
/// Schrijft de tekst van een .iss-bestand: CRLF-regeleinden, secties met een lege regel ervoor en
/// kopjes die alleen verschijnen als er daadwerkelijk iets onder komt te staan.
/// </summary>
internal sealed class IssWriter
{
    private const string NewLine = "\r\n";

    private readonly StringBuilder _text = new();
    private bool _lastLineBlank = true;
    private bool _lastLineSection;
    private string? _pendingHeading;

    public void Comment(string text) => Line("; " + text);

    /// <summary>
    /// Een kopje als commentaarregel boven een groep richtlijnen. Het wordt pas geschreven bij de
    /// eerste richtlijn of regel die erna komt, zodat een groep zonder inhoud geen los kopje achterlaat.
    /// </summary>
    public void Heading(string text) => _pendingHeading = text;

    public void Blank()
    {
        if (!_lastLineBlank)
        {
            Line(string.Empty);
        }
    }

    public void Section(string name)
    {
        _pendingHeading = null;
        Blank();
        Line("[" + name + "]");
        _lastLineSection = true;
    }

    public void Directive(string key, string value)
    {
        FlushHeading();
        Line(key + "=" + value);
    }

    public void Entry(params string[] parameters)
    {
        FlushHeading();
        Line(string.Join("; ", parameters));
    }

    public static string Quoted(string key, string value) => key + ": \"" + IssEscape.Quoted(value) + "\"";

    public static string Raw(string key, string value) => key + ": " + value;

    public override string ToString() => _text.ToString();

    private void FlushHeading()
    {
        if (_pendingHeading is null)
        {
            return;
        }

        // Direct onder een sectiekop komt het eerste kopje zonder lege regel ertussen.
        if (!_lastLineSection)
        {
            Blank();
        }

        Comment(_pendingHeading);
        _pendingHeading = null;
    }

    private void Line(string text)
    {
        _text.Append(text).Append(NewLine);
        _lastLineBlank = text.Length == 0;
        _lastLineSection = false;
    }
}
