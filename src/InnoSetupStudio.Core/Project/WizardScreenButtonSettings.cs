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
/// TextColor/BackgroundColor/BitmapFilePath (backlogitem uit sectie 14 van de architectuurdoc)
/// volgen dezelfde leeg-is-onveranderd-conventie als Caption: een lege string laat Inno Setup's
/// eigen knopuiterlijk intact. Kleuren zijn hex-tekst ("#RRGGBB" of "#AARRGGBB", zoals WPF's eigen
/// ColorConverter accepteert) in plaats van een eigen kleurtype, zodat dit model — net als de rest
/// van dit bestand — geen WPF-afhankelijkheid nodig heeft (InnoSetupStudio.Core kent geen
/// System.Windows). BitmapFilePath is, net als LicenseFilePath, een pad dat IProjectAssetService
/// naar de projectmap kopieert zodra het van elders komt. De generator (fase 5/6) zal deze drie
/// velden, net als Caption, moeten omzetten naar Pascal Script op TNewButton — Inno Setup's eigen
/// knopklasse ondersteunt Font.Color (tekstkleur) direct, maar geen achtergrondkleur of bitmap op
/// een standaardknop; die twee vereisen zelf-getekende knoppen in de generator, nog niet gebouwd.
/// </summary>
public sealed class WizardScreenButtonSettings
{
    public string BackButtonCaption { get; set; } = string.Empty;

    public bool? BackButtonEnabled { get; set; }

    public bool? BackButtonVisible { get; set; }

    public string BackButtonTextColor { get; set; } = string.Empty;

    public string BackButtonBackgroundColor { get; set; } = string.Empty;

    public string BackButtonBitmapFilePath { get; set; } = string.Empty;

    public string NextButtonCaption { get; set; } = string.Empty;

    public bool? NextButtonEnabled { get; set; }

    public bool? NextButtonVisible { get; set; }

    public string NextButtonTextColor { get; set; } = string.Empty;

    public string NextButtonBackgroundColor { get; set; } = string.Empty;

    public string NextButtonBitmapFilePath { get; set; } = string.Empty;

    public string CancelButtonCaption { get; set; } = string.Empty;

    public bool? CancelButtonEnabled { get; set; }

    public bool? CancelButtonVisible { get; set; }

    public string CancelButtonTextColor { get; set; } = string.Empty;

    public string CancelButtonBackgroundColor { get; set; } = string.Empty;

    public string CancelButtonBitmapFilePath { get; set; } = string.Empty;
}
