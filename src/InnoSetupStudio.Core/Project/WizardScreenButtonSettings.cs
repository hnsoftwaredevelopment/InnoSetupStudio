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
/// standaardknop native ondersteunt. Herbert heeft dit expliciet geschrapt (2026-09-04).
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

    public string NextButtonCaption { get; set; } = string.Empty;

    public bool? NextButtonEnabled { get; set; }

    public bool? NextButtonVisible { get; set; }

    public string NextButtonTextColor { get; set; } = string.Empty;

    public string NextButtonFontFamily { get; set; } = string.Empty;

    public int? NextButtonFontSize { get; set; }

    public bool? NextButtonFontBold { get; set; }

    public string CancelButtonCaption { get; set; } = string.Empty;

    public bool? CancelButtonEnabled { get; set; }

    public bool? CancelButtonVisible { get; set; }

    public string CancelButtonTextColor { get; set; } = string.Empty;

    public string CancelButtonFontFamily { get; set; } = string.Empty;

    public int? CancelButtonFontSize { get; set; }

    public bool? CancelButtonFontBold { get; set; }
}
