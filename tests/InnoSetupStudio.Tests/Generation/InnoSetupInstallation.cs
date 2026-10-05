using System.Diagnostics;
using System.Text;

namespace InnoSetupStudio.Tests.Generation;

/// <summary>
/// Zoekt een Inno Setup-installatie op de machine. De tests die ISCC (de Inno Setup-compiler)
/// gebruiken worden overgeslagen als die er niet staat, bijvoorbeeld op een build-server. Een
/// andere locatie kan met de omgevingsvariabele INNO_SETUP_DIR worden opgegeven.
/// </summary>
internal static class InnoSetupInstallation
{
    private static readonly Lazy<string?> Located = new(Find);

    /// <summary>De map met ISCC.exe, of null als Inno Setup niet is gevonden.</summary>
    public static string? Folder => Located.Value;

    public static string? IsccPath => Folder is null ? null : System.IO.Path.Combine(Folder, "ISCC.exe");

    public static string SkipReason => "Inno Setup (ISCC.exe) is niet gevonden. Zet INNO_SETUP_DIR of installeer Inno Setup 6 of 7.";

    private static string? Find()
    {
        var candidates = new List<string?> { Environment.GetEnvironmentVariable("INNO_SETUP_DIR") };
        foreach (var root in new[]
        {
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
        })
        {
            if (!string.IsNullOrEmpty(root))
            {
                candidates.Add(System.IO.Path.Combine(root, "Inno Setup 7"));
                candidates.Add(System.IO.Path.Combine(root, "Inno Setup 6"));
            }
        }

        return candidates.FirstOrDefault(c => !string.IsNullOrEmpty(c) && File.Exists(System.IO.Path.Combine(c, "ISCC.exe")));
    }

    /// <summary>Start ISCC op het opgegeven script en geeft exitcode en uitvoer terug.</summary>
    public static async Task<CompilerRun> CompileAsync(string scriptPath)
    {
        var start = new ProcessStartInfo(IsccPath!)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
        };
        start.ArgumentList.Add(scriptPath);

        using var process = Process.Start(start) ?? throw new InvalidOperationException("ISCC kon niet worden gestart.");
        var output = process.StandardOutput.ReadToEndAsync();
        var error = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();
        return new CompilerRun(process.ExitCode, await output + await error);
    }
}

internal sealed record CompilerRun(int ExitCode, string Output);

/// <summary>Een [Fact] die wordt overgeslagen zolang Inno Setup niet is geïnstalleerd.</summary>
[AttributeUsage(AttributeTargets.Method)]
internal sealed class IsccFactAttribute : FactAttribute
{
    public IsccFactAttribute()
    {
        if (InnoSetupInstallation.Folder is null)
        {
            Skip = InnoSetupInstallation.SkipReason;
        }
    }
}

/// <summary>Een [Theory] die wordt overgeslagen zolang Inno Setup niet is geïnstalleerd.</summary>
[AttributeUsage(AttributeTargets.Method)]
internal sealed class IsccTheoryAttribute : TheoryAttribute
{
    public IsccTheoryAttribute()
    {
        if (InnoSetupInstallation.Folder is null)
        {
            Skip = InnoSetupInstallation.SkipReason;
        }
    }
}
