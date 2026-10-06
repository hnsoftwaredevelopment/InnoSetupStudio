using InnoSetupStudio.Core.Generation;
using InnoSetupStudio.Core.Project;
using static InnoSetupStudio.Tests.Generation.GeneratorTestSupport;

namespace InnoSetupStudio.Tests.Generation;

/// <summary>Tests voor [CustomMessages] en [Code] met de knopinstellingen (stap 4, zie
/// docs/Ontwerp-Knopinstellingen-Generator.md).</summary>
public class IssGeneratorButtonTests
{
    // De regels van een sectie, zonder de sectiekop en zonder de lege regel erna. Leeg als de
    // sectie ontbreekt.
    private static string[] Section(GenerationResult result, string name)
    {
        var lines = result.Script.Split("\r\n");
        var start = Array.IndexOf(lines, "[" + name + "]");
        if (start < 0)
        {
            return [];
        }

        return lines
            .Skip(start + 1)
            .TakeWhile(line => !line.StartsWith('['))
            .Reverse().SkipWhile(line => line.Length == 0).Reverse()
            .ToArray();
    }

    private static string CodeText(GenerationResult result) => string.Join("\n", Section(result, "Code"));

    private static GenerationIssue[] IssuesOf(GenerationResult result, GenerationIssueCode code)
        => result.Issues.Where(issue => issue.Code == code).ToArray();

    // ---- niets aangepast ---------------------------------------------------------------------

    [Fact]
    public void Project_without_button_settings_has_no_messages_and_no_code()
    {
        var result = Generate(SampleProject());

        Assert.DoesNotContain("[CustomMessages]", result.Script);
        Assert.DoesNotContain("[Code]", result.Script);
        Assert.Empty(result.Issues);
    }

    [Fact]
    public void Settings_that_resolve_to_nothing_give_no_code()
    {
        var project = SampleProject();
        project.WelcomeScreenButtons.NextButtonCaption = "   ";
        project.WelcomeScreenButtons.NextButtonEnabled = true;
        project.WelcomeScreenButtons.NextButtonVisible = true;
        project.WelcomeScreenButtons.NextButtonFontBold = false;
        project.WelcomeScreenButtons.NextButtonFontSize = 0;

        var result = Generate(project);

        Assert.DoesNotContain("[Code]", result.Script);
        Assert.DoesNotContain("[CustomMessages]", result.Script);
    }

    // ---- teksten ---------------------------------------------------------------------------

    [Fact]
    public void Caption_on_one_screen_gives_a_message_and_a_case_for_that_page()
    {
        var project = SampleProject();
        project.WelcomeScreenButtons.NextButtonCaption = "Start";

        var result = Generate(project);

        Assert.Equal(new[] { "BtnWelcomeNextCaption=Start" }, Section(result, "CustomMessages"));
        Assert.Equal(
            new[]
            {
                "procedure CurPageChanged(CurPageID: Integer);",
                "begin",
                "  case CurPageID of",
                "    wpWelcome:",
                "      begin",
                "        WizardForm.NextButton.Caption := CustomMessage('BtnWelcomeNextCaption');",
                "      end;",
                "  end;",
                "end;",
            },
            Section(result, "Code"));
        Assert.Empty(result.Issues);
    }

    [Fact]
    public void Sections_come_in_the_documented_order()
    {
        var project = SampleProject();
        project.CreateDesktopIcon = true;
        project.CreateStartMenuIcon = true;
        project.WelcomeScreenButtons.NextButtonCaption = "Start";

        var script = Generate(project).Script;

        var order = new[] { "[Setup]", "[Languages]", "[CustomMessages]", "[Tasks]", "[Files]", "[Icons]", "[Code]" }
            .Select(name => script.IndexOf(name, StringComparison.Ordinal))
            .ToArray();
        Assert.All(order, index => Assert.True(index >= 0));
        Assert.Equal(order.Order().ToArray(), order);
    }

    [Fact]
    public void Message_without_language_prefix_comes_before_the_translations_in_catalog_order()
    {
        var project = SampleProject();
        project.SupportedLanguageIds = ["german", "dutch"];
        project.WelcomeScreenButtons.NextButtonCaption = "Start";
        project.WelcomeScreenButtons.NextButtonCaptionByLanguage["german"] = "Los";
        project.WelcomeScreenButtons.NextButtonCaptionByLanguage["dutch"] = "Begin";

        Assert.Equal(
            new[] { "BtnWelcomeNextCaption=Start", "dutch.BtnWelcomeNextCaption=Begin", "german.BtnWelcomeNextCaption=Los" },
            Section(Generate(project), "CustomMessages"));
    }

    [Fact]
    public void Translations_for_languages_outside_the_project_are_ignored()
    {
        var project = SampleProject();
        project.SupportedLanguageIds = ["dutch"];
        project.WelcomeScreenButtons.NextButtonCaption = "Start";
        project.WelcomeScreenButtons.NextButtonCaptionByLanguage["german"] = "Los";
        project.WelcomeScreenButtons.NextButtonCaptionByLanguage["english"] = "Go";

        Assert.Equal(new[] { "BtnWelcomeNextCaption=Start" }, Section(Generate(project), "CustomMessages"));
    }

    [Fact]
    public void Translation_without_universal_text_gives_an_empty_message_and_a_guarded_assignment()
    {
        var project = SampleProject();
        project.SupportedLanguageIds = ["dutch"];
        project.WelcomeScreenButtons.NextButtonCaptionByLanguage["dutch"] = "Begin";

        var result = Generate(project);

        Assert.Equal(new[] { "BtnWelcomeNextCaption=", "dutch.BtnWelcomeNextCaption=Begin" }, Section(result, "CustomMessages"));
        Assert.Contains(
            "        if CustomMessage('BtnWelcomeNextCaption') <> '' then\n"
            + "          WizardForm.NextButton.Caption := CustomMessage('BtnWelcomeNextCaption');",
            CodeText(result));
    }

    [Fact]
    public void Translation_only_for_a_language_outside_the_project_gives_nothing()
    {
        var project = SampleProject();
        project.SupportedLanguageIds = ["dutch"];
        project.WelcomeScreenButtons.NextButtonCaptionByLanguage["german"] = "Los";

        var result = Generate(project);

        Assert.DoesNotContain("[CustomMessages]", result.Script);
        Assert.DoesNotContain("[Code]", result.Script);
    }

    [Fact]
    public void Texts_are_written_literally_without_escaping()
    {
        const string text = "It's \"Go\"; a=b %n %1 {app} {{ é";
        var project = SampleProject();
        project.WelcomeScreenButtons.NextButtonCaption = text;

        Assert.Equal(new[] { "BtnWelcomeNextCaption=" + text }, Section(Generate(project), "CustomMessages"));
    }

    [Fact]
    public void Texts_are_trimmed()
    {
        var project = SampleProject();
        project.WelcomeScreenButtons.NextButtonCaption = "  Start  ";

        Assert.Equal(new[] { "BtnWelcomeNextCaption=Start" }, Section(Generate(project), "CustomMessages"));
    }

    [Theory]
    [InlineData("Start\r\nnu", "WelcomeScreenButtons.NextButtonCaption")]
    [InlineData("Start\nnu", "WelcomeScreenButtons.NextButtonCaption")]
    public void Caption_with_a_line_break_is_an_error_and_is_left_out(string caption, string field)
    {
        var project = SampleProject();
        project.WelcomeScreenButtons.NextButtonCaption = caption;

        var result = Generate(project);

        var issue = Assert.Single(IssuesOf(result, GenerationIssueCode.ValueContainsLineBreak));
        Assert.Equal(GenerationSeverity.Error, issue.Severity);
        Assert.Equal(new[] { field }, issue.Arguments);
        Assert.DoesNotContain("[CustomMessages]", result.Script);
        Assert.DoesNotContain("[Code]", result.Script);
    }

    [Fact]
    public void Translation_tooltip_and_font_with_a_line_break_are_errors_and_are_left_out()
    {
        var project = SampleProject();
        project.SupportedLanguageIds = ["dutch"];
        project.ReadyScreenButtons.NextButtonCaptionByLanguage["dutch"] = "Nu\ninstalleren";
        project.ReadyScreenButtons.NextButtonTooltip = "Tip\r\nregel";
        project.ReadyScreenButtons.NextButtonFontFamily = "Arial\nBlack";

        var result = Generate(project);

        Assert.Equal(
            new[]
            {
                "ReadyScreenButtons.NextButtonCaptionByLanguage[dutch]",
                "ReadyScreenButtons.NextButtonFontFamily",
                "ReadyScreenButtons.NextButtonTooltip",
            },
            IssuesOf(result, GenerationIssueCode.ValueContainsLineBreak).Select(i => i.Arguments.Single()).Order().ToArray());
        Assert.DoesNotContain("[Code]", result.Script);
    }

    [Fact]
    public void Line_break_in_the_default_screen_is_reported_once()
    {
        var project = SampleProject();
        project.DefaultScreenButtons.NextButtonCaption = "Verder\nnu";

        var result = Generate(project);

        var issue = Assert.Single(IssuesOf(result, GenerationIssueCode.ValueContainsLineBreak));
        Assert.Equal(new[] { "DefaultScreenButtons.NextButtonCaption" }, issue.Arguments);
    }

    // ---- standaardscherm -----------------------------------------------------------------------

    [Fact]
    public void Default_screen_applies_to_every_shown_screen_that_has_a_model()
    {
        var project = SampleProject();
        project.DefaultScreenButtons.NextButtonCaption = "Verder";

        var result = Generate(project);

        Assert.Equal(
            new[]
            {
                "BtnWelcomeNextCaption=Verder",
                "BtnSelectDirNextCaption=Verder",
                "BtnSelectGroupNextCaption=Verder",
                "BtnReadyNextCaption=Verder",
            },
            Section(result, "CustomMessages"));
        var code = CodeText(result);
        Assert.Contains("wpWelcome:", code);
        Assert.Contains("wpSelectDir:", code);
        Assert.Contains("wpSelectProgramGroup:", code);
        Assert.Contains("wpReady:", code);
        Assert.DoesNotContain("wpLicense", code);
        Assert.DoesNotContain("wpFinished", code);
    }

    [Fact]
    public void Own_value_overrides_the_default_screen()
    {
        var project = SampleProject();
        project.DefaultScreenButtons.NextButtonCaption = "Verder";
        project.ReadyScreenButtons.NextButtonCaption = "Installeer";

        var result = Generate(project);

        Assert.Contains("BtnReadyNextCaption=Installeer", Section(result, "CustomMessages"));
        Assert.DoesNotContain("BtnReadyNextCaption=Verder", Section(result, "CustomMessages"));
    }

    [Fact]
    public void Translations_do_not_cascade_from_the_default_screen()
    {
        var project = SampleProject();
        project.SupportedLanguageIds = ["dutch"];
        project.DefaultScreenButtons.NextButtonCaption = "Verder";
        project.DefaultScreenButtons.NextButtonCaptionByLanguage["dutch"] = "Doorgaan";
        project.WelcomeScreenButtons.NextButtonCaptionByLanguage["dutch"] = "Begin";

        var messages = Section(Generate(project), "CustomMessages");

        Assert.Contains("dutch.BtnWelcomeNextCaption=Begin", messages);
        Assert.DoesNotContain(messages, line => line.Contains("Doorgaan", StringComparison.Ordinal));
    }

    // ---- ingeschakeld en zichtbaar -----------------------------------------------------------------

    [Fact]
    public void Enabled_is_only_written_as_false()
    {
        var project = SampleProject();
        project.WelcomeScreenButtons.NextButtonEnabled = true;
        project.WelcomeScreenButtons.BackButtonEnabled = true;
        project.ReadyScreenButtons.CancelButtonEnabled = false;

        var code = CodeText(Generate(project));

        Assert.Contains("WizardForm.CancelButton.Enabled := False;", code);
        Assert.DoesNotContain("Enabled := True", code);
        Assert.DoesNotContain("wpWelcome", code);
    }

    [Fact]
    public void Own_true_overrides_a_default_false_so_that_screen_gets_no_code()
    {
        var project = SampleProject();
        project.DefaultScreenButtons.CancelButtonEnabled = false;
        project.LicenseScreenButtons.CancelButtonEnabled = true;
        project.WizardScreens.ShowLicensePage = true;
        project.LicenseFilePath = @"C:\Docs\licentie.txt";

        var code = CodeText(Generate(project));

        Assert.Contains("wpWelcome:", code);
        Assert.DoesNotContain("wpLicense", code);
        Assert.DoesNotContain("Enabled := True", code);
    }

    [Fact]
    public void Visible_is_only_written_as_false()
    {
        var project = SampleProject();
        project.WelcomeScreenButtons.BackButtonVisible = true;
        project.ReadyScreenButtons.CancelButtonVisible = false;

        var code = CodeText(Generate(project));

        Assert.Contains("WizardForm.CancelButton.Visible := False;", code);
        Assert.DoesNotContain("Visible := True", code);
        Assert.DoesNotContain("wpWelcome", code);
    }

    // ---- lettertype, vet, tooltip en het terugzetten ---------------------------------------------------

    [Fact]
    public void Font_size_is_captured_reset_on_every_page_and_set_on_its_page()
    {
        var project = SampleProject();
        project.ReadyScreenButtons.NextButtonFontSize = 10;

        Assert.Equal(
            new[]
            {
                "var",
                "  InitNextFontSize: Integer;",
                string.Empty,
                "procedure InitializeWizard;",
                "begin",
                "  InitNextFontSize := WizardForm.NextButton.Font.Size;",
                "end;",
                string.Empty,
                "procedure CurPageChanged(CurPageID: Integer);",
                "begin",
                "  { Setup zet Font en Hint niet terug bij een paginawissel: eerst naar de beginwaarde. }",
                "  WizardForm.NextButton.Font.Size := InitNextFontSize;",
                "  case CurPageID of",
                "    wpReady:",
                "      begin",
                "        WizardForm.NextButton.Font.Size := 10;",
                "      end;",
                "  end;",
                "end;",
            },
            Section(Generate(project), "Code"));
    }

    [Fact]
    public void Reset_only_covers_the_properties_and_buttons_that_are_used()
    {
        var project = SampleProject();
        project.ReadyScreenButtons.NextButtonFontSize = 10;
        project.WelcomeScreenButtons.CancelButtonCaption = "Stop";

        var code = CodeText(Generate(project));

        Assert.Contains("WizardForm.NextButton.Font.Size := InitNextFontSize;", code);
        Assert.DoesNotContain(".Hint", code);
        Assert.DoesNotContain(".ShowHint", code);
        Assert.DoesNotContain("Font.Style", code);
        Assert.DoesNotContain("Font.Name", code);
        Assert.DoesNotContain("InitCancel", code);
        Assert.DoesNotContain("InitBack", code);
    }

    [Fact]
    public void Bold_is_captured_reset_to_the_initial_style_and_set_with_fsBold()
    {
        var project = SampleProject();
        project.WelcomeScreenButtons.NextButtonFontBold = true;
        project.ReadyScreenButtons.NextButtonFontBold = false;

        var result = Generate(project);
        var code = CodeText(result);

        Assert.Contains("  InitNextFontStyle: TFontStyles;", code);
        Assert.Contains("  InitNextFontStyle := WizardForm.NextButton.Font.Style;", code);
        Assert.Contains("  WizardForm.NextButton.Font.Style := InitNextFontStyle;\n  case CurPageID of", code);
        Assert.Contains("WizardForm.NextButton.Font.Style := [fsBold];", code);
        Assert.DoesNotContain("wpReady", code);
    }

    [Fact]
    public void Font_name_is_captured_reset_and_quoted()
    {
        var project = SampleProject();
        project.WelcomeScreenButtons.NextButtonFontFamily = "Segoe 'UI'";

        var code = CodeText(Generate(project));

        Assert.Contains("  InitNextFontName: String;", code);
        Assert.Contains("  InitNextFontName := WizardForm.NextButton.Font.Name;", code);
        Assert.Contains("  WizardForm.NextButton.Font.Name := InitNextFontName;", code);
        Assert.Contains("        WizardForm.NextButton.Font.Name := 'Segoe ''UI''';", code);
    }

    [Fact]
    public void Non_positive_font_size_is_ignored()
    {
        var project = SampleProject();
        project.WelcomeScreenButtons.NextButtonFontSize = -3;

        Assert.DoesNotContain("[Code]", Generate(project).Script);
    }

    [Fact]
    public void Tooltip_sets_hint_and_show_hint_and_is_reset_on_every_page()
    {
        var project = SampleProject();
        project.WelcomeScreenButtons.NextButtonTooltip = "Ga verder";

        var result = Generate(project);
        var code = CodeText(result);

        Assert.Equal(new[] { "BtnWelcomeNextTooltip=Ga verder" }, Section(result, "CustomMessages"));
        Assert.Contains("  WizardForm.NextButton.Hint := '';\n  WizardForm.NextButton.ShowHint := False;\n  case CurPageID of", code);
        Assert.Contains(
            "        WizardForm.NextButton.Hint := CustomMessage('BtnWelcomeNextTooltip');\n"
            + "        WizardForm.NextButton.ShowHint := True;",
            code);
    }

    [Fact]
    public void Tooltip_with_only_a_translation_is_guarded()
    {
        var project = SampleProject();
        project.SupportedLanguageIds = ["dutch"];
        project.WelcomeScreenButtons.NextButtonTooltipByLanguage["dutch"] = "Ga verder";

        var result = Generate(project);

        Assert.Equal(new[] { "BtnWelcomeNextTooltip=", "dutch.BtnWelcomeNextTooltip=Ga verder" }, Section(result, "CustomMessages"));
        Assert.Contains(
            "        if CustomMessage('BtnWelcomeNextTooltip') <> '' then\n"
            + "        begin\n"
            + "          WizardForm.NextButton.Hint := CustomMessage('BtnWelcomeNextTooltip');\n"
            + "          WizardForm.NextButton.ShowHint := True;\n"
            + "        end;",
            CodeText(result));
    }

    [Fact]
    public void Statements_for_one_button_come_in_a_fixed_order()
    {
        var project = SampleProject();
        var settings = project.WelcomeScreenButtons;
        settings.CancelButtonVisible = false;
        settings.CancelButtonEnabled = false;
        settings.CancelButtonTooltip = "Tip";
        settings.CancelButtonFontBold = true;
        settings.CancelButtonFontSize = 9;
        settings.CancelButtonFontFamily = "Arial";
        settings.CancelButtonCaption = "Stop";
        settings.BackButtonCaption = "Terug";

        var lines = Section(Generate(project), "Code")
            .Where(line => line.StartsWith("        ", StringComparison.Ordinal))
            .Select(line => line.Trim())
            .ToArray();

        Assert.Equal(
            new[]
            {
                "WizardForm.BackButton.Caption := CustomMessage('BtnWelcomeBackCaption');",
                "WizardForm.CancelButton.Caption := CustomMessage('BtnWelcomeCancelCaption');",
                "WizardForm.CancelButton.Font.Name := 'Arial';",
                "WizardForm.CancelButton.Font.Size := 9;",
                "WizardForm.CancelButton.Font.Style := [fsBold];",
                "WizardForm.CancelButton.Hint := CustomMessage('BtnWelcomeCancelTooltip');",
                "WizardForm.CancelButton.ShowHint := True;",
                "WizardForm.CancelButton.Enabled := False;",
                "WizardForm.CancelButton.Visible := False;",
            },
            lines);
    }

    // ---- Bladeren-knoppen --------------------------------------------------------------------------

    [Fact]
    public void Browse_button_is_set_once_in_InitializeWizard_without_a_page_change_handler()
    {
        var project = SampleProject();
        project.SupportedLanguageIds = ["dutch"];
        project.SelectDestinationBrowseButton.Caption = "Find...";
        project.SelectDestinationBrowseButton.CaptionByLanguage["dutch"] = "Zoeken...";
        project.SelectDestinationBrowseButton.Enabled = false;
        project.SelectProgramGroupBrowseButton.Visible = false;
        project.SelectProgramGroupBrowseButton.FontBold = true;

        var result = Generate(project);

        Assert.Equal(
            new[] { "BtnSelectDirBrowseCaption=Find...", "dutch.BtnSelectDirBrowseCaption=Zoeken..." },
            Section(result, "CustomMessages"));
        Assert.Equal(
            new[]
            {
                "procedure InitializeWizard;",
                "begin",
                "  WizardForm.DirBrowseButton.Caption := CustomMessage('BtnSelectDirBrowseCaption');",
                "  WizardForm.DirBrowseButton.Enabled := False;",
                "  WizardForm.GroupBrowseButton.Font.Style := [fsBold];",
                "  WizardForm.GroupBrowseButton.Visible := False;",
                "end;",
            },
            Section(result, "Code"));
    }

    // ---- pagina's die uit staan -------------------------------------------------------------------

    [Fact]
    public void Hidden_screen_gets_no_code_and_an_info_message()
    {
        var project = SampleProject();
        project.LicenseScreenButtons.NextButtonCaption = "Akkoord";

        var result = Generate(project);

        var issue = Assert.Single(result.Issues);
        Assert.Equal(GenerationSeverity.Info, issue.Severity);
        Assert.Equal(GenerationIssueCode.ButtonSettingsForHiddenScreen, issue.Code);
        Assert.Equal(new[] { "LicenseScreenButtons" }, issue.Arguments);
        Assert.DoesNotContain("[Code]", result.Script);
        Assert.DoesNotContain("[CustomMessages]", result.Script);
    }

    [Fact]
    public void Hidden_screen_with_a_browse_button_reports_the_browse_settings_separately()
    {
        var project = SampleProject();
        project.WizardScreens.ShowSelectDestinationPage = false;
        project.SelectDestinationScreenButtons.NextButtonCaption = "Volgende";
        project.SelectDestinationBrowseButton.Tooltip = "Kies een map";

        var infos = IssuesOf(Generate(project), GenerationIssueCode.ButtonSettingsForHiddenScreen);

        Assert.Equal(
            new[] { "SelectDestinationBrowseButton", "SelectDestinationScreenButtons" },
            infos.Select(i => i.Arguments.Single()).Order().ToArray());
    }

    [Fact]
    public void Visible_screen_with_settings_gives_no_hidden_screen_message()
    {
        var project = SampleProject();
        project.ReadyScreenButtons.NextButtonCaption = "Installeren";

        Assert.False(HasIssue(Generate(project), GenerationIssueCode.ButtonSettingsForHiddenScreen));
    }

    [Fact]
    public void Default_screen_alone_never_gives_a_hidden_screen_message()
    {
        var project = SampleProject();
        project.DefaultScreenButtons.CancelButtonCaption = "Stop";

        Assert.False(HasIssue(Generate(project), GenerationIssueCode.ButtonSettingsForHiddenScreen));
    }

    // ---- Volgende onbruikbaar ----------------------------------------------------------------------

    [Theory]
    [InlineData(false, null)]
    [InlineData(null, false)]
    public void Disabled_or_hidden_next_button_is_a_warning(bool? enabled, bool? visible)
    {
        var project = SampleProject();
        project.ReadyScreenButtons.NextButtonEnabled = enabled;
        project.ReadyScreenButtons.NextButtonVisible = visible;

        var issue = Assert.Single(IssuesOf(Generate(project), GenerationIssueCode.NextButtonUnusable));

        Assert.Equal(GenerationSeverity.Warning, issue.Severity);
        Assert.Equal(new[] { "ReadyScreenButtons" }, issue.Arguments);
    }

    [Fact]
    public void Next_button_disabled_through_the_default_screen_warns_for_every_shown_screen()
    {
        var project = SampleProject();
        project.DefaultScreenButtons.NextButtonEnabled = false;

        var arguments = IssuesOf(Generate(project), GenerationIssueCode.NextButtonUnusable)
            .Select(i => i.Arguments.Single())
            .ToArray();

        Assert.Equal(
            new[]
            {
                "WelcomeScreenButtons", "SelectDestinationScreenButtons", "SelectProgramGroupScreenButtons", "ReadyScreenButtons",
            },
            arguments);
    }

    [Fact]
    public void Back_and_cancel_buttons_never_give_the_unusable_warning()
    {
        var project = SampleProject();
        project.ReadyScreenButtons.BackButtonEnabled = false;
        project.ReadyScreenButtons.CancelButtonVisible = false;

        Assert.False(HasIssue(Generate(project), GenerationIssueCode.NextButtonUnusable));
    }

    [Fact]
    public void Disabled_next_button_on_a_hidden_screen_gives_no_warning()
    {
        var project = SampleProject();
        project.LicenseScreenButtons.NextButtonEnabled = false;

        Assert.False(HasIssue(Generate(project), GenerationIssueCode.NextButtonUnusable));
    }

    // ---- samenhang tussen [CustomMessages] en [Code] ---------------------------------------------------

    // Een onbekende berichtnaam in CustomMessage() is een fatale fout tijdens het draaien van Setup,
    // en ISCC ziet dat niet. Daarom hier: elke naam die de code gebruikt, staat ook gedefinieerd.
    [Fact]
    public void Every_message_used_in_the_code_is_defined_and_every_definition_is_used()
    {
        var project = SampleProject();
        project.LicenseFilePath = @"C:\Docs\licentie.txt";
        project.InfoBeforeFilePath = @"C:\Docs\voor.rtf";
        project.InfoAfterFilePath = @"C:\Docs\na.txt";
        ApplyButtonSettings(project);

        var result = Generate(project);

        var defined = Section(result, "CustomMessages")
            .Select(line => line[..line.IndexOf('=', StringComparison.Ordinal)])
            .Select(name => name[(name.IndexOf('.', StringComparison.Ordinal) + 1)..])
            .ToHashSet(StringComparer.Ordinal);
        var used = System.Text.RegularExpressions.Regex
            .Matches(CodeText(result), @"CustomMessage\('([A-Za-z]+)'\)")
            .Select(match => match.Groups[1].Value)
            .ToHashSet(StringComparer.Ordinal);

        Assert.NotEmpty(defined);
        Assert.Equal(defined.Order().ToArray(), used.Order().ToArray());
    }

    [Fact]
    public void Unprefixed_message_always_comes_before_its_translations()
    {
        var project = SampleProject();
        ApplyButtonSettings(project);

        var names = Section(Generate(project), "CustomMessages")
            .Select(line => line[..line.IndexOf('=', StringComparison.Ordinal)])
            .ToArray();

        foreach (var translated in names.Where(name => name.Contains('.', StringComparison.Ordinal)))
        {
            var plain = translated[(translated.IndexOf('.', StringComparison.Ordinal) + 1)..];
            Assert.True(
                Array.IndexOf(names, plain) is var index and >= 0 && index < Array.IndexOf(names, translated),
                plain + " staat niet vóór " + translated);
        }
    }

    // ---- het volledige voorbeeld uit het ontwerp --------------------------------------------------------

    [Fact]
    public void Design_document_example_is_generated_as_documented()
    {
        var project = SampleProject();
        project.SupportedLanguageIds = ["dutch"];
        project.WelcomeScreenButtons.NextButtonCaption = "Start";
        project.WelcomeScreenButtons.NextButtonCaptionByLanguage["dutch"] = "Begin";
        project.WelcomeScreenButtons.NextButtonTooltip = "Ga verder";
        project.ReadyScreenButtons.NextButtonCaption = "Install now";
        project.ReadyScreenButtons.NextButtonCaptionByLanguage["dutch"] = "Nu installeren";
        project.ReadyScreenButtons.NextButtonFontSize = 10;
        project.SelectDestinationBrowseButton.Caption = "Find...";
        project.SelectDestinationBrowseButton.CaptionByLanguage["dutch"] = "Zoeken...";

        var result = Generate(project);

        Assert.Equal(
            new[]
            {
                "BtnWelcomeNextCaption=Start",
                "dutch.BtnWelcomeNextCaption=Begin",
                "BtnWelcomeNextTooltip=Ga verder",
                "BtnSelectDirBrowseCaption=Find...",
                "dutch.BtnSelectDirBrowseCaption=Zoeken...",
                "BtnReadyNextCaption=Install now",
                "dutch.BtnReadyNextCaption=Nu installeren",
            },
            Section(result, "CustomMessages"));
        Assert.Equal(
            new[]
            {
                "var",
                "  InitNextFontSize: Integer;",
                string.Empty,
                "procedure InitializeWizard;",
                "begin",
                "  InitNextFontSize := WizardForm.NextButton.Font.Size;",
                "  WizardForm.DirBrowseButton.Caption := CustomMessage('BtnSelectDirBrowseCaption');",
                "end;",
                string.Empty,
                "procedure CurPageChanged(CurPageID: Integer);",
                "begin",
                "  { Setup zet Font en Hint niet terug bij een paginawissel: eerst naar de beginwaarde. }",
                "  WizardForm.NextButton.Font.Size := InitNextFontSize;",
                "  WizardForm.NextButton.Hint := '';",
                "  WizardForm.NextButton.ShowHint := False;",
                "  case CurPageID of",
                "    wpWelcome:",
                "      begin",
                "        WizardForm.NextButton.Caption := CustomMessage('BtnWelcomeNextCaption');",
                "        WizardForm.NextButton.Hint := CustomMessage('BtnWelcomeNextTooltip');",
                "        WizardForm.NextButton.ShowHint := True;",
                "      end;",
                "    wpReady:",
                "      begin",
                "        WizardForm.NextButton.Caption := CustomMessage('BtnReadyNextCaption');",
                "        WizardForm.NextButton.Font.Size := 10;",
                "      end;",
                "  end;",
                "end;",
            },
            Section(result, "Code"));
        Assert.Empty(result.Issues);
    }
}
