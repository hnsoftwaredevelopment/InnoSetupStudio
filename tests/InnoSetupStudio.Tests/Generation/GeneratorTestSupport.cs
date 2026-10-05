using InnoSetupStudio.Core.Generation;
using InnoSetupStudio.Core.Project;

namespace InnoSetupStudio.Tests.Generation;

/// <summary>Nepomgeving voor de generator: bestaande mappen en bestanden staan in een lijst, zodat
/// de tests niet van de schijf van de ontwikkelaar afhangen.</summary>
internal sealed class FakeGeneratorEnvironment : IGeneratorEnvironment
{
    private readonly HashSet<string> _directories = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _files = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Een omgeving waarin alles bestaat: geen enkele bestandsmelding.</summary>
    public static FakeGeneratorEnvironment Everything => new() { AllExist = true };

    /// <summary>Een omgeving waarin niets bestaat.</summary>
    public static FakeGeneratorEnvironment Nothing => new();

    public bool AllExist { get; init; }

    public FakeGeneratorEnvironment WithDirectory(string path)
    {
        _directories.Add(path);
        return this;
    }

    public FakeGeneratorEnvironment WithFile(string path)
    {
        _files.Add(path);
        return this;
    }

    public bool DirectoryExists(string path) => AllExist || _directories.Contains(path);

    public bool FileExists(string path) => AllExist || _files.Contains(path);
}

internal static class GeneratorTestSupport
{
    public const string SampleAppId = "{8C5C7F52-0F3E-4F5B-9A59-1D2E3F4A5B6C}";

    /// <summary>Een project met precies genoeg gegevens voor een foutloos script.</summary>
    public static InstallerProject SampleProject() => new()
    {
        AppId = SampleAppId,
        AppName = "MijnApp",
        AppVersion = "1.2.3",
        SourceFilesPath = @"C:\Bron\MijnApp",
        MainExecutable = "MijnApp.exe",
        CreateStartMenuIcon = false,
    };

    public static GenerationResult Generate(InstallerProject project, IGeneratorEnvironment? environment = null)
        => new IssGenerator(environment ?? FakeGeneratorEnvironment.Everything).Generate(project);

    /// <summary>De regels van het script, zonder lege regels en zonder commentaar.</summary>
    public static string[] Lines(GenerationResult result)
        => result.Script
            .Split("\r\n", StringSplitOptions.None)
            .Where(line => line.Length > 0 && !line.StartsWith(';'))
            .ToArray();

    public static bool HasIssue(GenerationResult result, GenerationIssueCode code)
        => result.Issues.Any(issue => issue.Code == code);
}
