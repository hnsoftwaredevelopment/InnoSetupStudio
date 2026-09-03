namespace InnoSetupStudio.Core.Project;

/// <summary>
/// Algemene projectinformatie voor één installer: naam, ontwikkelaar, contactgegevens en de
/// bestandslocaties die de generator later nodig heeft (fase 5). Komt grotendeels overeen met
/// een deel van de Inno Setup [Setup]-sectie.
/// </summary>
public sealed class InstallerProject
{
    /// <summary>
    /// Vaste, unieke identificatie van de applicatie voor Inno Setup (AppId), gebruikt om bij
    /// een nieuwe installatie een eerdere installatie van dezelfde app te herkennen (upgrade in
    /// plaats van dubbele installatie). Wordt één keer gegenereerd bij het aanmaken van een
    /// nieuw project via <see cref="CreateNew"/> en blijft daarna ongewijzigd: een project met
    /// een gewijzigd AppId ziet Inno Setup als een compleet andere applicatie.
    /// </summary>
    public string AppId { get; set; } = string.Empty;

    public string AppName { get; set; } = string.Empty;

    public string AppVersion { get; set; } = string.Empty;

    public string Publisher { get; set; } = string.Empty;

    public string PublisherEmail { get; set; } = string.Empty;

    public string PublisherUrl { get; set; } = string.Empty;

    /// <summary>Map met de bronbestanden die de installer moet meenemen.</summary>
    public string SourceFilesPath { get; set; } = string.Empty;

    /// <summary>Map waarin het gecompileerde installer-bestand terechtkomt.</summary>
    public string OutputPath { get; set; } = string.Empty;

    /// <summary>Map met eigen afbeeldingen voor de installer (bijvoorbeeld een wizard-banner).</summary>
    public string CustomImagesPath { get; set; } = string.Empty;

    /// <summary>Pad naar het .ico-bestand dat als installer-icon wordt gebruikt.</summary>
    public string SetupIconFile { get; set; } = string.Empty;

    /// <summary>Welke standaard wizardschermen deze installer toont (fase 3).</summary>
    public WizardScreenSelection WizardScreens { get; set; } = new();

    /// <summary>
    /// Pad naar het licentiebestand (.txt of .rtf) dat op de licentiepagina wordt getoond, alleen
    /// relevant zolang <see cref="WizardScreenSelection.ShowLicensePage"/> aan staat. Leeg totdat
    /// de gebruiker in de schermeditor (fase 4) een bestand kiest.
    /// </summary>
    public string LicenseFilePath { get; set; } = string.Empty;

    /// <summary>
    /// Vaste installatiemap die op de bestemmingspagina wordt voorgesteld, in Inno Setup's eigen
    /// constanten-notatie (bijvoorbeeld <c>{autopf}\MijnApp</c>). Leeg betekent: de schermeditor
    /// en generator vallen terug op <c>{autopf}\AppName</c> op basis van <see cref="AppName"/>.
    /// </summary>
    public string DefaultDirName { get; set; } = string.Empty;

    /// <summary>
    /// Mag de gebruiker op de bestemmingspagina een andere map kiezen dan het voorstel, of ligt
    /// die vast. Komt overeen met Inno Setup's <c>DisableDirPage</c>-richtlijn (omgekeerd: hier
    /// betekent <see langword="true"/> dat de pagina bewerkbaar is, wat de standaard is).
    /// </summary>
    public bool AllowUserToChangeDir { get; set; } = true;

    /// <summary>Maakt een nieuw, leeg project met een vers gegenereerd AppId.</summary>
    public static InstallerProject CreateNew() => new()
    {
        AppId = $"{{{Guid.NewGuid().ToString().ToUpperInvariant()}}}",
    };
}
