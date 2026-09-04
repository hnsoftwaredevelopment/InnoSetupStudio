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

    /// <summary>
    /// Pad naar de afbeelding die over de volledige hoogte links op de Welkomst- en
    /// Voltooid-pagina's staat, Inno Setup's <c>WizardImageFile</c>-richtlijn. Leeg betekent: nog
    /// niet aangepast door de gebruiker. De schermeditor toont in dat geval een meegeleverde
    /// standaardafbeelding (zie WizardImageResolver), maar dit veld blijft leeg totdat de
    /// gebruiker in de projectinstellingen echt een eigen bestand kiest.
    /// </summary>
    public string WizardImageFile { get; set; } = string.Empty;

    /// <summary>
    /// Pad naar de kleine afbeelding rechtsboven op de overige wizardpagina's, Inno Setup's
    /// <c>WizardSmallImageFile</c>-richtlijn. Zelfde leeg-betekent-nog-niet-aangepast-gedrag als
    /// <see cref="WizardImageFile"/>.
    /// </summary>
    public string WizardSmallImageFile { get; set; } = string.Empty;

    /// <summary>
    /// Aanpassingen van de Terug-/Volgende-/Annuleren-knop op de Welkomstpagina. Zie
    /// <see cref="WizardScreenButtonSettings"/>. Alleen schermen waarvoor al een editor bestaat
    /// (fase 4) hebben zo'n eigenschap; de overige acht standaardschermen krijgen er één zodra hun
    /// editor gebouwd wordt.
    /// </summary>
    public WizardScreenButtonSettings WelcomeScreenButtons { get; set; } = new();

    /// <summary>Zie <see cref="WelcomeScreenButtons"/>, maar dan voor de licentiepagina.</summary>
    public WizardScreenButtonSettings LicenseScreenButtons { get; set; } = new();

    /// <summary>Zie <see cref="WelcomeScreenButtons"/>, maar dan voor de bestemmingspagina.</summary>
    public WizardScreenButtonSettings SelectDestinationScreenButtons { get; set; } = new();

    /// <summary>
    /// Standaardwaarden voor de Terug-/Volgende-/Annuleren-knop die elk scherm overneemt zolang
    /// het zelf niets voor een veld instelt (lege Caption / null Enabled of Visible) — de
    /// tweelaags-resolutie uit §12.6/§12.7 van de architectuurdoc: eigen waarde op het scherm →
    /// deze standaardwaarde → Inno Setup's eigen ingebouwde standaard. Ingesteld via het
    /// Standaardscherm in de schermeditor (fase 4); dat is geen echt installerscherm, dus dit veld
    /// heeft geen tegenhanger in <see cref="WizardScreenSelection"/> en de eindgebruiker ziet het
    /// nooit als aparte pagina.
    /// </summary>
    public WizardScreenButtonSettings DefaultScreenButtons { get; set; } = new();

    /// <summary>Maakt een nieuw, leeg project met een vers gegenereerd AppId.</summary>
    public static InstallerProject CreateNew() => new()
    {
        AppId = $"{{{Guid.NewGuid().ToString().ToUpperInvariant()}}}",
    };
}
