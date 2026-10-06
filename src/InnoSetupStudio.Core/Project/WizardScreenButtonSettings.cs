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
/// FontFamily/FontSize/FontBold (backlogitem uit sectie 14 van de architectuurdoc) volgen dezelfde
/// leeg-is-onveranderd-conventie als Caption: een lege string/null laat Inno Setup's eigen
/// knopuiterlijk intact. De generator zet deze velden, net als Caption, om naar Pascal Script op
/// TNewButton: Font.Name/Font.Size/Font.Style zijn gewone TFont-eigenschappen die op een
/// standaardknop direct werken.
///
/// Tekstkleur, achtergrondkleur en een bitmap op de knop zijn bewust NIET opgenomen: TNewButton wordt
/// door Windows' eigen thema-engine getekend, dus Font.Color heeft geen effect (gemeten in Inno Setup
/// 7.1.0, in modern en classic) en een achtergrondkleur of bitmap vereist een zelf-getekende knop,
/// wat niet iets is wat Inno Setup's standaardknop native ondersteunt. De achtergrondkleur is op
/// 2026-09-04 geschrapt, de tekstkleur op 2026-10-06 (Herbert). Oudere .issproj-bestanden met een
/// tekstkleur blijven openen: de JSON-lezer negeert onbekende velden. Diezelfde
/// afweging gold niet voor Tooltip hieronder (backlogitem 3, sectie 17): TNewButton is een gewone
/// TControl-afstammeling, dus Hint/ShowHint werken net zo rechtstreeks als de Font-eigenschappen.
/// </summary>
public sealed class WizardScreenButtonSettings
{
    public string BackButtonCaption { get; set; } = string.Empty;

    public bool? BackButtonEnabled { get; set; }

    public bool? BackButtonVisible { get; set; }

    public string BackButtonFontFamily { get; set; } = string.Empty;

    public int? BackButtonFontSize { get; set; }

    public bool? BackButtonFontBold { get; set; }

    public string BackButtonTooltip { get; set; } = string.Empty;

    public string NextButtonCaption { get; set; } = string.Empty;

    public bool? NextButtonEnabled { get; set; }

    public bool? NextButtonVisible { get; set; }

    public string NextButtonFontFamily { get; set; } = string.Empty;

    public int? NextButtonFontSize { get; set; }

    public bool? NextButtonFontBold { get; set; }

    public string NextButtonTooltip { get; set; } = string.Empty;

    public string CancelButtonCaption { get; set; } = string.Empty;

    public bool? CancelButtonEnabled { get; set; }

    public bool? CancelButtonVisible { get; set; }

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
    /// Cascade (docs/Ontwerp-Vertalingen-Standaardscherm.md): per taal wint eerst de vertaling van
    /// dit scherm, dan de eigen (universele) tekst van dit scherm, dan de vertaling van het
    /// Standaardscherm. ButtonSettingsResolver.ResolveTranslation bevat die regel.
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
