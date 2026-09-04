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
/// </summary>
public sealed class WizardScreenButtonSettings
{
    public string BackButtonCaption { get; set; } = string.Empty;

    public bool? BackButtonEnabled { get; set; }

    public bool? BackButtonVisible { get; set; }

    public string NextButtonCaption { get; set; } = string.Empty;

    public bool? NextButtonEnabled { get; set; }

    public bool? NextButtonVisible { get; set; }

    public string CancelButtonCaption { get; set; } = string.Empty;

    public bool? CancelButtonEnabled { get; set; }

    public bool? CancelButtonVisible { get; set; }
}
