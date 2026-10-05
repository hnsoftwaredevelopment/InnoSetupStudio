using InnoSetupStudio.Core.Generation;
using InnoSetupStudio.Core.Project;
using static InnoSetupStudio.Tests.Generation.GeneratorTestSupport;

namespace InnoSetupStudio.Tests.Generation;

public class IssGeneratorTests
{
    // ---- basis ----------------------------------------------------------------------------

    [Fact]
    public void Sample_project_gives_a_script_without_issues()
    {
        var result = Generate(SampleProject());

        Assert.Empty(result.Issues);
        Assert.False(result.HasErrors);
    }

    [Fact]
    public void Basic_data_is_written_with_an_escaped_app_id()
    {
        var lines = Lines(Generate(SampleProject()));

        Assert.Contains("AppId={{8C5C7F52-0F3E-4F5B-9A59-1D2E3F4A5B6C}", lines);
        Assert.Contains("AppName=MijnApp", lines);
        Assert.Contains("AppVersion=1.2.3", lines);
        Assert.Contains(@"DefaultDirName={autopf}\MijnApp", lines);
        Assert.Contains("DefaultGroupName=MijnApp", lines);
        Assert.Contains("Compression=lzma2", lines);
        Assert.Contains("SolidCompression=yes", lines);
        Assert.Contains("UninstallDisplayName=MijnApp", lines);
        Assert.Contains(@"UninstallDisplayIcon={app}\MijnApp.exe", lines);
        Assert.Contains("OutputBaseFilename=MijnApp-1.2.3-Setup", lines);
    }

    [Fact]
    public void Files_section_copies_the_whole_source_folder()
    {
        var lines = Lines(Generate(SampleProject()));

        Assert.Contains("[Files]", lines);
        Assert.Contains(
            @"Source: ""C:\Bron\MijnApp\*""; DestDir: ""{app}""; Flags: ignoreversion recursesubdirs createallsubdirs",
            lines);
    }

    [Fact]
    public void Trailing_separator_of_the_source_folder_is_removed()
    {
        var project = SampleProject();
        project.SourceFilesPath = @"C:\Bron\MijnApp\";

        Assert.Contains(
            @"Source: ""C:\Bron\MijnApp\*""; DestDir: ""{app}""; Flags: ignoreversion recursesubdirs createallsubdirs",
            Lines(Generate(project)));
    }

    [Fact]
    public void Custom_directory_and_group_names_are_written_as_given()
    {
        var project = SampleProject();
        project.DefaultDirName = @"{commonpf}\Voortman\MijnApp";
        project.DefaultGroupName = "Voortman\\MijnApp";

        var lines = Lines(Generate(project));

        Assert.Contains(@"DefaultDirName={commonpf}\Voortman\MijnApp", lines);
        Assert.Contains(@"DefaultGroupName=Voortman\MijnApp", lines);
    }

    [Fact]
    public void Publisher_data_is_written_and_the_url_also_serves_as_support_and_update_url()
    {
        var project = SampleProject();
        project.Publisher = "Voortman Steel Machinery";
        project.PublisherUrl = "https://www.voortman.net";
        project.PublisherEmail = "info@voortman.net";

        var lines = Lines(Generate(project));

        Assert.Contains("AppPublisher=Voortman Steel Machinery", lines);
        Assert.Contains("AppPublisherURL=https://www.voortman.net", lines);
        Assert.Contains("AppSupportURL=https://www.voortman.net", lines);
        Assert.Contains("AppUpdatesURL=https://www.voortman.net", lines);
        Assert.Contains("AppContact=info@voortman.net", lines);
    }

    [Fact]
    public void Empty_optional_data_is_left_out()
    {
        var lines = Lines(Generate(SampleProject()));

        Assert.DoesNotContain(lines, l => l.StartsWith("AppPublisher", StringComparison.Ordinal));
        Assert.DoesNotContain(lines, l => l.StartsWith("AppSupportURL", StringComparison.Ordinal));
        Assert.DoesNotContain(lines, l => l.StartsWith("AppContact", StringComparison.Ordinal));
        Assert.DoesNotContain(lines, l => l.StartsWith("OutputDir", StringComparison.Ordinal));
        Assert.DoesNotContain(lines, l => l.StartsWith("SetupIconFile", StringComparison.Ordinal));
    }

    [Fact]
    public void Output_folder_and_file_name_are_written()
    {
        var project = SampleProject();
        project.OutputPath = @"C:\Uitvoer";
        project.OutputBaseFilename = "MijnAppSetup";

        var lines = Lines(Generate(project));

        Assert.Contains(@"OutputDir=C:\Uitvoer", lines);
        Assert.Contains("OutputBaseFilename=MijnAppSetup", lines);
    }

    [Fact]
    public void Values_are_trimmed()
    {
        var project = SampleProject();
        project.AppName = "  MijnApp  ";

        Assert.Contains("AppName=MijnApp", Lines(Generate(project)));
    }

    // ---- architectuur en uiterlijk ----------------------------------------------------------

    [Theory]
    [InlineData(InstallerArchitecture.X64, true)]
    [InlineData(InstallerArchitecture.X86, false)]
    public void Architecture_x64_is_written_as_x64compatible(InstallerArchitecture architecture, bool expected)
    {
        var project = SampleProject();
        project.Architecture = architecture;

        var lines = Lines(Generate(project));

        Assert.Equal(expected, lines.Contains("ArchitecturesInstallIn64BitMode=x64compatible"));
        Assert.DoesNotContain("ArchitecturesInstallIn64BitMode=x64", lines);
    }

    [Theory]
    [InlineData(InstallerWizardStyle.Modern, true)]
    [InlineData(InstallerWizardStyle.Classic, false)]
    public void Wizard_style_modern_is_written_and_classic_is_left_to_the_default(InstallerWizardStyle style, bool expected)
    {
        var project = SampleProject();
        project.WizardStyle = style;

        Assert.Equal(expected, Lines(Generate(project)).Contains("WizardStyle=modern"));
    }

    [Fact]
    public void Images_and_icon_are_written_and_missing_files_are_reported()
    {
        var project = SampleProject();
        project.SetupIconFile = @"C:\Beeld\app.ico";
        project.WizardImageFile = @"C:\Beeld\groot.bmp";
        project.WizardSmallImageFile = @"C:\Beeld\klein.bmp";

        var result = Generate(project, new FakeGeneratorEnvironment().WithDirectory(@"C:\Bron\MijnApp").WithFile(@"C:\Bron\MijnApp\MijnApp.exe").WithFile(@"C:\Beeld\app.ico"));
        var lines = Lines(result);

        Assert.Contains(@"SetupIconFile=C:\Beeld\app.ico", lines);
        Assert.Contains(@"WizardImageFile=C:\Beeld\groot.bmp", lines);
        Assert.Contains(@"WizardSmallImageFile=C:\Beeld\klein.bmp", lines);
        Assert.Equal(2, result.Issues.Count(i => i.Code == GenerationIssueCode.FileNotFound));
        Assert.Contains(result.Issues, i => i.Code == GenerationIssueCode.FileNotFound && i.Arguments.SequenceEqual(new[] { "WizardImageFile", @"C:\Beeld\groot.bmp" }));
    }

    // ---- wizardpagina's -------------------------------------------------------------------------

    [Theory]
    [InlineData(true, DisablePageMode.AlwaysShow, "DisableDirPage=no")]
    [InlineData(true, DisablePageMode.NeverShow, "DisableDirPage=yes")]
    [InlineData(true, DisablePageMode.AutoSkipIfKnown, null)]
    [InlineData(false, DisablePageMode.AlwaysShow, "DisableDirPage=yes")]
    [InlineData(false, DisablePageMode.AutoSkipIfKnown, "DisableDirPage=yes")]
    public void Destination_page_maps_to_DisableDirPage(bool pageShown, DisablePageMode mode, string? expected)
    {
        var project = SampleProject();
        project.WizardScreens.ShowSelectDestinationPage = pageShown;
        project.DirPageMode = mode;

        AssertOptionalDirective(Lines(Generate(project)), "DisableDirPage", expected);
    }

    [Theory]
    [InlineData(true, DisablePageMode.AlwaysShow, "DisableProgramGroupPage=no")]
    [InlineData(true, DisablePageMode.NeverShow, "DisableProgramGroupPage=yes")]
    [InlineData(true, DisablePageMode.AutoSkipIfKnown, null)]
    [InlineData(false, DisablePageMode.AlwaysShow, "DisableProgramGroupPage=yes")]
    [InlineData(false, DisablePageMode.AutoSkipIfKnown, "DisableProgramGroupPage=yes")]
    public void Start_menu_page_maps_to_DisableProgramGroupPage(bool pageShown, DisablePageMode mode, string? expected)
    {
        var project = SampleProject();
        project.WizardScreens.ShowSelectProgramGroupPage = pageShown;
        project.GroupPageMode = mode;

        AssertOptionalDirective(Lines(Generate(project)), "DisableProgramGroupPage", expected);
    }

    [Theory]
    [InlineData(true, null)]
    [InlineData(false, "DisableReadyPage=yes")]
    public void Ready_page_maps_to_DisableReadyPage(bool pageShown, string? expected)
    {
        var project = SampleProject();
        project.WizardScreens.ShowReadyPage = pageShown;

        AssertOptionalDirective(Lines(Generate(project)), "DisableReadyPage", expected);
    }

    [Theory]
    [InlineData(true, null)]
    [InlineData(false, "DisableFinishedPage=yes")]
    public void Finished_page_maps_to_DisableFinishedPage(bool pageShown, string? expected)
    {
        var project = SampleProject();
        project.WizardScreens.ShowFinishedPage = pageShown;

        AssertOptionalDirective(Lines(Generate(project)), "DisableFinishedPage", expected);
    }

    [Theory]
    [InlineData(true, "DisableWelcomePage=no")]
    [InlineData(false, null)]
    public void Welcome_page_is_hidden_by_default_in_inno_setup(bool pageShown, string? expected)
    {
        var project = SampleProject();
        project.WizardScreens.ShowWelcomePage = pageShown;

        AssertOptionalDirective(Lines(Generate(project)), "DisableWelcomePage", expected);
    }

    [Fact]
    public void Ready_page_options_only_appear_when_they_differ_from_the_default_and_the_page_is_shown()
    {
        var project = SampleProject();
        project.DisableReadyMemo = true;
        project.AlwaysShowDirOnReadyPage = true;
        project.AlwaysShowGroupOnReadyPage = true;

        var shown = Lines(Generate(project));
        Assert.Contains("DisableReadyMemo=yes", shown);
        Assert.Contains("AlwaysShowDirOnReadyPage=yes", shown);
        Assert.Contains("AlwaysShowGroupOnReadyPage=yes", shown);

        project.WizardScreens.ShowReadyPage = false;
        var hidden = Lines(Generate(project));
        Assert.DoesNotContain("DisableReadyMemo=yes", hidden);
        Assert.DoesNotContain("AlwaysShowDirOnReadyPage=yes", hidden);
    }

    [Fact]
    public void Defaults_of_inno_setup_are_not_written()
    {
        var lines = Lines(Generate(SampleProject()));

        foreach (var directive in new[]
        {
            "AppendDefaultGroupName", "AlwaysUsePersonalGroup", "UsePreviousAppDir", "UsePreviousGroup",
            "UsePreviousSetupType", "UsePreviousTasks", "UsePreviousLanguage", "UsePreviousUserInfo",
            "DisableReadyMemo", "UserInfoPage", "LicenseFile", "InfoBeforeFile", "InfoAfterFile",
        })
        {
            Assert.DoesNotContain(lines, l => l.StartsWith(directive + "=", StringComparison.Ordinal));
        }
    }

    [Fact]
    public void Options_that_differ_from_the_inno_setup_default_are_written()
    {
        var project = SampleProject();
        project.AppendDefaultGroupName = false;
        project.AlwaysUsePersonalGroup = true;
        project.UsePreviousAppDir = false;
        project.UsePreviousGroup = false;
        project.UsePreviousSetupType = false;
        project.UsePreviousTasks = false;
        project.UsePreviousLanguage = false;

        var lines = Lines(Generate(project));

        Assert.Contains("AppendDefaultGroupName=no", lines);
        Assert.Contains("AlwaysUsePersonalGroup=yes", lines);
        Assert.Contains("UsePreviousAppDir=no", lines);
        Assert.Contains("UsePreviousGroup=no", lines);
        Assert.Contains("UsePreviousSetupType=no", lines);
        Assert.Contains("UsePreviousTasks=no", lines);
        Assert.Contains("UsePreviousLanguage=no", lines);
    }

    [Fact]
    public void Pages_with_a_file_write_the_file_directive()
    {
        var project = SampleProject();
        project.WizardScreens.ShowLicensePage = true;
        project.WizardScreens.ShowInfoBeforePage = true;
        project.WizardScreens.ShowInfoAfterPage = true;
        project.LicenseFilePath = @"C:\Docs\licentie.txt";
        project.InfoBeforeFilePath = @"C:\Docs\voor.rtf";
        project.InfoAfterFilePath = @"C:\Docs\na.txt";

        var result = Generate(project);
        var lines = Lines(result);

        Assert.Contains(@"LicenseFile=C:\Docs\licentie.txt", lines);
        Assert.Contains(@"InfoBeforeFile=C:\Docs\voor.rtf", lines);
        Assert.Contains(@"InfoAfterFile=C:\Docs\na.txt", lines);
        Assert.Empty(result.Issues);
    }

    [Fact]
    public void Page_without_a_file_is_reported_and_writes_no_directive()
    {
        var project = SampleProject();
        project.WizardScreens.ShowLicensePage = true;

        var result = Generate(project);

        Assert.DoesNotContain(Lines(result), l => l.StartsWith("LicenseFile=", StringComparison.Ordinal));
        var issue = Assert.Single(result.Issues);
        Assert.Equal(GenerationSeverity.Warning, issue.Severity);
        Assert.Equal(GenerationIssueCode.PageWithoutFile, issue.Code);
        Assert.Equal(new[] { "LicenseFilePath" }, issue.Arguments);
    }

    [Fact]
    public void File_of_a_hidden_page_is_not_written_and_not_checked()
    {
        var project = SampleProject();
        project.LicenseFilePath = @"C:\Docs\licentie.txt";

        var result = Generate(project, FakeGeneratorEnvironment.Nothing);

        Assert.DoesNotContain(Lines(result), l => l.StartsWith("LicenseFile=", StringComparison.Ordinal));
        Assert.DoesNotContain(result.Issues, i => i.Code == GenerationIssueCode.FileNotFound);
    }

    [Fact]
    public void Missing_page_file_gives_a_warning_but_is_still_written()
    {
        var project = SampleProject();
        project.WizardScreens.ShowLicensePage = true;
        project.LicenseFilePath = @"C:\Docs\weg.txt";

        var result = Generate(project, new FakeGeneratorEnvironment().WithDirectory(@"C:\Bron\MijnApp").WithFile(@"C:\Bron\MijnApp\MijnApp.exe"));

        Assert.Contains(@"LicenseFile=C:\Docs\weg.txt", Lines(result));
        Assert.Contains(result.Issues, i => i.Code == GenerationIssueCode.FileNotFound && i.Severity == GenerationSeverity.Warning);
        Assert.False(result.HasErrors);
    }

    [Fact]
    public void User_info_page_writes_its_defaults()
    {
        var project = SampleProject();
        project.WizardScreens.ShowUserInfoPage = true;
        project.DefaultUserInfoName = "Herbert";
        project.DefaultUserInfoOrg = "Voortman";
        project.DefaultUserInfoSerial = "1234-5678";
        project.UsePreviousUserInfo = false;

        var lines = Lines(Generate(project));

        Assert.Contains("UserInfoPage=yes", lines);
        Assert.Contains("DefaultUserInfoName=Herbert", lines);
        Assert.Contains("DefaultUserInfoOrg=Voortman", lines);
        Assert.Contains("DefaultUserInfoSerial=1234-5678", lines);
        Assert.Contains("UsePreviousUserInfo=no", lines);
    }

    [Fact]
    public void User_info_details_are_left_out_when_the_page_is_hidden()
    {
        var project = SampleProject();
        project.DefaultUserInfoName = "Herbert";
        project.UsePreviousUserInfo = false;

        var lines = Lines(Generate(project));

        Assert.DoesNotContain(lines, l => l.StartsWith("DefaultUserInfo", StringComparison.Ordinal));
        Assert.DoesNotContain("UsePreviousUserInfo=no", lines);
    }

    private static void AssertOptionalDirective(string[] lines, string directive, string? expected)
    {
        var found = lines.Where(l => l.StartsWith(directive + "=", StringComparison.Ordinal)).ToArray();
        if (expected is null)
        {
            Assert.Empty(found);
        }
        else
        {
            Assert.Equal(new[] { expected }, found);
        }
    }
}
