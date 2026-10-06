using InnoSetupStudio.Core.Generation;
using InnoSetupStudio.Core.Project;

namespace InnoSetupStudio.Tests.Generation;

/// <summary>
/// Laat de echte Inno Setup-compiler (ISCC) de gegenereerde scripts compileren. Dit is de test die
/// ertoe doet: de generator is pas goed als ISCC het resultaat accepteert. Deze tests worden
/// overgeslagen op een machine zonder Inno Setup, en bouwen alleen een installer, ze voeren hem niet
/// uit (standaard vraagt Setup om beheerdersrechten).
/// </summary>
public sealed class IssCompilerTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "InnoSetupStudioTests", Guid.NewGuid().ToString("N"));

    public IssCompilerTests()
    {
        Directory.CreateDirectory(Source);
        Directory.CreateDirectory(Path.Combine(Source, "data"));
        File.WriteAllBytes(Path.Combine(Source, "MijnApp.exe"), [0x4D, 0x5A, 0x00, 0x00]);
        File.WriteAllText(Path.Combine(Source, "data", "leesmij.txt"), "voorbeeld");
        File.WriteAllText(LicenseFile, "Licentietekst");
        File.WriteAllText(InfoBeforeFile, "Voor de installatie");
        File.WriteAllText(InfoAfterFile, "Na de installatie");
        File.WriteAllBytes(IconFile, TinyIcon());
        File.WriteAllBytes(BitmapFile, TinyBitmap());
    }

    private string Source => Path.Combine(_root, "bron");

    private string Output => Path.Combine(_root, "uit");

    private string LicenseFile => Path.Combine(_root, "licentie.txt");

    private string InfoBeforeFile => Path.Combine(_root, "voor.txt");

    private string InfoAfterFile => Path.Combine(_root, "na.txt");

    private string IconFile => Path.Combine(_root, "app.ico");

    private string BitmapFile => Path.Combine(_root, "beeld.bmp");

    public void Dispose()
    {
        try
        {
            Directory.Delete(_root, recursive: true);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Opruimen van tijdelijke bestanden mag een test nooit laten falen.
        }
    }

    [IsccFact]
    public async Task Minimal_project_compiles()
    {
        var project = NewProject();

        await AssertCompilesAsync(project, "MijnApp-1.0-Setup.exe");
    }

    [IsccTheory]
    [InlineData(InstallerArchitecture.X64, InstallerWizardStyle.Modern)]
    [InlineData(InstallerArchitecture.X64, InstallerWizardStyle.Classic)]
    [InlineData(InstallerArchitecture.X86, InstallerWizardStyle.Modern)]
    [InlineData(InstallerArchitecture.X86, InstallerWizardStyle.Classic)]
    public async Task Architecture_and_wizard_style_combinations_compile(InstallerArchitecture architecture, InstallerWizardStyle style)
    {
        var project = NewProject();
        project.Architecture = architecture;
        project.WizardStyle = style;

        await AssertCompilesAsync(project, "MijnApp-1.0-Setup.exe");
    }

    [IsccFact]
    public async Task Project_with_every_page_and_all_options_compiles()
    {
        var project = NewProject();
        project.Publisher = "Voortman Steel Machinery";
        project.PublisherUrl = "https://www.voortman.net";
        project.PublisherEmail = "info@voortman.net";
        project.SetupIconFile = IconFile;
        project.WizardImageFile = BitmapFile;
        project.WizardSmallImageFile = BitmapFile;
        project.DefaultDirName = @"{autopf}\Voortman\MijnApp";
        project.DefaultGroupName = @"Voortman\MijnApp";
        project.AppendDefaultGroupName = false;
        project.AlwaysUsePersonalGroup = false;
        project.UsePreviousTasks = false;
        project.CreateStartMenuIcon = true;
        project.CreateDesktopIcon = true;
        project.LicenseFilePath = LicenseFile;
        project.InfoBeforeFilePath = InfoBeforeFile;
        project.InfoAfterFilePath = InfoAfterFile;
        project.DefaultUserInfoName = "Herbert";
        project.DefaultUserInfoOrg = "Voortman";
        project.DisableReadyMemo = true;
        project.AlwaysShowDirOnReadyPage = true;
        project.DirPageMode = DisablePageMode.AutoSkipIfKnown;
        project.GroupPageMode = DisablePageMode.AlwaysShow;
        project.SupportedLanguageIds = ["english", "dutch", "german"];
        var screens = project.WizardScreens;
        screens.ShowWelcomePage = true;
        screens.ShowLicensePage = true;
        screens.ShowInfoBeforePage = true;
        screens.ShowUserInfoPage = true;
        screens.ShowSelectTasksPage = true;
        screens.ShowInfoAfterPage = true;

        await AssertCompilesAsync(project, "MijnApp-1.0-Setup.exe");
    }

    [IsccFact]
    public async Task Project_with_every_page_hidden_compiles()
    {
        var project = NewProject();
        var screens = project.WizardScreens;
        screens.ShowWelcomePage = false;
        screens.ShowSelectDestinationPage = false;
        screens.ShowSelectProgramGroupPage = false;
        screens.ShowReadyPage = false;
        screens.ShowFinishedPage = false;

        await AssertCompilesAsync(project, "MijnApp-1.0-Setup.exe");
    }

    [IsccFact]
    public async Task Every_language_of_the_catalog_compiles_together()
    {
        var project = NewProject();
        project.SupportedLanguageIds = InnoLanguageCatalog.Languages.Select(l => l.Id).ToList();

        await AssertCompilesAsync(project, "MijnApp-1.0-Setup.exe");
    }

    [IsccFact]
    public async Task Special_characters_in_texts_compile()
    {
        var project = NewProject();
        project.AppName = "Café {Plus} \"Pro\" & Co";
        project.Publisher = "Müller {BV}";
        project.CreateStartMenuIcon = true;
        project.CreateDesktopIcon = true;
        project.WizardScreens.ShowSelectTasksPage = true;

        await AssertCompilesAsync(project, "Café {Plus} _Pro_ & Co-1.0-Setup.exe");
    }

    [IsccFact]
    public async Task Output_file_name_with_accents_compiles_with_the_utf8_byte_order_mark()
    {
        var project = NewProject();
        project.OutputBaseFilename = "Café-Setup";

        await AssertCompilesAsync(project, "Café-Setup.exe");
    }

    // Paden en bestandsnamen met een accolade: welke richtlijnen lezen "{" als constante en welke niet.
    [IsccTheory]
    [InlineData("Source")]
    [InlineData("OutputDir")]
    [InlineData("OutputBaseFilename")]
    [InlineData("LicenseFile")]
    [InlineData("InfoBeforeFile")]
    [InlineData("SetupIconFile")]
    [InlineData("WizardImageFile")]
    public async Task Braces_in_a_path_compile(string field)
    {
        var project = NewProject();
        var braceFolder = Path.Combine(_root, "map {x}");
        Directory.CreateDirectory(braceFolder);
        var expected = "MijnApp-1.0-Setup.exe";
        switch (field)
        {
            case "Source":
                var source = Path.Combine(_root, "bron {x}");
                Directory.CreateDirectory(source);
                File.WriteAllText(Path.Combine(source, "MijnApp.exe"), "x");
                project.SourceFilesPath = source;
                break;
            case "OutputDir":
                project.OutputPath = Path.Combine(_root, "uit {x}");
                break;
            case "OutputBaseFilename":
                project.OutputBaseFilename = "Naam {y}";
                expected = "Naam {y}.exe";
                break;
            case "LicenseFile":
                File.Copy(LicenseFile, Path.Combine(braceFolder, "l.txt"));
                project.LicenseFilePath = Path.Combine(braceFolder, "l.txt");
                project.WizardScreens.ShowLicensePage = true;
                break;
            case "InfoBeforeFile":
                File.Copy(InfoBeforeFile, Path.Combine(braceFolder, "i.txt"));
                project.InfoBeforeFilePath = Path.Combine(braceFolder, "i.txt");
                project.WizardScreens.ShowInfoBeforePage = true;
                break;
            case "SetupIconFile":
                File.Copy(IconFile, Path.Combine(braceFolder, "a.ico"));
                project.SetupIconFile = Path.Combine(braceFolder, "a.ico");
                break;
            case "WizardImageFile":
                File.Copy(BitmapFile, Path.Combine(braceFolder, "b.bmp"));
                project.WizardImageFile = Path.Combine(braceFolder, "b.bmp");
                break;
        }

        await AssertCompilesAsync(project, expected);
    }
    // ---- knopinstellingen (stap 4) ------------------------------------------------------------------

    [IsccTheory]
    [InlineData(InstallerWizardStyle.Modern)]
    [InlineData(InstallerWizardStyle.Classic)]
    public async Task Button_settings_for_all_screens_and_three_languages_compile_without_warnings(InstallerWizardStyle style)
    {
        var project = NewProject();
        project.WizardStyle = style;
        project.LicenseFilePath = LicenseFile;
        project.InfoBeforeFilePath = InfoBeforeFile;
        project.InfoAfterFilePath = InfoAfterFile;
        GeneratorTestSupport.ApplyButtonSettings(project);

        await AssertCompilesAsync(project, "MijnApp-1.0-Setup.exe");
    }

    [IsccFact]
    public async Task Button_settings_in_a_single_language_project_compile()
    {
        var project = NewProject();
        project.WelcomeScreenButtons.NextButtonCaption = "Start";
        project.WelcomeScreenButtons.NextButtonTooltip = "Ga verder";
        project.WelcomeScreenButtons.NextButtonFontFamily = "Consolas";
        project.WelcomeScreenButtons.NextButtonFontSize = 11;
        project.WelcomeScreenButtons.NextButtonFontBold = true;

        await AssertCompilesAsync(project, "MijnApp-1.0-Setup.exe");
    }

    [IsccFact]
    public async Task Only_the_default_screen_with_settings_compiles()
    {
        var project = NewProject();
        project.DefaultScreenButtons.BackButtonCaption = "Terug";
        project.DefaultScreenButtons.NextButtonCaption = "Verder";
        project.DefaultScreenButtons.NextButtonFontSize = 10;
        project.DefaultScreenButtons.NextButtonFontBold = true;
        project.DefaultScreenButtons.CancelButtonVisible = false;

        await AssertCompilesAsync(project, "MijnApp-1.0-Setup.exe");
    }

    [IsccTheory]
    [InlineData("It's")]
    [InlineData("\"Quoted\"")]
    [InlineData("100%")]
    [InlineData("%n %1 %%")]
    [InlineData("{app}")]
    [InlineData("{")]
    [InlineData("{{")]
    [InlineData("a{b}c")]
    [InlineData("{cm:CreateDesktopIcon}")]
    [InlineData("a;b")]
    [InlineData("a=b")]
    [InlineData("Größe é à ç")]
    [InlineData("Install > now")]
    public async Task Special_characters_in_button_texts_compile_without_warnings(string text)
    {
        var project = NewProject();
        project.SupportedLanguageIds = ["english", "dutch", "german"];
        var welcome = project.WelcomeScreenButtons;
        welcome.NextButtonCaption = text;
        welcome.NextButtonTooltip = text;
        welcome.NextButtonCaptionByLanguage["dutch"] = text + " NL";
        welcome.NextButtonTooltipByLanguage["german"] = text + " DE";
        project.SelectDestinationBrowseButton.Caption = text;

        await AssertCompilesAsync(project, "MijnApp-1.0-Setup.exe");
    }

    [IsccFact]
    public async Task Font_name_with_an_apostrophe_compiles()
    {
        var project = NewProject();
        project.WelcomeScreenButtons.NextButtonFontFamily = "Segoe 'UI'";

        await AssertCompilesAsync(project, "MijnApp-1.0-Setup.exe");
    }

    [IsccFact]
    public async Task Text_color_is_reported_but_the_script_still_compiles()
    {
        var project = NewProject();
        project.DefaultScreenButtons.NextButtonTextColor = "#FF0000";
        project.DefaultScreenButtons.NextButtonCaption = "Verder";

        var result = new IssGenerator().Generate(project);
        Assert.Contains(result.Issues, issue => issue.Code == GenerationIssueCode.ButtonTextColorNotSupported);

        await AssertCompilesAsync(project, "MijnApp-1.0-Setup.exe");
    }

    [IsccFact]
    public async Task Button_settings_on_screens_that_are_turned_off_compile()
    {
        var project = NewProject();
        project.WizardScreens.ShowWelcomePage = false;
        project.WizardScreens.ShowSelectDestinationPage = false;
        project.WelcomeScreenButtons.NextButtonCaption = "Start";
        project.SelectDestinationBrowseButton.Caption = "Zoeken";

        await AssertCompilesAsync(project, "MijnApp-1.0-Setup.exe");
    }

    [IsccFact]
    public async Task Unescaped_app_id_does_not_compile()
    {
        var project = NewProject();
        var script = new IssGenerator().Generate(project).Script
            .Replace("AppId={{", "AppId={", StringComparison.Ordinal);

        var run = await CompileScriptAsync(script);

        Assert.NotEqual(0, run.ExitCode);
    }

    [IsccFact]
    public void Every_language_file_of_the_catalog_exists_in_the_installation()
    {
        var folder = InnoSetupInstallation.Folder!;
        foreach (var language in InnoLanguageCatalog.Languages)
        {
            var path = language.MessagesFile.Replace("compiler:", folder + Path.DirectorySeparatorChar, StringComparison.Ordinal);
            Assert.True(File.Exists(path), $"{language.Id}: {path} ontbreekt");
        }
    }

    private InstallerProject NewProject() => new()
    {
        AppId = "{8C5C7F52-0F3E-4F5B-9A59-1D2E3F4A5B6C}",
        AppName = "MijnApp",
        AppVersion = "1.0",
        SourceFilesPath = Source,
        MainExecutable = "MijnApp.exe",
        OutputPath = Output,
    };

    private async Task AssertCompilesAsync(InstallerProject project, string expectedInstaller)
    {
        var result = new IssGenerator().Generate(project);
        Assert.False(result.HasErrors, "Generator meldt fouten: " + string.Join(", ", result.Issues.Select(i => i.Code)));

        var run = await CompileScriptAsync(result.Script);

        Assert.True(run.ExitCode == 0, "ISCC gaf exitcode " + run.ExitCode + Environment.NewLine + run.Output + Environment.NewLine + result.Script);
        Assert.DoesNotMatch(@"(?im)^\s*warning:", run.Output);
        Assert.True(File.Exists(Path.Combine(project.OutputPath, expectedInstaller)), "Installer ontbreekt: " + expectedInstaller + Environment.NewLine + "Aanwezig: " + string.Join(" | ", Directory.Exists(project.OutputPath) ? Directory.GetFiles(project.OutputPath).Select(Path.GetFileName) : []) + Environment.NewLine + run.Output);
    }

    private async Task<CompilerRun> CompileScriptAsync(string script, System.Text.Encoding? encoding = null)
    {
        Directory.CreateDirectory(_root);
        var scriptPath = Path.Combine(_root, "setup.iss");
        await File.WriteAllTextAsync(scriptPath, script, encoding ?? IssGenerator.ScriptEncoding);
        return await InnoSetupInstallation.CompileAsync(scriptPath);
    }

    // Een bitmap van 2x2 pixels, 24 bits per pixel; elke rij wordt aangevuld tot een veelvoud van 4 bytes.
    private static byte[] TinyBitmap()
    {
        byte[] pixels = [0xFF, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x00, 0x00, 0x00, 0xFF, 0x00];
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);
        writer.Write((byte)'B');
        writer.Write((byte)'M');
        writer.Write(54 + pixels.Length);
        writer.Write(0);
        writer.Write(54);
        writer.Write(40);
        writer.Write(2);
        writer.Write(2);
        writer.Write((short)1);
        writer.Write((short)24);
        writer.Write(0);
        writer.Write(pixels.Length);
        writer.Write(2835);
        writer.Write(2835);
        writer.Write(0);
        writer.Write(0);
        writer.Write(pixels);
        return stream.ToArray();
    }

    // Een pictogram van 1x1 pixel, 32 bits met alfakanaal.
    private static byte[] TinyIcon()
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);
        writer.Write((short)0);
        writer.Write((short)1);
        writer.Write((short)1);
        writer.Write((byte)1);
        writer.Write((byte)1);
        writer.Write((byte)0);
        writer.Write((byte)0);
        writer.Write((short)1);
        writer.Write((short)32);
        writer.Write(40 + 4 + 4);
        writer.Write(22);
        writer.Write(40);
        writer.Write(1);
        writer.Write(2);
        writer.Write((short)1);
        writer.Write((short)32);
        writer.Write(0);
        writer.Write(8);
        writer.Write(0);
        writer.Write(0);
        writer.Write(0);
        writer.Write(0);
        writer.Write(0xFF336699u);
        writer.Write(0u);
        return stream.ToArray();
    }
}
