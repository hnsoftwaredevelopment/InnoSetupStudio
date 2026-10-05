using System.Collections;
using System.Text;
using InnoSetupStudio.Core.Project;

namespace InnoSetupStudio.Core.Generation;

/// <summary>
/// Zet een <see cref="InstallerProject"/> om naar de tekst van een Inno Setup-script (.iss).
/// Versie 1 ("dunne generator", zie docs/Ontwerp-Dunne-Generator.md): alleen wat Inno Setup zonder
/// Pascal Script kan, dus de secties [Setup], [Languages], [Tasks], [Files] en [Icons]. Knopinstellingen
/// en alles wat een [Code]-blok vraagt volgen in een latere stap en worden nu gemeld als
/// <see cref="GenerationIssueCode.ButtonSettingsNotGenerated"/>.
///
/// Eigenschappen van de uitvoer: deterministisch (zelfde project, zelfde tekst), alleen richtlijnen
/// die van Inno Setup's eigen standaard afwijken (behalve de basisgegevens), en CRLF-regeleinden.
/// De aanroeper schrijft de tekst weg met <see cref="ScriptEncoding"/> (UTF-8 met BOM).
/// </summary>
public sealed class IssGenerator
{
    /// <summary>Codering waarmee het .iss moet worden weggeschreven: UTF-8 met BOM, zodat Inno Setup
    /// tekens als "é" correct leest.</summary>
    public static Encoding ScriptEncoding { get; } = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);

    private readonly IGeneratorEnvironment _environment;

    public IssGenerator()
        : this(new DiskGeneratorEnvironment())
    {
    }

    public IssGenerator(IGeneratorEnvironment environment)
    {
        _environment = environment;
    }

    public GenerationResult Generate(InstallerProject project)
    {
        ArgumentNullException.ThrowIfNull(project);
        return new Run(project, _environment).Execute();
    }

    // Eén Run per aanroep, zodat de generator zelf geen toestand heeft en veilig hergebruikt kan
    // worden.
    private sealed class Run
    {
        private readonly InstallerProject _project;
        private readonly IGeneratorEnvironment _environment;
        private readonly List<GenerationIssue> _issues = [];
        private readonly IssWriter _writer = new();

        public Run(InstallerProject project, IGeneratorEnvironment environment)
        {
            _project = project;
            _environment = environment;
        }

        public GenerationResult Execute()
        {
            var p = _project;
            var screens = p.WizardScreens ?? new WizardScreenSelection();

            var appId = Clean("AppId", p.AppId);
            var appName = Clean("AppName", p.AppName);
            var appVersion = Clean("AppVersion", p.AppVersion);
            if (appId is null)
            {
                Add(GenerationSeverity.Error, GenerationIssueCode.AppIdMissing);
            }

            if (appName is null)
            {
                Add(GenerationSeverity.Error, GenerationIssueCode.AppNameMissing);
            }

            if (appVersion is null)
            {
                Add(GenerationSeverity.Error, GenerationIssueCode.AppVersionMissing);
            }

            var source = Clean("SourceFilesPath", p.SourceFilesPath)?.TrimEnd('\\', '/');
            if (string.IsNullOrEmpty(source))
            {
                source = null;
                Add(GenerationSeverity.Error, GenerationIssueCode.SourceFilesPathMissing);
            }
            else if (!_environment.DirectoryExists(source))
            {
                Add(GenerationSeverity.Warning, GenerationIssueCode.SourceFilesPathNotFound, source);
            }

            var mainExecutable = ResolveMainExecutable(source);

            _writer.Comment("Gegenereerd door Inno Setup Studio. Handmatige wijzigingen in dit bestand gaan");
            _writer.Comment("verloren zodra het opnieuw wordt gegenereerd.");

            WriteSetupSection(screens, appId, appName, appVersion, mainExecutable);
            WriteLanguagesSection();
            WriteTasksSection(screens);
            WriteFilesSection(source);
            WriteIconsSection(appName, mainExecutable);
            ReportUnsupported(screens);

            return new GenerationResult(_writer.ToString(), _issues);
        }

        private void WriteSetupSection(WizardScreenSelection screens, string? appId, string? appName, string? appVersion, string? mainExecutable)
        {
            var p = _project;
            _writer.Section("Setup");

            _writer.Heading("Toepassing");
            if (appId is not null)
            {
                _writer.Directive("AppId", IssEscape.Constants(appId));
            }

            if (appName is not null)
            {
                _writer.Directive("AppName", IssEscape.Constants(appName));
            }

            if (appVersion is not null)
            {
                _writer.Directive("AppVersion", IssEscape.Constants(appVersion));
            }

            Optional("AppPublisher", Clean("Publisher", p.Publisher));
            var url = Clean("PublisherUrl", p.PublisherUrl);
            Optional("AppPublisherURL", url);
            Optional("AppSupportURL", url);
            Optional("AppUpdatesURL", url);
            Optional("AppContact", Clean("PublisherEmail", p.PublisherEmail));

            _writer.Heading("Mappen en startmenu");

            // DefaultDirName en DefaultGroupName zijn bewust Inno Setup-constantenteksten
            // ("{autopf}\MijnApp"): niet escapen. Alleen de AppName in de terugvalwaarde is letterlijk.
            var escapedName = IssEscape.Constants(IssEscape.FileSystemName(appName ?? string.Empty));
            _writer.Directive("DefaultDirName", Clean("DefaultDirName", p.DefaultDirName) ?? "{autopf}\\" + escapedName);
            _writer.Directive("DefaultGroupName", Clean("DefaultGroupName", p.DefaultGroupName) ?? escapedName);
            Optional("DisableDirPage", PageModeValue(screens.ShowSelectDestinationPage, p.DirPageMode), escape: false);
            Optional("DisableProgramGroupPage", PageModeValue(screens.ShowSelectProgramGroupPage, p.GroupPageMode), escape: false);
            YesNoIfDifferent("AppendDefaultGroupName", p.AppendDefaultGroupName, innoDefault: true);
            YesNoIfDifferent("AlwaysUsePersonalGroup", p.AlwaysUsePersonalGroup, innoDefault: false);
            YesNoIfDifferent("UsePreviousAppDir", p.UsePreviousAppDir, innoDefault: true);
            YesNoIfDifferent("UsePreviousGroup", p.UsePreviousGroup, innoDefault: true);
            YesNoIfDifferent("UsePreviousSetupType", p.UsePreviousSetupType, innoDefault: true);
            YesNoIfDifferent("UsePreviousTasks", p.UsePreviousTasks, innoDefault: true);
            YesNoIfDifferent("UsePreviousLanguage", p.UsePreviousLanguage, innoDefault: true);

            _writer.Heading("Wizardpagina's");

            // Inno Setup's eigen standaard voor DisableWelcomePage is yes (geverifieerd in de
            // documentatie): een aangevinkte Welkomstpagina vraagt dus om expliciet "no".
            YesNoIfDifferent("DisableWelcomePage", !screens.ShowWelcomePage, innoDefault: true);
            PageFile(screens.ShowLicensePage, "LicenseFilePath", "LicenseFile", p.LicenseFilePath);
            PageFile(screens.ShowInfoBeforePage, "InfoBeforeFilePath", "InfoBeforeFile", p.InfoBeforeFilePath);
            PageFile(screens.ShowInfoAfterPage, "InfoAfterFilePath", "InfoAfterFile", p.InfoAfterFilePath);

            if (screens.ShowUserInfoPage)
            {
                _writer.Directive("UserInfoPage", "yes");
                Optional("DefaultUserInfoName", Clean("DefaultUserInfoName", p.DefaultUserInfoName));
                Optional("DefaultUserInfoOrg", Clean("DefaultUserInfoOrg", p.DefaultUserInfoOrg));
                Optional("DefaultUserInfoSerial", Clean("DefaultUserInfoSerial", p.DefaultUserInfoSerial));
                YesNoIfDifferent("UsePreviousUserInfo", p.UsePreviousUserInfo, innoDefault: true);
            }

            YesNoIfDifferent("DisableReadyPage", !screens.ShowReadyPage, innoDefault: false);
            if (screens.ShowReadyPage)
            {
                YesNoIfDifferent("DisableReadyMemo", p.DisableReadyMemo, innoDefault: false);
                YesNoIfDifferent("AlwaysShowDirOnReadyPage", p.AlwaysShowDirOnReadyPage, innoDefault: false);
                YesNoIfDifferent("AlwaysShowGroupOnReadyPage", p.AlwaysShowGroupOnReadyPage, innoDefault: false);
            }

            YesNoIfDifferent("DisableFinishedPage", !screens.ShowFinishedPage, innoDefault: false);

            _writer.Heading("Uiterlijk");
            OptionalFile("SetupIconFile", p.SetupIconFile);
            OptionalFile("WizardImageFile", p.WizardImageFile);
            OptionalFile("WizardSmallImageFile", p.WizardSmallImageFile);
            if (p.WizardStyle == InstallerWizardStyle.Modern)
            {
                _writer.Directive("WizardStyle", "modern");
            }

            _writer.Heading("Installatie en uitvoer");
            if (p.Architecture == InstallerArchitecture.X64)
            {
                // x64compatible: ook op Windows op Arm64 (x64-emulatie) in 64-bit modus installeren.
                // "x64" is volgens de documentatie een verouderde alias van x64os.
                _writer.Directive("ArchitecturesInstallIn64BitMode", "x64compatible");
            }

            _writer.Directive("Compression", "lzma2");
            _writer.Directive("SolidCompression", "yes");
            if (appName is not null)
            {
                _writer.Directive("UninstallDisplayName", IssEscape.Constants(appName));
            }

if (mainExecutable is not null)
            {
                _writer.Directive("UninstallDisplayIcon", "{app}\\" + IssEscape.Constants(mainExecutable));
            }

            Optional("OutputDir", Clean("OutputPath", p.OutputPath), escape: false);
            _writer.Directive("OutputBaseFilename", p.GetEffectiveOutputBaseFilename());
        }

        private void WriteLanguagesSection()
        {
            var requested = _project.SupportedLanguageIds ?? [];
            var known = InnoLanguageCatalog.Languages.Select(l => l.Id).ToHashSet(StringComparer.Ordinal);
            foreach (var id in requested.Where(id => !known.Contains(id)).Distinct(StringComparer.Ordinal))
            {
                Add(GenerationSeverity.Warning, GenerationIssueCode.UnknownLanguage, id);
            }

            _writer.Section("Languages");

            // Engels staat altijd in de lijst en altijd eerst: Setup valt voor een taal zonder
            // eigen tekst terug op de eerste taal. De rest in vaste cataloguvolgorde, voor een
            // deterministische uitvoer.
            foreach (var language in InnoLanguageCatalog.Languages)
            {
                if (language.Id == InnoLanguageCatalog.EnglishId || requested.Contains(language.Id))
                {
                    _writer.Entry(
                        IssWriter.Quoted("Name", language.Id),
                        IssWriter.Quoted("MessagesFile", language.MessagesFile));
                }
            }
        }

        private void WriteTasksSection(WizardScreenSelection screens)
        {
            if (!_project.CreateDesktopIcon)
            {
                if (screens.ShowSelectTasksPage)
                {
                    Add(GenerationSeverity.Warning, GenerationIssueCode.TasksPageWithoutTasks);
                }

                return;
            }

            if (!screens.ShowSelectTasksPage)
            {
                Add(GenerationSeverity.Warning, GenerationIssueCode.TasksPageShownForDesktopIcon);
            }

            _writer.Section("Tasks");
            _writer.Entry(
                IssWriter.Raw("Name", "\"desktopicon\""),
                IssWriter.Raw("Description", "\"{cm:CreateDesktopIcon}\""),
                IssWriter.Raw("GroupDescription", "\"{cm:AdditionalIcons}\""),
                IssWriter.Raw("Flags", "unchecked"));
        }

        private void WriteFilesSection(string? source)
        {
            if (source is null)
            {
                return;
            }

            _writer.Section("Files");
            _writer.Entry(
                IssWriter.Quoted("Source", source + "\\*"),
                IssWriter.Raw("DestDir", "\"{app}\""),
                IssWriter.Raw("Flags", "ignoreversion recursesubdirs createallsubdirs"));
        }

        private void WriteIconsSection(string? appName, string? mainExecutable)
        {
            var wantStartMenu = _project.CreateStartMenuIcon;
            var wantDesktop = _project.CreateDesktopIcon;
            if (!wantStartMenu && !wantDesktop)
            {
                return;
            }

            if (mainExecutable is null)
            {
                // Een ongeldig pad is al apart gemeld; alleen "niet gekozen" krijgt deze melding.
                if (string.IsNullOrWhiteSpace(_project.MainExecutable))
                {
                    Add(GenerationSeverity.Warning, GenerationIssueCode.ShortcutsWithoutMainExecutable);
                }

                return;
            }

            var shortcutName = IssEscape.Constants(IssEscape.FileSystemName(appName ?? string.Empty));
            var target = "{app}\\" + IssEscape.Constants(mainExecutable);
            _writer.Section("Icons");
            if (wantStartMenu)
            {
                _writer.Entry(
                    IssWriter.Quoted("Name", "{group}\\" + shortcutName),
                    IssWriter.Quoted("Filename", target),
                    IssWriter.Raw("WorkingDir", "\"{app}\""));
            }

            if (wantDesktop)
            {
                _writer.Entry(
                    IssWriter.Quoted("Name", "{autodesktop}\\" + shortcutName),
                    IssWriter.Quoted("Filename", target),
                    IssWriter.Raw("WorkingDir", "\"{app}\""),
                    IssWriter.Raw("Tasks", "desktopicon"));
            }
        }

        private void ReportUnsupported(WizardScreenSelection screens)
        {
            if (screens.ShowSelectComponentsPage)
            {
                Add(GenerationSeverity.Warning, GenerationIssueCode.ComponentsPageNotSupported);
            }

            var p = _project;
            var customized = new object?[]
            {
                p.WelcomeScreenButtons, p.LicenseScreenButtons, p.InfoBeforeScreenButtons,
                p.UserInfoScreenButtons, p.SelectDestinationScreenButtons, p.SelectProgramGroupScreenButtons,
                p.ReadyScreenButtons, p.InfoAfterScreenButtons, p.DefaultScreenButtons,
                p.SelectDestinationBrowseButton, p.SelectProgramGroupBrowseButton,
            }.Count(settings => settings is not null && HasCustomizations(settings));

            if (customized > 0)
            {
                Add(
                    GenerationSeverity.Info,
                    GenerationIssueCode.ButtonSettingsNotGenerated,
                    customized.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }
        }

        // Het hoofdprogramma als pad met backslashes, of null als er geen bruikbaar is gekozen.
        private string? ResolveMainExecutable(string? source)
        {
            var main = Clean("MainExecutable", _project.MainExecutable);
            if (main is null)
            {
                return null;
            }

            if (!InstallerProject.IsValidMainExecutablePath(main))
            {
                Add(GenerationSeverity.Warning, GenerationIssueCode.MainExecutableInvalid, main);
                return null;
            }

            main = main.Replace('/', '\\');
            if (source is not null
                && _environment.DirectoryExists(source)
                && !_environment.FileExists(Path.Combine(source, main)))
            {
                Add(GenerationSeverity.Warning, GenerationIssueCode.MainExecutableNotFound, main);
            }

            return main;
        }

        // Een wizardpagina met een bestand (licentie, info voor, info na): de richtlijn komt er
        // alleen als de pagina aan staat en er een bestand is gekozen.
        private void PageFile(bool pageShown, string fieldName, string directive, string? path)
        {
            if (!pageShown)
            {
                return;
            }

            var value = Clean(fieldName, path);
            if (value is null)
            {
                Add(GenerationSeverity.Warning, GenerationIssueCode.PageWithoutFile, fieldName);
                return;
            }

            _writer.Directive(directive, value);
            WarnIfFileMissing(fieldName, value);
        }

        private void OptionalFile(string directive, string? path)
        {
            var value = Clean(directive, path);
            if (value is null)
            {
                return;
            }

            _writer.Directive(directive, value);
            WarnIfFileMissing(directive, value);
        }

        private void WarnIfFileMissing(string fieldName, string path)
        {
            if (!_environment.FileExists(path))
            {
                Add(GenerationSeverity.Warning, GenerationIssueCode.FileNotFound, fieldName, path);
            }
        }

        private void Optional(string directive, string? value, bool escape = true)
        {
            if (value is not null)
            {
                _writer.Directive(directive, escape ? IssEscape.Constants(value) : value);
            }
        }

        private void YesNoIfDifferent(string directive, bool value, bool innoDefault)
        {
            if (value != innoDefault)
            {
                _writer.Directive(directive, value ? "yes" : "no");
            }
        }

        // DisableDirPage en DisableProgramGroupPage kennen no/yes/auto, en auto is Inno Setup's eigen
        // standaard (geverifieerd in de documentatie): dan hoeft er niets te staan.
        private static string? PageModeValue(bool pageShown, DisablePageMode mode)
        {
            if (!pageShown)
            {
                return "yes";
            }

            return mode switch
            {
                DisablePageMode.AlwaysShow => "no",
                DisablePageMode.NeverShow => "yes",
                DisablePageMode.AutoSkipIfKnown => null,
                _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null),
            };
        }

        // Trimt een waarde. Leeg wordt null. Een waarde met een regeleinde wordt gemeld en ook null:
        // zo'n waarde zou het .iss beschadigen.
        private string? Clean(string fieldName, string? raw)
        {
            var value = raw?.Trim();
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            if (IssEscape.ContainsLineBreak(value))
            {
                Add(GenerationSeverity.Error, GenerationIssueCode.ValueContainsLineBreak, fieldName);
                return null;
            }

            return value;
        }

        private void Add(GenerationSeverity severity, GenerationIssueCode code, params string[] arguments)
            => _issues.Add(new GenerationIssue(severity, code, arguments));

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
}
