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

    /// <summary>Maakt een nieuw, leeg project met een vers gegenereerd AppId.</summary>
    public static InstallerProject CreateNew() => new()
    {
        AppId = $"{{{Guid.NewGuid().ToString().ToUpperInvariant()}}}",
    };
}
