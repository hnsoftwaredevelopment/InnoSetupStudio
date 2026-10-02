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
    /// Aanpassingen van de schermspecifieke "Bladeren"-knop op de bestemmingspagina (Inno Setup's
    /// WizardForm.DirBrowseButton). Zie <see cref="BrowseButtonSettings"/> voor waarom dit een
    /// apart model is, los van <see cref="SelectDestinationScreenButtons"/>.
    /// </summary>
    public BrowseButtonSettings SelectDestinationBrowseButton { get; set; } = new();

    /// <summary>
    /// Pad naar de afbeelding die over de volledige hoogte links op de Welkomst- en
    /// Voltooid-pagina's staat, Inno Setup's <c>WizardImageFile</c>-richtlijn. Leeg betekent: nog
    /// niet aangepast door de gebruiker. De schermeditor toont in dat geval een meegeleverde
    /// standaardafbeelding (zie WizardImageResolver), maar dit veld blijft leeg totdat de
    /// gebruiker op het Standaardscherm in de schermeditor echt een eigen bestand kiest (vóór
    /// backlogitem 1, sectie 14: dat was de projectinstellingen).
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
    /// drielaags-resolutie uit §12.6/§12.7 van de architectuurdoc: eigen waarde op het scherm →
    /// deze standaardwaarde → Inno Setup's eigen ingebouwde standaard. Ingesteld via het
    /// Standaardscherm in de schermeditor (fase 4); dat is geen echt installerscherm, dus dit veld
    /// heeft geen tegenhanger in <see cref="WizardScreenSelection"/> en de eindgebruiker ziet het
    /// nooit als aparte pagina.
    /// </summary>
    public WizardScreenButtonSettings DefaultScreenButtons { get; set; } = new();

    /// <summary>
    /// Welke talen deze installer aanbiedt tijdens de installatie, als taal-id's uit
    /// <see cref="InnoLanguageCatalog"/>. Bevat altijd minstens <see cref="InnoLanguageCatalog.EnglishId"/>:
    /// Inno Setup toont die taal ook zonder eigen [Languages]-sectie, dus een installer die verder
    /// niets kiest is impliciet Engelstalig. Meer dan één taal betekent dat de installer meertalig
    /// is — bewust geen apart vlaggetje daarvoor, dat zou een tegenstrijdige status met deze lijst
    /// kunnen opleveren. Zie backlogitem 4, sectie 14 van de architectuurdoc: bewust vóór een
    /// mogelijke tekst-per-taal-uitbreiding op de knop-Captions gebouwd, zodat dat werk niet twee
    /// keer gedaan hoeft te worden. Alleen "welke talen" (fase 4-scope); hóe teksten per taal
    /// worden ingevoerd, en het schrijven van de [Languages]-sectie zelf (generator, fase 5/6),
    /// volgen later.
    /// </summary>
    public List<string> SupportedLanguageIds { get; set; } = new() { InnoLanguageCatalog.EnglishId };

    // Overige instellingen (backlogitem 3, sectie 25): instellingen die niet bij één specifiek
    // wizardscherm horen, verzameld in een eigen tabblad in ProjectSettingsWindow. Herbert gaf als
    // referentie een aantal schermafbeeldingen van Inno Script Studio (Kymoto Solutions) door, dat
    // tientallen van dit soort instellingen in een boomstructuur (Appearance, Program Group,
    // Uninstall Settings, enzovoort) aanbiedt. Voor nu dus bewust alleen de drie die Herbert zelf
    // noemde (bureaublad-snelkoppeling, startmenu, update capability); de rest komt later, mogelijk
    // als eigen tabbladen (Herbert, 2026-10-01: "Er zijn er veel meer, misschien ook een aantal
    // voor een eigen tabblad, maar dat komt later allemaal wel").

    /// <summary>
    /// Biedt deze installer een optionele taak "Maak een snelkoppeling op het bureaublad" aan.
    /// Komt overeen met de desktopicon-taak uit HNSoftwareInstallerFramework's Shortcuts.iss
    /// (<c>CreateDesktopIcon == "yes"</c>): staat uit totdat de gebruiker het aanvinkt op de
    /// Aanvullende-taken-pagina. Standaard <see langword="false"/>, net als dat framework
    /// (<c>Flags: unchecked</c>) — een bureaubladpictogram is iets dat de eindgebruiker bewust
    /// kiest, niet iets dat de installer ongevraagd neerzet.
    /// </summary>
    public bool CreateDesktopIcon { get; set; }

    /// <summary>
    /// Bepaalt of de (toekomstige) generator een startmenu-snelkoppeling opneemt in de
    /// <c>[Icons]</c>-sectie. Dit is geen tegenhanger van Inno Setup's <c>AllowNoIcons</c>-
    /// richtlijn: die richtlijn voegt alleen een "Geen Start Menu-map aanmaken"-aanvinkvakje toe
    /// waarmee de eindgebruiker tíjdens de installatie zelf van snelkoppelingen kan afzien, terwijl
    /// dit veld een bouwtijd-keuze is die bepaalt of de snelkoppeling-entry er überhaupt komt.
    /// Standaard <see langword="true"/>: zowel Inno Setup zelf (zonder <c>AllowNoIcons</c>) als
    /// HNSoftwareInstallerFramework's Base.iss maken altijd een startmenu-snelkoppeling tenzij
    /// nadrukkelijk anders gekozen.
    /// </summary>
    public bool CreateStartMenuIcon { get; set; } = true;

    /// <summary>
    /// Onthoudt deze installer bij een update (een nieuwe versie over een al geïnstalleerde
    /// versie heen) de eerder gekozen installatiemap, in plaats van die opnieuw te vragen. Komt
    /// overeen met Inno Setup's <c>UsePreviousAppDir</c>-richtlijn, die ook zonder deze instelling
    /// al standaard "yes" is — dit veld maakt die keuze alleen zichtbaar en per project
    /// aanpasbaar.
    /// </summary>
    public bool UsePreviousAppDir { get; set; } = true;

    /// <summary>Zie <see cref="UsePreviousAppDir"/>, maar dan voor de startmenugroep (Inno Setup's
    /// <c>UsePreviousGroup</c>-richtlijn).</summary>
    public bool UsePreviousGroup { get; set; } = true;

    /// <summary>Zie <see cref="UsePreviousAppDir"/>, maar dan voor het gekozen installatietype
    /// (Inno Setup's <c>UsePreviousSetupType</c>-richtlijn, alleen relevant zodra het project
    /// meerdere Types/Components gebruikt — fase 5/6).</summary>
    public bool UsePreviousSetupType { get; set; } = true;

    /// <summary>Zie <see cref="UsePreviousAppDir"/>, maar dan voor de aangevinkte taken, zoals de
    /// <see cref="CreateDesktopIcon"/>-taak hierboven (Inno Setup's <c>UsePreviousTasks</c>-
    /// richtlijn).</summary>
    public bool UsePreviousTasks { get; set; } = true;

    /// <summary>Zie <see cref="UsePreviousAppDir"/>, maar dan voor de gekozen installertaal (Inno
    /// Setup's <c>UsePreviousLanguage</c>-richtlijn).</summary>
    public bool UsePreviousLanguage { get; set; } = true;

    /// <summary>Maakt een nieuw, leeg project met een vers gegenereerd AppId.</summary>
    public static InstallerProject CreateNew() => new()
    {
        AppId = $"{{{Guid.NewGuid().ToString().ToUpperInvariant()}}}",
    };
}
