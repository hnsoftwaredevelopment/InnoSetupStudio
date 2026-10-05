using System.Text.RegularExpressions;
using System.Xml.Linq;
using InnoSetupStudio.Core.Generation;

namespace InnoSetupStudio.Tests.Generation;

/// <summary>
/// De generator geeft alleen codes met argumenten; de app zet die om naar tekst via de resx-bestanden
/// (GenerationIssueFormatter). Deze tests lezen de resx-bestanden rechtstreeks en controleren dat elke
/// code en ernst in alle drie de talen een tekst heeft met de juiste plaatsaanduidingen. Een nieuwe
/// <see cref="GenerationIssueCode"/> laat deze tests falen totdat de teksten en het aantal
/// argumenten hieronder zijn toegevoegd.
/// </summary>
public class GenerationIssueResourceTests
{
    private static readonly string[] ResourceFiles = ["Strings.resx", "Strings.en-US.resx", "Strings.de-DE.resx"];

    // Het aantal argumenten dat de generator per code meegeeft (zie IssGenerator).
    private static readonly Dictionary<GenerationIssueCode, int> ArgumentCounts = new()
    {
        [GenerationIssueCode.AppIdMissing] = 0,
        [GenerationIssueCode.AppNameMissing] = 0,
        [GenerationIssueCode.AppVersionMissing] = 0,
        [GenerationIssueCode.SourceFilesPathMissing] = 0,
        [GenerationIssueCode.ValueContainsLineBreak] = 1,
        [GenerationIssueCode.SourceFilesPathNotFound] = 1,
        [GenerationIssueCode.PageWithoutFile] = 1,
        [GenerationIssueCode.FileNotFound] = 2,
        [GenerationIssueCode.MainExecutableInvalid] = 1,
        [GenerationIssueCode.MainExecutableNotFound] = 1,
        [GenerationIssueCode.ShortcutsWithoutMainExecutable] = 0,
        [GenerationIssueCode.ComponentsPageNotSupported] = 0,
        [GenerationIssueCode.TasksPageWithoutTasks] = 0,
        [GenerationIssueCode.TasksPageShownForDesktopIcon] = 0,
        [GenerationIssueCode.UnknownLanguage] = 1,
        [GenerationIssueCode.ButtonSettingsNotGenerated] = 1,
        [GenerationIssueCode.OutputBaseFilenameDefaulted] = 1,
    };

    [Fact]
    public void Every_issue_code_has_an_argument_count()
        => Assert.Equal(Enum.GetValues<GenerationIssueCode>().Order(), ArgumentCounts.Keys.Order());

    [Theory]
    [MemberData(nameof(Files))]
    public void Every_issue_code_has_a_text_with_matching_placeholders(string file)
    {
        var strings = Load(file);

        foreach (var (code, argumentCount) in ArgumentCounts)
        {
            var key = "GenIssue_" + code;
            Assert.True(strings.TryGetValue(key, out var text) && !string.IsNullOrWhiteSpace(text), $"{file}: {key} ontbreekt of is leeg");

            var placeholders = Regex.Matches(text!, @"\{(\d+)\}").Select(m => int.Parse(m.Groups[1].Value)).Distinct().Order().ToArray();
            Assert.True(
                placeholders.SequenceEqual(Enumerable.Range(0, argumentCount)),
                $"{file}: {key} heeft plaatsaanduidingen [{string.Join(",", placeholders)}], verwacht {argumentCount} argument(en)");
        }
    }

    [Theory]
    [MemberData(nameof(Files))]
    public void Every_severity_has_a_text(string file)
    {
        var strings = Load(file);

        foreach (var severity in Enum.GetValues<GenerationSeverity>())
        {
            var key = "GenSeverity_" + severity;
            Assert.True(strings.TryGetValue(key, out var text) && !string.IsNullOrWhiteSpace(text), $"{file}: {key} ontbreekt of is leeg");
        }
    }

    [Theory]
    [MemberData(nameof(Files))]
    public void Generate_dialog_texts_exist(string file)
    {
        var strings = Load(file);

        foreach (var key in new[]
        {
            "ButtonGenerateScript", "DialogFilterScriptFiles", "GenerateUnsavedMessage", "GenerateResultTitle",
            "GenerateResultWrittenFormat", "GenerateResultNotWritten", "GenerateResultNoIssues",
            "GenerateOpenFolder", "GenerateWriteFailedFormat", "GenerateTargetIsProjectFile",
        })
        {
            Assert.True(strings.TryGetValue(key, out var text) && !string.IsNullOrWhiteSpace(text), $"{file}: {key} ontbreekt of is leeg");
        }

        Assert.Contains("{0}", strings["GenerateResultWrittenFormat"]);
        Assert.Contains("{0}", strings["GenerateWriteFailedFormat"]);
    }

    public static TheoryData<string> Files() => new(ResourceFiles);

    private static Dictionary<string, string> Load(string file)
    {
        var path = Path.Combine(FindRepositoryRoot(), "src", "InnoSetupStudio.App", "Resources", file);
        return XDocument.Load(path).Root!
            .Elements("data")
            .ToDictionary(e => (string)e.Attribute("name")!, e => (string?)e.Element("value") ?? string.Empty);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !Directory.Exists(Path.Combine(directory.FullName, "src", "InnoSetupStudio.App", "Resources")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("Repositorymap niet gevonden vanaf " + AppContext.BaseDirectory);
    }
}
