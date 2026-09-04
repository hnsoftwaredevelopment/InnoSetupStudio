namespace InnoSetupStudio.Core.Project;

/// <summary>
/// Aanpassingen van de "Bladeren"-knop op het Bestemmingspagina-scherm (Select Destination). In
/// Inno Setup is dit een schermspecifieke knop (WizardForm.DirBrowseButton, ook een TNewButton),
/// niet één van de drie gedeelde Terug-/Volgende-/Annuleren-knoppen — vandaar een eigen, apart
/// model in plaats van een uitbreiding van <see cref="WizardScreenButtonSettings"/>.
///
/// Bewust GEEN drielaags-resolutie via het Standaardscherm (zie WizardScreenEditorViewModel's
/// Effective*-eigenschappen): deze knop komt maar op één scherm voor, dus er is geen "ander
/// scherm" waarvan een standaardwaarde zinvol zou zijn. Bewust ook geen Caption: Herbert heeft
/// deze knop expliciet genoemd zonder Caption in de gewenste velden (2026-09-04) — alleen
/// Enabled/Visible/TextColor/Font.
///
/// Zelfde leeg/null-is-onveranderd-conventie als WizardScreenButtonSettings: een lege
/// TextColor/FontFamily of null Enabled/Visible/FontSize/FontBold laat Inno Setup's eigen
/// standaardgedrag/-uiterlijk voor deze knop intact.
/// </summary>
public sealed class BrowseButtonSettings
{
    public bool? Enabled { get; set; }

    public bool? Visible { get; set; }

    public string TextColor { get; set; } = string.Empty;

    public string FontFamily { get; set; } = string.Empty;

    public int? FontSize { get; set; }

    public bool? FontBold { get; set; }
}
