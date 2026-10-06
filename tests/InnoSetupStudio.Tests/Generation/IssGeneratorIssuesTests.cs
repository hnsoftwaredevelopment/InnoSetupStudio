using InnoSetupStudio.Core.Generation;
using InnoSetupStudio.Core.Project;
using static InnoSetupStudio.Tests.Generation.GeneratorTestSupport;

namespace InnoSetupStudio.Tests.Generation;

public class IssGeneratorIssuesTests
{
    // ---- escaping ---------------------------------------------------------------------------

    [Theory]
    [InlineData("", "")]
    [InlineData("MijnApp", "MijnApp")]
    [InlineData("{GUID}", "{{GUID}")]
    [InlineData("a{b}{c}", "a{{b}{{c}")]
    public void Constants_escape_the_opening_brace(string value, string expected)
        => Assert.Equal(expected, IssEscape.Constants(value));

    [Theory]
    [InlineData("MijnApp", "MijnApp")]
    [InlineData("Mijn \"App\"", "Mijn \"\"App\"\"")]
    public void Quoted_doubles_the_double_quote(string value, string expected)
        => Assert.Equal(expected, IssEscape.Quoted(value));

    [Theory]
    [InlineData("a\nb", true)]
    [InlineData("a\rb", true)]
    [InlineData("a\r\nb", true)]
    [InlineData("a b", false)]
    public void Line_breaks_are_detected(string value, bool expected)
        => Assert.Equal(expected, IssEscape.ContainsLineBreak(value));

    [Fact]
    public void Braces_in_literal_values_are_escaped()
    {
        var project = SampleProject();
        project.AppName = "Mijn {App}";
        project.Publisher = "Voortman {NL}";

        var lines = Lines(Generate(project));

        Assert.Contains("AppName=Mijn {{App}", lines);
        Assert.Contains("AppPublisher=Voortman {{NL}", lines);
        Assert.Contains("UninstallDisplayName=Mijn {{App}", lines);
        Assert.Contains(@"DefaultDirName={autopf}\Mijn {{App}", lines);
    }

    [Fact]
    public void Braces_in_compile_time_paths_are_not_escaped()
    {
        // Source, OutputDir, OutputBaseFilename en de bestandsrichtlijnen worden door de compiler
        // zelf gelezen, niet door Setup: daar is "{{" geen escape maar letterlijke tekst (ISCC-test).
        var project = SampleProject();
        project.SourceFilesPath = @"C:\Bron\{test}";
        project.OutputPath = @"C:\Uit\{test}";
        project.OutputBaseFilename = "Naam{test}";
        project.WizardScreens.ShowLicensePage = true;
        project.LicenseFilePath = @"C:\Docs\{test}\licentie.txt";

        var lines = Lines(Generate(project));

        Assert.Contains(
            @"Source: ""C:\Bron\{test}\*""; DestDir: ""{app}""; Flags: ignoreversion recursesubdirs createallsubdirs",
            lines);
        Assert.Contains(@"OutputDir=C:\Uit\{test}", lines);
        Assert.Contains("OutputBaseFilename=Naam{test}", lines);
        Assert.Contains(@"LicenseFile=C:\Docs\{test}\licentie.txt", lines);
    }
    [Theory]
    [InlineData("MijnApp", "MijnApp")]
    [InlineData("Mijn \"App\"", "Mijn _App_")]
    [InlineData("A:B/C\\D", "A_B_C_D")]
    [InlineData("Naam. ", "Naam")]
    [InlineData("Mijn {App}", "Mijn {App}")]
    [InlineData("...", "App")]
    [InlineData("", "App")]
    [InlineData("A\u0001B", "A_B")]
    [InlineData("A|B?C*D<E>", "A_B_C_D_E_")]
    public void File_system_name_replaces_characters_windows_does_not_allow(string value, string expected)
        => Assert.Equal(expected, IssEscape.FileSystemName(value));

    [Fact]
    public void Quotes_in_the_app_name_are_replaced_in_shortcut_and_folder_names()
    {
        var project = SampleProject();
        project.AppName = "Mijn \"App\"";
        project.CreateStartMenuIcon = true;

        var lines = Lines(Generate(project));

        Assert.Contains("AppName=Mijn \"App\"", lines);
        Assert.Contains(@"DefaultDirName={autopf}\Mijn _App_", lines);
        Assert.Contains("DefaultGroupName=Mijn _App_", lines);
        Assert.Contains(@"Name: ""{group}\Mijn _App_""; Filename: ""{app}\MijnApp.exe""; WorkingDir: ""{app}""", lines);
    }

    // ---- ontbrekende en ongeldige gegevens ----------------------------------------------------

    [Fact]
    public void Empty_project_reports_every_missing_required_value()
    {
        var result = Generate(new InstallerProject());

        Assert.True(result.HasErrors);
        Assert.True(HasIssue(result, GenerationIssueCode.AppIdMissing));
        Assert.True(HasIssue(result, GenerationIssueCode.AppNameMissing));
        Assert.True(HasIssue(result, GenerationIssueCode.AppVersionMissing));
        Assert.True(HasIssue(result, GenerationIssueCode.SourceFilesPathMissing));
        Assert.All(
            result.Issues.Where(i => i.Code is GenerationIssueCode.AppIdMissing or GenerationIssueCode.AppNameMissing or GenerationIssueCode.AppVersionMissing or GenerationIssueCode.SourceFilesPathMissing),
            i => Assert.Equal(GenerationSeverity.Error, i.Severity));
    }

    [Fact]
    public void Empty_project_still_gives_a_script_without_the_missing_directives()
    {
        var lines = Lines(Generate(new InstallerProject()));

        Assert.Contains("[Setup]", lines);
        Assert.DoesNotContain(lines, l => l.StartsWith("AppId=", StringComparison.Ordinal));
        Assert.DoesNotContain(lines, l => l.StartsWith("AppName=", StringComparison.Ordinal));
        Assert.DoesNotContain("[Files]", lines);
    }

    [Fact]
    public void Value_with_a_line_break_is_an_error_and_is_left_out()
    {
        var project = SampleProject();
        project.Publisher = "Voortman\r\nAppName=Kwaad";

        var result = Generate(project);

        Assert.True(result.HasErrors);
        var issue = Assert.Single(result.Issues, i => i.Code == GenerationIssueCode.ValueContainsLineBreak);
        Assert.Equal(GenerationSeverity.Error, issue.Severity);
        Assert.Equal(new[] { "Publisher" }, issue.Arguments);
        Assert.DoesNotContain("Kwaad", result.Script);
    }

    [Fact]
    public void Source_folder_that_does_not_exist_is_a_warning()
    {
        var result = Generate(SampleProject(), FakeGeneratorEnvironment.Nothing);

        var issue = Assert.Single(result.Issues);
        Assert.Equal(GenerationSeverity.Warning, issue.Severity);
        Assert.Equal(GenerationIssueCode.SourceFilesPathNotFound, issue.Code);
        Assert.Equal(new[] { @"C:\Bron\MijnApp" }, issue.Arguments);
        Assert.False(result.HasErrors);
    }

    [Fact]
    public void Main_executable_that_is_not_in_the_source_folder_is_a_warning()
    {
        var result = Generate(SampleProject(), new FakeGeneratorEnvironment().WithDirectory(@"C:\Bron\MijnApp"));

        var issue = Assert.Single(result.Issues);
        Assert.Equal(GenerationIssueCode.MainExecutableNotFound, issue.Code);
        Assert.Equal(new[] { "MijnApp.exe" }, issue.Arguments);
    }

    [Fact]
    public void Main_executable_in_a_subfolder_is_written_with_backslashes()
    {
        var project = SampleProject();
        project.MainExecutable = "bin/MijnApp.exe";
        project.CreateStartMenuIcon = true;

        var lines = Lines(Generate(project));

        Assert.Contains(@"UninstallDisplayIcon={app}\bin\MijnApp.exe", lines);
        Assert.Contains(@"Name: ""{group}\MijnApp""; Filename: ""{app}\bin\MijnApp.exe""; WorkingDir: ""{app}""", lines);
    }

    [Theory]
    [InlineData(@"..\Ander.exe")]
    [InlineData(@"C:\Ander.exe")]
    [InlineData(@"a\\b.exe")]
    public void Invalid_main_executable_is_a_warning_and_gives_no_shortcuts(string main)
    {
        var project = SampleProject();
        project.MainExecutable = main;
        project.CreateStartMenuIcon = true;

        var result = Generate(project);
        var lines = Lines(result);

        var issue = Assert.Single(result.Issues);
        Assert.Equal(GenerationIssueCode.MainExecutableInvalid, issue.Code);
        Assert.DoesNotContain("[Icons]", lines);
        Assert.DoesNotContain(lines, l => l.StartsWith("UninstallDisplayIcon=", StringComparison.Ordinal));
    }

    // ---- snelkoppelingen en taken -------------------------------------------------------------

    [Fact]
    public void Start_menu_shortcut_is_written_without_a_task()
    {
        var project = SampleProject();
        project.CreateStartMenuIcon = true;

        var lines = Lines(Generate(project));

        Assert.Contains("[Icons]", lines);
        Assert.Contains(@"Name: ""{group}\MijnApp""; Filename: ""{app}\MijnApp.exe""; WorkingDir: ""{app}""", lines);
        Assert.DoesNotContain("[Tasks]", lines);
    }

    [Fact]
    public void Desktop_shortcut_gets_an_unchecked_task()
    {
        var project = SampleProject();
        project.CreateDesktopIcon = true;
        project.WizardScreens.ShowSelectTasksPage = true;

        var result = Generate(project);
        var lines = Lines(result);

        Assert.Contains("[Tasks]", lines);
        Assert.Contains(@"Name: ""desktopicon""; Description: ""{cm:CreateDesktopIcon}""; GroupDescription: ""{cm:AdditionalIcons}""; Flags: unchecked", lines);
        Assert.Contains(@"Name: ""{autodesktop}\MijnApp""; Filename: ""{app}\MijnApp.exe""; WorkingDir: ""{app}""; Tasks: desktopicon", lines);
        Assert.Empty(result.Issues);
    }

    [Fact]
    public void Desktop_shortcut_without_tasks_page_gives_a_warning()
    {
        var project = SampleProject();
        project.CreateDesktopIcon = true;

        var result = Generate(project);

        Assert.True(HasIssue(result, GenerationIssueCode.TasksPageShownForDesktopIcon));
        Assert.Contains("[Tasks]", Lines(result));
    }

    [Fact]
    public void Tasks_page_without_tasks_gives_a_warning()
    {
        var project = SampleProject();
        project.WizardScreens.ShowSelectTasksPage = true;

        var result = Generate(project);

        Assert.True(HasIssue(result, GenerationIssueCode.TasksPageWithoutTasks));
        Assert.DoesNotContain("[Tasks]", Lines(result));
    }

    [Fact]
    public void Shortcuts_without_a_main_executable_give_a_warning_and_no_icons_section()
    {
        var project = SampleProject();
        project.MainExecutable = string.Empty;
        project.CreateStartMenuIcon = true;

        var result = Generate(project);

        Assert.True(HasIssue(result, GenerationIssueCode.ShortcutsWithoutMainExecutable));
        Assert.DoesNotContain("[Icons]", Lines(result));
    }

    [Fact]
    public void Components_page_is_reported_as_not_supported()
    {
        var project = SampleProject();
        project.WizardScreens.ShowSelectComponentsPage = true;

        var issue = Assert.Single(Generate(project).Issues);

        Assert.Equal(GenerationSeverity.Warning, issue.Severity);
        Assert.Equal(GenerationIssueCode.ComponentsPageNotSupported, issue.Code);
    }

    // ---- talen ------------------------------------------------------------------------------

    [Fact]
    public void Default_project_has_only_english()
    {
        var lines = Lines(Generate(SampleProject()));

        Assert.Contains(@"Name: ""english""; MessagesFile: ""compiler:Default.isl""", lines);
        Assert.Single(lines, l => l.StartsWith("Name: \"", StringComparison.Ordinal) && l.Contains("MessagesFile"));
    }

    [Fact]
    public void Languages_follow_the_catalog_order_with_english_first()
    {
        var project = SampleProject();
        project.SupportedLanguageIds = ["german", "dutch", "english"];

        var languages = Lines(Generate(project))
            .Where(l => l.Contains("MessagesFile", StringComparison.Ordinal))
            .ToArray();

        Assert.Equal(
            new[]
            {
                @"Name: ""english""; MessagesFile: ""compiler:Default.isl""",
                @"Name: ""dutch""; MessagesFile: ""compiler:Languages\Dutch.isl""",
                @"Name: ""german""; MessagesFile: ""compiler:Languages\German.isl""",
            },
            languages);
    }

    [Fact]
    public void Dutch_only_project_still_lists_english_first()
    {
        var project = SampleProject();
        project.SupportedLanguageIds = ["dutch"];

        var languages = Lines(Generate(project)).Where(l => l.Contains("MessagesFile", StringComparison.Ordinal)).ToArray();

        Assert.Equal(2, languages.Length);
        Assert.StartsWith(@"Name: ""english""", languages[0]);
    }

    [Fact]
    public void Unknown_language_is_a_warning_and_is_skipped()
    {
        var project = SampleProject();
        project.SupportedLanguageIds = ["english", "klingon"];

        var result = Generate(project);

        var issue = Assert.Single(result.Issues);
        Assert.Equal(GenerationIssueCode.UnknownLanguage, issue.Code);
        Assert.Equal(new[] { "klingon" }, issue.Arguments);
        Assert.DoesNotContain("klingon", result.Script);
    }

    // ---- knopinstellingen -----------------------------------------------------------------------

    [Fact]
    public void Untouched_button_settings_give_no_message()
        => Assert.Empty(Generate(SampleProject()).Issues);

    // ---- eigenschappen van de uitvoer ---------------------------------------------------------------

    [Fact]
    public void Output_is_deterministic()
    {
        var project = SampleProject();
        project.SupportedLanguageIds = ["german", "dutch", "french"];
        project.CreateDesktopIcon = true;
        project.CreateStartMenuIcon = true;

        Assert.Equal(Generate(project).Script, Generate(project).Script);
    }

    [Fact]
    public void Script_uses_crlf_only()
    {
        var script = Generate(SampleProject()).Script;

        Assert.EndsWith("\r\n", script);
        Assert.DoesNotContain("\n", script.Replace("\r\n", string.Empty, StringComparison.Ordinal));
    }

    [Fact]
    public void Script_starts_with_a_generated_by_comment_and_has_no_double_blank_lines()
    {
        var script = Generate(SampleProject()).Script;

        Assert.StartsWith("; Gegenereerd door Inno Setup Studio", script);
        Assert.DoesNotContain("\r\n\r\n\r\n", script);
    }

    [Fact]
    public void Script_encoding_is_utf8_with_bom()
        => Assert.Equal(new byte[] { 0xEF, 0xBB, 0xBF }, IssGenerator.ScriptEncoding.GetPreamble());

    [Fact]
    public void Generate_does_not_change_the_project()
    {
        var project = SampleProject();
        project.AppName = "  Spaties  ";
        project.SupportedLanguageIds = ["dutch"];

        Generate(project);

        Assert.Equal("  Spaties  ", project.AppName);
        Assert.Equal(new[] { "dutch" }, project.SupportedLanguageIds);
    }

    [Fact]
    public void Generate_rejects_null()
        => Assert.Throws<ArgumentNullException>(() => new IssGenerator().Generate(null!));

    [Fact]
    public void Empty_installer_file_name_is_reported_with_the_default_name()
    {
        var project = SampleProject();
        project.OutputBaseFilename = "  ";

        var result = Generate(project);

        var issue = Assert.Single(result.Issues, i => i.Code == GenerationIssueCode.OutputBaseFilenameDefaulted);
        Assert.Equal(GenerationSeverity.Info, issue.Severity);
        Assert.Equal(new[] { "MijnApp-1.2.3-Setup" }, issue.Arguments);
        Assert.Contains("OutputBaseFilename=MijnApp-1.2.3-Setup", Lines(result));
        Assert.False(result.HasErrors);
    }

    [Fact]
    public void Entered_installer_file_name_is_not_reported()
    {
        var project = SampleProject();
        project.OutputBaseFilename = "MijnSetup";

        var result = Generate(project);

        Assert.False(HasIssue(result, GenerationIssueCode.OutputBaseFilenameDefaulted));
        Assert.Contains("OutputBaseFilename=MijnSetup", Lines(result));
    }
}
