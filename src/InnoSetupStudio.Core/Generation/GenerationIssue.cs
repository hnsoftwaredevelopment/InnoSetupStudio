namespace InnoSetupStudio.Core.Generation;

/// <summary>Hoe zwaar een melding van de generator weegt.</summary>
public enum GenerationSeverity
{
    /// <summary>Informatie: iets wat bewust niet (of nog niet) wordt vertaald.</summary>
    Info,

    /// <summary>Waarschuwing: het .iss wordt wel gegenereerd, maar het resultaat is mogelijk niet
    /// wat de gebruiker verwacht, of ISCC kan er een fout op geven.</summary>
    Warning,

    /// <summary>Fout: het .iss is niet bruikbaar. De aanroeper schrijft het bestand dan niet weg.</summary>
    Error,
}

/// <summary>
/// Stabiele code per soort melding. De Core kent geen vertalingen: de app zet een code (met de
/// bijbehorende <see cref="GenerationIssue.Arguments"/>) om naar een tekst in de taal van de
/// gebruiker.
/// </summary>
public enum GenerationIssueCode
{
    /// <summary>AppId is leeg. Argumenten: geen.</summary>
    AppIdMissing,

    /// <summary>AppName is leeg. Argumenten: geen.</summary>
    AppNameMissing,

    /// <summary>AppVersion is leeg. Argumenten: geen.</summary>
    AppVersionMissing,

    /// <summary>De map met bronbestanden is leeg, dus er is niets om te installeren. Argumenten: geen.</summary>
    SourceFilesPathMissing,

    /// <summary>Een waarde bevat een regeleinde en is daarom weggelaten. Argumenten: veldnaam.</summary>
    ValueContainsLineBreak,

    /// <summary>De map met bronbestanden bestaat niet op schijf. Argumenten: pad.</summary>
    SourceFilesPathNotFound,

    /// <summary>Een wizardpagina staat aan, maar het bijbehorende bestand is niet gekozen.
    /// Argumenten: veldnaam van het bestand.</summary>
    PageWithoutFile,

    /// <summary>Een gekozen bestand bestaat niet op schijf. Argumenten: veldnaam, pad.</summary>
    FileNotFound,

    /// <summary>Het hoofdprogramma heeft een ongeldig pad (absoluut of met ".."). Argumenten: pad.</summary>
    MainExecutableInvalid,

    /// <summary>Het hoofdprogramma staat niet in de map met bronbestanden. Argumenten: pad.</summary>
    MainExecutableNotFound,

    /// <summary>Er is een snelkoppeling gevraagd maar geen hoofdprogramma gekozen, dus er komen geen
    /// snelkoppelingen. Argumenten: geen.</summary>
    ShortcutsWithoutMainExecutable,

    /// <summary>De pagina Select Components staat aan, maar componenten worden nog niet gegenereerd.
    /// Argumenten: geen.</summary>
    ComponentsPageNotSupported,

    /// <summary>De pagina Select Tasks staat aan, maar er is geen taak om te tonen. De pagina
    /// verschijnt dan niet. Argumenten: geen.</summary>
    TasksPageWithoutTasks,

    /// <summary>De pagina Select Tasks staat uit, maar de bureaubladtaak laat Setup de pagina toch
    /// tonen. Argumenten: geen.</summary>
    TasksPageShownForDesktopIcon,

    /// <summary>Een taal-id uit het project komt niet voor in de taalcatalogus en is overgeslagen.
    /// Argumenten: taal-id.</summary>
    UnknownLanguage,

    /// <summary>Er is geen bestandsnaam voor de installer ingevuld; de standaardnaam wordt gebruikt.
    /// Argumenten: de naam die in het script komt (zonder .exe).</summary>
    OutputBaseFilenameDefaulted,

    /// <summary>Op knoppen is een tekstkleur ingesteld. Setup tekent zijn knoppen met de themakleur,
    /// dus de kleur wordt niet gegenereerd. Argumenten: aantal knoppen met een tekstkleur.</summary>
    ButtonTextColorNotSupported,

    /// <summary>Een scherm met eigen knopinstellingen staat uit in het project, dus de instellingen
    /// worden niet gebruikt. Argumenten: veldnaam van de instellingen, bijvoorbeeld
    /// <c>LicenseScreenButtons</c>.</summary>
    ButtonSettingsForHiddenScreen,

    /// <summary>De Volgende-knop is op een getoond scherm uitgeschakeld of verborgen. Het script zet
    /// hem nergens weer aan, dus de gebruiker kan niet verder. Argumenten: veldnaam van het scherm.</summary>
    NextButtonUnusable,
}

/// <summary>Eén melding van de generator, zie <see cref="GenerationIssueCode"/>.</summary>
/// <param name="Severity">Hoe zwaar de melding weegt.</param>
/// <param name="Code">Welke soort melding het is.</param>
/// <param name="Arguments">Waarden die in de tekst van de melding horen, in de volgorde die bij
/// <paramref name="Code"/> staat beschreven.</param>
public sealed record GenerationIssue(GenerationSeverity Severity, GenerationIssueCode Code, IReadOnlyList<string> Arguments)
{
    public GenerationIssue(GenerationSeverity severity, GenerationIssueCode code, params string[] arguments)
        : this(severity, code, (IReadOnlyList<string>)arguments)
    {
    }
}
