namespace InnoSetupStudio.Core.Project;

/// <summary>
/// Aanpassingen van de Terug-/Volgende-/Annuleren-knop voor één wizardscherm. In Inno Setup zijn
/// dit geen [Setup]-richtlijnen maar Pascal Script-eigenschappen op WizardForm.BackButton/
/// NextButton/CancelButton (TNewButton-objecten), meestal ingesteld vanuit een
/// CurPageChanged-event-handler. De generator (fase 5/6) zet een ingevulde instantie om naar zo'n
/// event-handler; dit model bevat alleen de gegevens, geen gegenereerde code.
///
/// Een lege Caption betekent: Inno Setup's eigen standaardtekst voor deze knop op dit scherm
/// blijft ongewijzigd (bijvoorbeeld "Next &gt;" op de meeste schermen, "Install" op de
/// Klaar-om-te-installeren-pagina). Een null Enabled/Visible betekent hetzelfde voor het
/// ingeschakeld/zichtbaar-gedrag: Inno Setup's eigen logica (bijvoorbeeld dat Terug op het eerste
/// scherm vanzelf uitstaat) blijft dan intact. Alleen expliciet true/false overschrijft dat.
///
/// TextColor/FontFamily/FontSize/FontBold (backlogitem uit sectie 14 van de architectuurdoc)
/// volgen dezelfde leeg-is-onveranderd-conventie als Caption: een lege string/null laat Inno
/// Setup's eigen knopuiterlijk intact. TextColor is hex-tekst ("#RRGGBB" of "#AARRGGBB", zoals
/// WPF's eigen ColorConverter accepteert) in plaats van een eigen kleurtype, zodat dit model — net
/// als de rest van dit bestand — geen WPF-afhankelijkheid nodig heeft (InnoSetupStudio.Core kent
/// geen System.Windows). De generator (fase 5/6) zal deze velden, net als Caption, moeten omzetten
/// naar Pascal Script op TNewButton: Font.Color/Font.Name/Font.Size/Font.Style zijn gewone
/// TFont-eigenschappen die op een standaardknop direct werken.
///
/// Achtergrondkleur en een bitmap op de knop zijn bewust NIET opgenomen: TNewButton wordt door
/// Windows' eigen thema-engine getekend, dus een achtergrondkleur of bitmap zetten vereist het
/// uitschakelen van de Windows-thematisering en een zelf-getekende knop (OnPaint-achtig) in Pascal
/// Script — vergelijkbare extra generatorwerk voor beide, en niet iets wat Inno Setup's
/// standaardknop native ondersteunt. Herbert heeft dit expliciet geschrapt (2026-09-04). Diezelfde
/// afweging gold niet voor Tooltip hieronder (backlogitem 3, sectie 17): TNewButton is een gewone
/// TControl-afstammeling, dus Hint/ShowHint werken net zo rechtstreeks als de Font-eigenschappen.
/// </summary>
public sealed class WizardScreenButtonSettings
{
    public string BackButtonCaption { get; set; } = string.Empty;

    public bool? BackButtonEnabled { get; set; }

    public bool? BackButtonVisible { get; set; }

    public string BackButtonTextColor { get; set; } = string.Empty;

    public string BackButtonFontFamily { get; set; } = string.Empty;

    public int? BackButtonFontSize { get; set; }

    public bool? BackButtonFontBold { get; set; }

    public string BackButtonTooltip { get; set; } = string.Empty;

    public string NextButtonCaption { get; set; } = string.Empty;

    public bool? NextButtonEnabled { get; set; }

    public bool? NextButtonVisible { get; set; }

    public string NextButtonTextColor { get; set; } = string.Empty;

    public string NextButtonFontFamily { get; set; } = string.Empty;

    public int? NextButtonFontSize { get; set; }

    public bool? NextButtonFontBold { get; set; }

    public string NextButtonTooltip { get; set; } = string.Empty;

    public string CancelButtonCaption { get; set; } = string.Empty;

    public bool? CancelButtonEnabled { get; set; }

    public bool? CancelButtonVisible { get; set; }

    public string CancelButtonTextColor { get; set; } = string.Empty;

    public string CancelButtonFontFamily { get; set; } = string.Empty;

    public int? CancelButtonFontSize { get; set; }

    public bool? CancelButtonFontBold { get; set; }

    public string CancelButtonTooltip { get; set; } = string.Empty;

    /// <summary>
    /// Vertaling per taal van <see cref="BackButtonCaption"/>, voor projecten met meer dan één
    /// geselecteerde taal (InstallerProject.SupportedLanguageIds.Count &gt; 1; backlogitem
    /// "meertalige knopteksten", sectie 14). Sleutel is een taal-id uit InnoLanguageCatalog
    /// (bijvoorbeeld "dutch"), nooit InnoLanguageCatalog.EnglishId: Engels gebruikt gewoon
    /// BackButtonCaption hierboven, dat blijft de universele terugvalwaarde voor elke taal zonder
    /// eigen vertaling hier — zelfde terugval als Inno Setup's eigen CustomMessage()-mechanisme:
    /// ontbreekt een taalspecifieke [CustomMessages]-regel, dan valt Inno Setup terug op de EERSTE
    /// taal in [Languages], en dat is in deze app altijd Engels (zie
    /// InnoLanguageCatalog.EnglishId/InstallerProject.SupportedLanguageIds). Een lege waarde (of
    /// een ontbrekende sleutel) betekent: deze taal gebruikt ook gewoon BackButtonCaption.
    ///
    /// Alleen bedoeld voor overschrijvingen van de Terug-knop OP DIT SCHERM; er is bewust geen
    /// cascade via het Standaardscherm zoals bij BackButtonCaption zelf — een vertaling die voor
    /// alle schermen moet gelden, moet dus op elk scherm apart ingevuld worden. Dat is een bewuste
    /// vereenvoudiging ten opzichte van de drielaagse Effective*-resolutie: zonder dit zou elke
    /// taal ook zijn eigen Standaardscherm-laag nodig hebben, wat de omvang van deze eerste versie
    /// flink vergroot. Kan later alsnog toegevoegd worden als Herbert daar behoefte aan heeft.
    ///
    /// Backward-compatible: een ouder .issproj-bestand zonder dit veld deserialiseert gewoon naar
    /// een lege dictionary (System.Text.Json roept de parameterloze constructor aan en laat een
    /// ontbrekende JSON-sleutel bij de hier getoonde standaardwaarde), dus bestaande eentalige
    /// projecten hebben geen migratie nodig.
    /// </summary>
    public Dictionary<string, string> BackButtonCaptionByLanguage { get; set; } = new();

    /// <summary>Zie <see cref="BackButtonCaptionByLanguage"/>, maar dan voor <see cref="BackButtonTooltip"/>.</summary>
    public Dictionary<string, string> BackButtonTooltipByLanguage { get; set; } = new();

    /// <summary>Zie <see cref="BackButtonCaptionByLanguage"/>, maar dan voor de Volgende-knop.</summary>
    public Dictionary<string, string> NextButtonCaptionByLanguage { get; set; } = new();

    /// <summary>Zie <see cref="BackButtonTooltipByLanguage"/>, maar dan voor de Volgende-knop.</summary>
    public Dictionary<string, string> NextButtonTooltipByLanguage { get; set; } = new();

    /// <summary>Zie <see cref="BackButtonCaptionByLanguage"/>, maar dan voor de Annuleren-knop.</summary>
    public Dictionary<string, string> CancelButtonCaptionByLanguage { get; set; } = new();

    /// <summary>Zie <see cref="BackButtonTooltipByLanguage"/>, maar dan voor de Annuleren-knop.</summary>
    public Dictionary<string, string> CancelButtonTooltipByLanguage { get; set; } = new();
}
