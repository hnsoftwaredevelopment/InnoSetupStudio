using System.Collections;
using System.Globalization;
using InnoSetupStudio.Core.Project;

namespace InnoSetupStudio.Core.Generation;

/// <summary>Eén regel voor de sectie [CustomMessages]: de naam (eventueel met taalvoorvoegsel, zoals
/// <c>dutch.BtnWelcomeNextCaption</c>) en de tekst.</summary>
internal sealed record CustomMessageLine(string Name, string Value);

/// <summary>Wat <see cref="ButtonScript"/> aan het script toevoegt: de berichten voor
/// [CustomMessages] en de regels voor [Code]. Beide zijn leeg als er niets is aangepast.</summary>
internal sealed record ButtonScriptOutput(IReadOnlyList<CustomMessageLine> Messages, IReadOnlyList<string> CodeLines);

/// <summary>
/// Bouwt het knopgedeelte van het script (stap 4, zie docs/Ontwerp-Knopinstellingen-Generator.md):
/// [CustomMessages] met de knopteksten en tooltips, en een [Code]-blok dat die bij het openen van
/// de wizard en bij elke paginawissel op de knoppen zet. De knopmodellen komen uit
/// <see cref="InstallerProject"/>; de waarde per knop bepaalt <see cref="ButtonSettingsResolver"/>.
/// </summary>
internal sealed class ButtonScript
{
    private static readonly WizardButton[] ButtonOrder = [WizardButton.Back, WizardButton.Next, WizardButton.Cancel];

    private readonly InstallerProject _project;
    private readonly WizardScreenSelection _screens;
    private readonly IReadOnlyList<string> _languageIds;
    private readonly Action<GenerationSeverity, GenerationIssueCode, string[]> _report;
    private readonly List<CustomMessageLine> _messages = [];

    /// <param name="languageIds">De talen (zonder Engels) die in [Languages] komen, in
    /// cataloguvolgorde. Vertalingen voor andere talen worden genegeerd.</param>
    /// <param name="report">Meldt een bevinding aan de generator.</param>
    public ButtonScript(
        InstallerProject project,
        WizardScreenSelection screens,
        IReadOnlyList<string> languageIds,
        Action<GenerationSeverity, GenerationIssueCode, string[]> report)
    {
        _project = project;
        _screens = screens;
        _languageIds = languageIds;
        _report = report;
    }

    // Verwijzing naar een bericht in [CustomMessages]. Guarded: de universele tekst is leeg, dus de
    // code moet controleren of CustomMessage iets teruggeeft voordat het de waarde toewijst.
    private readonly record struct MessageRef(string Name, bool Guarded);

    private sealed record ButtonPlan(
        WizardButton? Button,
        string Target,
        EffectiveButtonSettings Settings,
        MessageRef? Caption,
        MessageRef? Tooltip);

    private sealed record PagePlan(string PageId, IReadOnlyDictionary<WizardButton, ButtonPlan> Buttons);

    // Een wizardscherm met zijn eigenschap in InstallerProject, zijn pagina-ID in Setup en, voor de
    // twee mappenpagina's, de Bladeren-knop.
    private sealed record ScreenSpec(
        string Key,
        string FieldName,
        string PageId,
        bool Shown,
        WizardScreenButtonSettings? Settings,
        string? BrowseFieldName = null,
        string? BrowseTarget = null,
        BrowseButtonSettings? Browse = null);

    public ButtonScriptOutput Build()
    {
        var defaults = Sanitize("DefaultScreenButtons", _project.DefaultScreenButtons);
        var pages = new List<PagePlan>();
        var browseButtons = new List<ButtonPlan>();
        var textColors = 0;

        foreach (var spec in Specs())
        {
            if (!spec.Shown)
            {
                ReportHiddenScreen(spec);
                continue;
            }

            var settings = Sanitize(spec.FieldName, spec.Settings);
            var buttons = new Dictionary<WizardButton, ButtonPlan>();
            foreach (var button in ButtonOrder)
            {
                var effective = ButtonSettingsResolver.Resolve(settings, defaults, button);
                buttons[button] = Plan(
                    "Btn" + spec.Key + button,
                    button,
                    "WizardForm." + button + "Button",
                    effective);
                textColors += HasTextColor(effective) ? 1 : 0;
            }

            var next = buttons[WizardButton.Next].Settings;
            if (next.Enabled == false || next.Visible == false)
            {
                _report(GenerationSeverity.Warning, GenerationIssueCode.NextButtonUnusable, [spec.FieldName]);
            }

            pages.Add(new PagePlan(spec.PageId, buttons));

            if (spec.BrowseFieldName is not null && spec.BrowseTarget is not null)
            {
                var effective = ButtonSettingsResolver.Resolve(Sanitize(spec.BrowseFieldName, spec.Browse));
                browseButtons.Add(Plan("Btn" + spec.Key + "Browse", null, spec.BrowseTarget, effective));
                textColors += HasTextColor(effective) ? 1 : 0;
            }
        }

        if (textColors > 0)
        {
            _report(
                GenerationSeverity.Warning,
                GenerationIssueCode.ButtonTextColorNotSupported,
                [textColors.ToString(CultureInfo.InvariantCulture)]);
        }

        return new ButtonScriptOutput(_messages, BuildCode(pages, browseButtons));
    }

    private IEnumerable<ScreenSpec> Specs()
    {
        var p = _project;
        var s = _screens;
        yield return new ScreenSpec("Welcome", nameof(InstallerProject.WelcomeScreenButtons), "wpWelcome", s.ShowWelcomePage, p.WelcomeScreenButtons);
        yield return new ScreenSpec("License", nameof(InstallerProject.LicenseScreenButtons), "wpLicense", s.ShowLicensePage, p.LicenseScreenButtons);
        yield return new ScreenSpec("InfoBefore", nameof(InstallerProject.InfoBeforeScreenButtons), "wpInfoBefore", s.ShowInfoBeforePage, p.InfoBeforeScreenButtons);
        yield return new ScreenSpec("UserInfo", nameof(InstallerProject.UserInfoScreenButtons), "wpUserInfo", s.ShowUserInfoPage, p.UserInfoScreenButtons);
        yield return new ScreenSpec(
            "SelectDir", nameof(InstallerProject.SelectDestinationScreenButtons), "wpSelectDir", s.ShowSelectDestinationPage, p.SelectDestinationScreenButtons,
            nameof(InstallerProject.SelectDestinationBrowseButton), "WizardForm.DirBrowseButton", p.SelectDestinationBrowseButton);
        yield return new ScreenSpec(
            "SelectGroup", nameof(InstallerProject.SelectProgramGroupScreenButtons), "wpSelectProgramGroup", s.ShowSelectProgramGroupPage, p.SelectProgramGroupScreenButtons,
            nameof(InstallerProject.SelectProgramGroupBrowseButton), "WizardForm.GroupBrowseButton", p.SelectProgramGroupBrowseButton);
        yield return new ScreenSpec("Ready", nameof(InstallerProject.ReadyScreenButtons), "wpReady", s.ShowReadyPage, p.ReadyScreenButtons);
        yield return new ScreenSpec("InfoAfter", nameof(InstallerProject.InfoAfterScreenButtons), "wpInfoAfter", s.ShowInfoAfterPage, p.InfoAfterScreenButtons);
    }

    // Een scherm dat uit staat krijgt geen code. Eigen instellingen erop worden gemeld.
    private void ReportHiddenScreen(ScreenSpec spec)
    {
        if (spec.Settings is not null && HasCustomizations(spec.Settings))
        {
            _report(GenerationSeverity.Info, GenerationIssueCode.ButtonSettingsForHiddenScreen, [spec.FieldName]);
        }

        if (spec.BrowseFieldName is not null && spec.Browse is not null && HasCustomizations(spec.Browse))
        {
            _report(GenerationSeverity.Info, GenerationIssueCode.ButtonSettingsForHiddenScreen, [spec.BrowseFieldName]);
        }
    }

    private ButtonPlan Plan(string messagePrefix, WizardButton? button, string target, EffectiveButtonSettings settings)
    {
        var caption = AddMessage(messagePrefix + "Caption", settings.Caption, settings.CaptionByLanguage);
        var tooltip = AddMessage(messagePrefix + "Tooltip", settings.Tooltip, settings.TooltipByLanguage);
        return new ButtonPlan(button, target, settings, caption, tooltip);
    }

    // Schrijft de berichtregels (zie sectie 6 van het ontwerp): eerst de regel zonder taalvoorvoegsel
    // met de universele tekst, daarna per taal met een vertaling. Zonder tekst en zonder vertaling
    // komt er geen bericht en geen verwijzing.
    private MessageRef? AddMessage(string name, string universal, IReadOnlyDictionary<string, string> translations)
    {
        var texts = new List<CustomMessageLine>();
        foreach (var languageId in _languageIds)
        {
            if (translations.TryGetValue(languageId, out var text) && !string.IsNullOrWhiteSpace(text))
            {
                texts.Add(new CustomMessageLine(languageId + "." + name, text));
            }
        }

        if (universal.Length == 0 && texts.Count == 0)
        {
            return null;
        }

        _messages.Add(new CustomMessageLine(name, universal));
        _messages.AddRange(texts);
        return new MessageRef(name, Guarded: universal.Length == 0);
    }

    private static bool HasTextColor(EffectiveButtonSettings settings) => !string.IsNullOrWhiteSpace(settings.TextColor);

    private static List<string> BuildCode(List<PagePlan> pages, List<ButtonPlan> browseButtons)
    {
        var pageButtons = pages.SelectMany(page => page.Buttons.Values).ToList();

        // Per knop: welke eigenschappen worden op minstens één pagina gebruikt. Alleen die worden
        // vastgelegd en teruggezet.
        var uses = ButtonOrder.ToDictionary(
            button => button,
            button =>
            {
                var plans = pageButtons.Where(plan => plan.Button == button).ToList();
                return new Usage(
                    plans.Any(plan => !string.IsNullOrWhiteSpace(plan.Settings.FontFamily)),
                    plans.Any(plan => plan.Settings.FontSize > 0),
                    plans.Any(plan => plan.Settings.FontBold == true),
                    plans.Any(plan => plan.Tooltip is not null));
            });

        var variables = new List<string>();
        var initialize = new List<string>();
        var resets = new List<string>();
        foreach (var button in ButtonOrder)
        {
            var use = uses[button];
            var target = "WizardForm." + button + "Button";
            var nameVariable = "Init" + button + "FontName";
            var sizeVariable = "Init" + button + "FontSize";
            if (use.FontName)
            {
                variables.Add(nameVariable + ": String;");
                initialize.Add(nameVariable + " := " + target + ".Font.Name;");
                resets.Add(target + ".Font.Name := " + nameVariable + ";");
            }

            if (use.FontSize)
            {
                variables.Add(sizeVariable + ": Integer;");
                initialize.Add(sizeVariable + " := " + target + ".Font.Size;");
                resets.Add(target + ".Font.Size := " + sizeVariable + ";");
            }

            if (use.Bold)
            {
                resets.Add(target + ".Font.Style := [];");
            }

            if (use.Tooltip)
            {
                resets.Add(target + ".Hint := '';");
                resets.Add(target + ".ShowHint := False;");
            }
        }

        foreach (var browse in browseButtons)
        {
            initialize.AddRange(Statements(browse));
        }

        var cases = new List<(string PageId, List<string> Statements)>();
        foreach (var page in pages)
        {
            var statements = ButtonOrder.SelectMany(button => Statements(page.Buttons[button])).ToList();
            if (statements.Count > 0)
            {
                cases.Add((page.PageId, statements));
            }
        }

        var lines = new List<string>();
        if (variables.Count > 0)
        {
            lines.Add("var");
            lines.AddRange(variables.Select(v => "  " + v));
        }

        if (initialize.Count > 0)
        {
            AddBlankSeparator(lines);
            lines.Add("procedure InitializeWizard;");
            lines.Add("begin");
            lines.AddRange(initialize.Select(l => "  " + l));
            lines.Add("end;");
        }

        if (cases.Count > 0)
        {
            AddBlankSeparator(lines);
            lines.Add("procedure CurPageChanged(CurPageID: Integer);");
            lines.Add("begin");
            if (resets.Count > 0)
            {
                lines.Add("  { Setup zet Font en Hint niet terug bij een paginawissel: eerst naar de beginwaarde. }");
                lines.AddRange(resets.Select(l => "  " + l));
            }

            lines.Add("  case CurPageID of");
            foreach (var (pageId, statements) in cases)
            {
                lines.Add("    " + pageId + ":");
                lines.Add("      begin");
                lines.AddRange(statements.Select(l => "        " + l));
                lines.Add("      end;");
            }

            lines.Add("  end;");
            lines.Add("end;");
        }

        return lines;
    }

    private readonly record struct Usage(bool FontName, bool FontSize, bool Bold, bool Tooltip);

    private static void AddBlankSeparator(List<string> lines)
    {
        if (lines.Count > 0)
        {
            lines.Add(string.Empty);
        }
    }

    // De Pascal-regels die één knop instellen. Enabled en Visible alleen als False: Setup zet
    // beide bij elke paginawissel zelf terug (en zet Volgende op de Licentie-pagina uit tot de
    // licentie is geaccepteerd), dus een expliciet True voegt niets toe en kan een controle van
    // Setup omzeilen. Lettertype, vet en tooltip alleen als ze zijn ingesteld; wat Setup niet
    // terugzet, regelt de reset in CurPageChanged.
    private static List<string> Statements(ButtonPlan plan)
    {
        var s = plan.Settings;
        var target = plan.Target;
        var lines = new List<string>();

        if (plan.Caption is { } caption)
        {
            if (caption.Guarded)
            {
                lines.Add("if CustomMessage('" + caption.Name + "') <> '' then");
                lines.Add("  " + target + ".Caption := CustomMessage('" + caption.Name + "');");
            }
            else
            {
                lines.Add(target + ".Caption := CustomMessage('" + caption.Name + "');");
            }
        }

        if (!string.IsNullOrWhiteSpace(s.FontFamily))
        {
            lines.Add(target + ".Font.Name := '" + s.FontFamily.Replace("'", "''", StringComparison.Ordinal) + "';");
        }

        if (s.FontSize > 0)
        {
            lines.Add(target + ".Font.Size := " + s.FontSize.Value.ToString(CultureInfo.InvariantCulture) + ";");
        }

        if (s.FontBold == true)
        {
            lines.Add(target + ".Font.Style := [fsBold];");
        }

        if (plan.Tooltip is { } tooltip)
        {
            if (tooltip.Guarded)
            {
                lines.Add("if CustomMessage('" + tooltip.Name + "') <> '' then");
                lines.Add("begin");
                lines.Add("  " + target + ".Hint := CustomMessage('" + tooltip.Name + "');");
                lines.Add("  " + target + ".ShowHint := True;");
                lines.Add("end;");
            }
            else
            {
                lines.Add(target + ".Hint := CustomMessage('" + tooltip.Name + "');");
                lines.Add(target + ".ShowHint := True;");
            }
        }

        if (s.Enabled == false)
        {
            lines.Add(target + ".Enabled := False;");
        }

        if (s.Visible == false)
        {
            lines.Add(target + ".Visible := False;");
        }

        return lines;
    }

    // Een kopie van de instellingen waarin elke tekst is getrimd. Een tekst met een regeleinde zou
    // het .iss beschadigen: die wordt gemeld en weggelaten. De tekstkleur wordt niet gegenereerd en
    // blijft ongemoeid.
    private T? Sanitize<T>(string objectName, T? source)
        where T : class, new()
    {
        if (source is null)
        {
            return null;
        }

        var clone = new T();
        foreach (var property in typeof(T).GetProperties())
        {
            var fieldName = objectName + "." + property.Name;
            var value = property.GetValue(source);
            switch (value)
            {
                case string text when property.Name.EndsWith("TextColor", StringComparison.Ordinal):
                    property.SetValue(clone, text);
                    break;
                case string text:
                    property.SetValue(clone, CleanText(fieldName, text) ?? string.Empty);
                    break;
                case Dictionary<string, string> translations:
                    var cleaned = new Dictionary<string, string>(StringComparer.Ordinal);
                    foreach (var (languageId, translation) in translations)
                    {
                        var clean = CleanText(fieldName + "[" + languageId + "]", translation);
                        if (!string.IsNullOrEmpty(clean))
                        {
                            cleaned[languageId] = clean;
                        }
                    }

                    property.SetValue(clone, cleaned);
                    break;
                default:
                    property.SetValue(clone, value);
                    break;
            }
        }

        return clone;
    }

    private string? CleanText(string fieldName, string? raw)
    {
        var value = raw?.Trim();
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }

        if (IssEscape.ContainsLineBreak(value))
        {
            _report(GenerationSeverity.Error, GenerationIssueCode.ValueContainsLineBreak, [fieldName]);
            return null;
        }

        return value;
    }

    // True als er in deze instellingen (WizardScreenButtonSettings of BrowseButtonSettings) iets
    // is aangepast: een niet-lege tekst, een gevulde dictionary of een waarde die niet null is.
    private static bool HasCustomizations(object settings)
    {
        foreach (var property in settings.GetType().GetProperties())
        {
            switch (property.GetValue(settings))
            {
                case string { Length: > 0 }:
                case IDictionary { Count: > 0 }:
                case bool:
                case int:
                    return true;
            }
        }

        return false;
    }
}
