using System.Text.Json.Serialization;

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

    /// <summary>
    /// Het hoofdprogramma van de applicatie, als pad relatief aan <see cref="SourceFilesPath"/>
    /// (bijvoorbeeld <c>MijnApp.exe</c>). De generator (fase 5) gebruikt dit voor de
    /// snelkoppelingen in de <c>[Icons]</c>-sectie en later voor "programma starten na
    /// installatie". Leeg betekent: er is geen hoofdprogramma gekozen, en de generator maakt dan
    /// geen snelkoppelingen. Nieuw sinds 2026-10-05 (ontwerp dunne generator, stap 1).
    /// </summary>
    public string MainExecutable { get; set; } = string.Empty;

    /// <summary>
    /// Bestandsnaam van de gegenereerde installer zonder extensie, Inno Setup's
    /// <c>OutputBaseFilename</c>-richtlijn. Leeg betekent: <see cref="GetEffectiveOutputBaseFilename"/>
    /// geeft <c>&lt;AppName&gt;-&lt;AppVersion&gt;-Setup</c>, in plaats van Inno Setup's eigen
    /// standaard <c>setup</c>.
    /// </summary>
    public string OutputBaseFilename { get; set; } = string.Empty;

    /// <summary>
    /// Voor welke architectuur het hoofdprogramma is gebouwd, zie <see cref="InstallerArchitecture"/>.
    /// Standaard <see cref="InstallerArchitecture.X64"/>, ook voor een ouder projectbestand zonder
    /// deze sleutel: er is nog geen generator, dus er verandert niets aan bestaande installers.
    /// </summary>
    [JsonConverter(typeof(StrictEnumJsonConverter<InstallerArchitecture>))]
    public InstallerArchitecture Architecture { get; set; } = InstallerArchitecture.X64;

    /// <summary>
    /// Uiterlijk van de wizard, Inno Setup's <c>WizardStyle</c>-richtlijn. Standaard
    /// <see cref="InstallerWizardStyle.Modern"/>, zie <see cref="InstallerWizardStyle"/>.
    /// </summary>
    [JsonConverter(typeof(StrictEnumJsonConverter<InstallerWizardStyle>))]
    public InstallerWizardStyle WizardStyle { get; set; } = InstallerWizardStyle.Modern;

    /// <summary>Welke standaard wizardschermen deze installer toont (fase 3).</summary>
    public WizardScreenSelection WizardScreens { get; set; } = new();

    /// <summary>
    /// Pad naar het licentiebestand (.txt of .rtf) dat op de licentiepagina wordt getoond, alleen
    /// relevant zolang <see cref="WizardScreenSelection.ShowLicensePage"/> aan staat. Leeg totdat
    /// de gebruiker in de schermeditor (fase 4) een bestand kiest.
    /// </summary>
    public string LicenseFilePath { get; set; } = string.Empty;

    /// <summary>
    /// Pad naar het leesmij-/infobestand (.txt of .rtf) dat vóór de bestemmingspagina wordt
    /// getoond, Inno Setup's <c>InfoBeforeFile</c>-richtlijn. Alleen relevant zolang
    /// <see cref="WizardScreenSelection.ShowInfoBeforePage"/> aan staat. Net als
    /// <see cref="LicenseFilePath"/>: leeg totdat de gebruiker in de schermeditor (fase 4) een
    /// bestand kiest, en Inno Setup toont de Info Before-pagina alleen als dit veld een bestand
    /// bevat (zie categorie 2 van de Feature-Checklist: "(InfoBeforeFile aanwezig)").
    /// </summary>
    public string InfoBeforeFilePath { get; set; } = string.Empty;

    /// <summary>Zie <see cref="InfoBeforeFilePath"/>, maar dan voor de Info After-pagina (Inno
    /// Setup's <c>InfoAfterFile</c>-richtlijn, na de bestemmingspagina en vóór Voltooid).</summary>
    public string InfoAfterFilePath { get; set; } = string.Empty;

    /// <summary>
    /// Standaard vooringevulde naam op de User Info-pagina, Inno Setup's
    /// <c>DefaultUserInfoName</c>-richtlijn. Leeg betekent: Inno Setup's eigen standaardgedrag
    /// (meestal de ingelogde Windows-gebruikersnaam) blijft gelden.
    /// </summary>
    public string DefaultUserInfoName { get; set; } = string.Empty;

    /// <summary>Zie <see cref="DefaultUserInfoName"/>, maar dan voor de organisatie (Inno Setup's
    /// <c>DefaultUserInfoOrg</c>-richtlijn).</summary>
    public string DefaultUserInfoOrg { get; set; } = string.Empty;

    /// <summary>Zie <see cref="DefaultUserInfoName"/>, maar dan voor het serienummer (Inno
    /// Setup's <c>DefaultUserInfoSerial</c>-richtlijn).</summary>
    public string DefaultUserInfoSerial { get; set; } = string.Empty;

    /// <summary>
    /// Onthoudt deze installer bij een update de eerder ingevulde naam/organisatie/serienummer,
    /// in plaats van de User Info-pagina leeg (of met de hierboven ingestelde standaardwaarden)
    /// opnieuw te tonen. Komt overeen met Inno Setup's <c>UsePreviousUserInfo</c>-richtlijn, die
    /// ook zonder deze instelling al standaard "yes" is — zelfde conventie als
    /// <see cref="UsePreviousAppDir"/> hieronder, hier gegroepeerd bij de User Info-velden
    /// zodat de schermeditor voor dit scherm alles bij elkaar heeft.
    /// </summary>
    public bool UsePreviousUserInfo { get; set; } = true;

    /// <summary>
    /// Standaard voorgestelde startmenugroep op de Select Start Menu Folder-pagina, Inno Setup's
    /// <c>DefaultGroupName</c>-richtlijn. Leeg betekent: de schermeditor en generator vallen
    /// terug op <see cref="AppName"/>, net als <see cref="DefaultDirName"/> hierboven voor de
    /// bestemmingspagina.
    /// </summary>
    public string DefaultGroupName { get; set; } = string.Empty;

    /// <summary>
    /// Stuurt specifiek Inno Setup's eigen Bladeren-dialoog op de Select Start Menu
    /// Folder-pagina (een boomweergave van bestaande startmenu-mappen, niet het tekstveld
    /// zelf): kiest de gebruiker daar bijvoorbeeld de bestaande map "Accessoires", dan plakt
    /// Inno Setup (bij <see langword="true"/>, de standaard) automatisch de laatste component
    /// van <see cref="DefaultGroupName"/> erachter (dus "Accessoires\AppName"). Bij
    /// <see langword="false"/> gebruikt Setup precies de gekozen map, en krijgt die
    /// Bladeren-dialoog er zelf een "Nieuwe map maken"-knop bij. Komt overeen met Inno Setup's
    /// <c>AppendDefaultGroupName</c>-richtlijn, standaard <see langword="true"/> net als Inno
    /// Setup zelf — geverifieerd via de officiële Inno Setup-documentatie (2026-10-02, n.a.v.
    /// Herberts vraag of dit niet gewoon ging om het combineren van een getypte naam met de
    /// standaardnaam: dat is het dus niet, het gaat specifiek om deze Bladeren-dialoog).
    /// </summary>
    public bool AppendDefaultGroupName { get; set; } = true;

    /// <summary>
    /// Laat Inno Setup's <c>{group}</c>-constante altijd naar het persoonlijke startmenu van de
    /// huidige gebruiker wijzen, ook als de installatie "voor alle gebruikers" is (normaal wijst
    /// <c>{group}</c> dan naar het Alle-gebruikers-startmenu). Komt overeen met Inno Setup's
    /// <c>AlwaysUsePersonalGroup</c>-richtlijn, standaard <see langword="false"/> net als Inno
    /// Setup zelf. Inno Setup's eigen documentatie waarschuwt dat dit "mogelijk niet het beoogde
    /// effect heeft" en de compiler geeft er een waarschuwing bij (tenzij
    /// <c>UsedUserAreasWarning</c> is uitgezet) — nog niet vertaald naar een eigen
    /// waarschuwing in deze IDE, generatorwerk voor fase 5/6.
    /// </summary>
    public bool AlwaysUsePersonalGroup { get; set; }

    /// <summary>
    /// Hoe de Start Menu-map-pagina (Select Start Menu Folder) zich gedraagt: altijd tonen
    /// (bewerkbaar), nooit tonen (vast), of automatisch overslaan bij een update (zie
    /// <see cref="DisablePageMode"/> voor de volledige toelichting). Komt overeen met Inno Setup's
    /// <c>DisableProgramGroupPage</c>-richtlijn. Standaard <see cref="DisablePageMode.AutoSkipIfKnown"/>
    /// — Inno Setup's eigen standaard voor <c>DisableProgramGroupPage</c> is namelijk ook <c>auto</c>
    /// (geverifieerd via de officiele documentatie, opnieuw op 2026-10-05). <see cref="DirPageMode"/>
    /// hierboven wijkt hier bewust van af. Nieuw veld, geen oudere JSON-sleutel om achterwaarts compatibel
    /// mee te blijven (in tegenstelling tot DirPageMode): dit scherm had nog geen eigen
    /// bewerkbaar-vinkje.
    /// </summary>
    [JsonConverter(typeof(DisablePageModeJsonConverter))]
    public DisablePageMode GroupPageMode { get; set; } = DisablePageMode.AutoSkipIfKnown;

    /// <summary>
    /// Aanpassingen van de schermspecifieke "Bladeren"-knop op de Select Start Menu
    /// Folder-pagina (Inno Setup's WizardForm.GroupBrowseButton, net als DirBrowseButton een
    /// TNewButton). Zie <see cref="BrowseButtonSettings"/> voor waarom dit een apart model is,
    /// los van <see cref="SelectProgramGroupScreenButtons"/> — hetzelfde model als
    /// <see cref="SelectDestinationBrowseButton"/>, hergebruikt in plaats van een tweede, bijna
    /// identieke klasse (Herberts verzoek, 2026-10-02: dezelfde bewerkingsmogelijkheden als de
    /// bestemmingspagina).
    /// </summary>
    public BrowseButtonSettings SelectProgramGroupBrowseButton { get; set; } = new();

    /// <summary>
    /// Verbergt de samenvattingstekst (memo) op de Klaar-om-te-installeren-pagina. Komt overeen
    /// met Inno Setup's <c>DisableReadyMemo</c>-richtlijn, standaard <see langword="false"/>
    /// (de samenvatting staat dus standaard aan) net als Inno Setup zelf.
    /// </summary>
    public bool DisableReadyMemo { get; set; }

    /// <summary>
    /// Toont de gekozen installatiemap altijd in de samenvatting op de
    /// Klaar-om-te-installeren-pagina, ook wanneer die pagina (<see cref="WizardScreenSelection.ShowSelectDestinationPage"/>)
    /// is overgeslagen. Komt overeen met Inno Setup's <c>AlwaysShowDirOnReadyPage</c>-richtlijn,
    /// standaard <see langword="false"/> net als Inno Setup zelf.
    /// </summary>
    public bool AlwaysShowDirOnReadyPage { get; set; }

    /// <summary>Zie <see cref="AlwaysShowDirOnReadyPage"/>, maar dan voor de startmenugroep
    /// (Inno Setup's <c>AlwaysShowGroupOnReadyPage</c>-richtlijn).</summary>
    public bool AlwaysShowGroupOnReadyPage { get; set; }

    /// <summary>
    /// Vaste installatiemap die op de bestemmingspagina wordt voorgesteld, in Inno Setup's eigen
    /// constanten-notatie (bijvoorbeeld <c>{autopf}\MijnApp</c>). Leeg betekent: de schermeditor
    /// en generator vallen terug op <c>{autopf}\AppName</c> op basis van <see cref="AppName"/>.
    /// </summary>
    public string DefaultDirName { get; set; } = string.Empty;

    /// <summary>
    /// Hoe de bestemmingspagina (Select Destination Location) zich gedraagt: altijd tonen
    /// (bewerkbaar), nooit tonen (vast), of automatisch overslaan bij een update (zie
    /// <see cref="DisablePageMode"/> voor de volledige toelichting). Komt overeen met Inno Setup's
    /// <c>DisableDirPage</c>-richtlijn. De projectstandaard blijft <see cref="DisablePageMode.AlwaysShow"/>
    /// om het oude <c>AllowUserToChangeDir=true</c>-gedrag te behouden. Dat wijkt af van Inno
    /// Setup's eigen standaard <c>auto</c>, die de pagina bij een bekende update kan overslaan.
    ///
    /// Was tot 2026-10-02 een <see langword="bool"/> onder dezelfde JSON-sleutel
    /// (<c>AllowUserToChangeDir</c>, <see langword="true"/> = bewerkbaar): de
    /// <c>[JsonPropertyName]</c> hieronder houdt die oude sleutelnaam aan zodat een bestaand
    /// projectbestand niet hoeft te worden aangepast, en <see cref="DisablePageModeJsonConverter"/>
    /// leest zowel die oude <c>true</c>/<c>false</c>-waarde als de nieuwe tekstwaarden (Herberts
    /// verzoek om een Auto-optie, net als bij <see cref="GroupPageMode"/> hieronder).
    /// </summary>
    [JsonPropertyName("AllowUserToChangeDir")]
    [JsonConverter(typeof(DisablePageModeJsonConverter))]
    public DisablePageMode DirPageMode { get; set; } = DisablePageMode.AlwaysShow;

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

    /// <summary>Zie <see cref="WelcomeScreenButtons"/>, maar dan voor de Info Before-pagina.</summary>
    public WizardScreenButtonSettings InfoBeforeScreenButtons { get; set; } = new();

    /// <summary>Zie <see cref="WelcomeScreenButtons"/>, maar dan voor de User Info-pagina.</summary>
    public WizardScreenButtonSettings UserInfoScreenButtons { get; set; } = new();

    /// <summary>Zie <see cref="WelcomeScreenButtons"/>, maar dan voor de bestemmingspagina.</summary>
    public WizardScreenButtonSettings SelectDestinationScreenButtons { get; set; } = new();

    /// <summary>Zie <see cref="WelcomeScreenButtons"/>, maar dan voor de Select Start Menu
    /// Folder-pagina.</summary>
    public WizardScreenButtonSettings SelectProgramGroupScreenButtons { get; set; } = new();

    /// <summary>Zie <see cref="WelcomeScreenButtons"/>, maar dan voor de
    /// Klaar-om-te-installeren-pagina.</summary>
    public WizardScreenButtonSettings ReadyScreenButtons { get; set; } = new();

    /// <summary>Zie <see cref="WelcomeScreenButtons"/>, maar dan voor de Info After-pagina.</summary>
    public WizardScreenButtonSettings InfoAfterScreenButtons { get; set; } = new();

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

    /// <summary>
    /// De bestandsnaam (zonder extensie) die de generator voor de installer gebruikt: de ingevulde
    /// <see cref="OutputBaseFilename"/>, of anders <c>&lt;AppName&gt;-&lt;AppVersion&gt;-Setup</c>.
    /// Tekens die Windows in een bestandsnaam niet toestaat worden vervangen door een
    /// onderstrepingsteken. Is ook de naam leeg, dan is het resultaat <c>Setup</c>; is alleen de
    /// versie leeg, dan <c>&lt;AppName&gt;-Setup</c>.
    /// </summary>
    public string GetEffectiveOutputBaseFilename()
    {
        var custom = OutputBaseFilename?.Trim();
        var name = !string.IsNullOrEmpty(custom)
            ? custom
            : string.Join("-", new[] { AppName?.Trim(), AppVersion?.Trim(), "Setup" }.Where(part => !string.IsNullOrEmpty(part)));

        var invalid = Path.GetInvalidFileNameChars();
        return new string(name.Select(c => Array.IndexOf(invalid, c) >= 0 ? '_' : c).ToArray());
    }

    /// <summary>Maakt een nieuw, leeg project met een vers gegenereerd AppId.</summary>
    public static InstallerProject CreateNew() => new()
    {
        AppId = $"{{{Guid.NewGuid().ToString().ToUpperInvariant()}}}",
    };
}
