namespace InnoSetupStudio.Core.Project;

/// <summary>
/// Aanpassingen van de "Bladeren"-knop op het Bestemmingspagina-scherm (Select Destination). In
/// Inno Setup is dit een schermspecifieke knop (WizardForm.DirBrowseButton, ook een TNewButton),
/// niet één van de drie gedeelde Terug-/Volgende-/Annuleren-knoppen — vandaar een eigen, apart
/// model in plaats van een uitbreiding van <see cref="WizardScreenButtonSettings"/>.
///
/// Bewust GEEN drielaags-resolutie via het Standaardscherm (zie WizardScreenEditorViewModel's
/// Effective*-eigenschappen): deze knop komt maar op één scherm voor, dus er is geen "ander
/// scherm" waarvan een standaardwaarde zinvol zou zijn.
///
/// Caption toegevoegd op 2026-09-29: eerder (2026-09-04) had Herbert dit veld hier niet gevraagd,
/// en het codecommentaar ging er toen van uit dat DirBrowseButton geen Caption zou hebben. Via
/// webzoekopdracht geverifieerd dat dit niet klopt — in Inno Setup's TWizardForm-objectmodel is
/// DirBrowseButton, net als BackButton/NextButton/CancelButton, gedeclareerd als TNewButton, en
/// TNewButton heeft een Caption-property. Herbert heeft daarop gevraagd dit ook hier mogelijk te
/// maken, net zoals bij de drie gedeelde knoppen.
///
/// Zelfde leeg/null-is-onveranderd-conventie als WizardScreenButtonSettings: een lege
/// Caption/TextColor/FontFamily/Tooltip of null Enabled/Visible/FontSize/FontBold laat Inno
/// Setup's eigen standaardgedrag/-uiterlijk voor deze knop intact.
/// </summary>
public sealed class BrowseButtonSettings
{
    public string Caption { get; set; } = string.Empty;

    public bool? Enabled { get; set; }

    public bool? Visible { get; set; }

    public string TextColor { get; set; } = string.Empty;

    public string FontFamily { get; set; } = string.Empty;

    public int? FontSize { get; set; }

    public bool? FontBold { get; set; }

    public string Tooltip { get; set; } = string.Empty;

    /// <summary>Zie <see cref="WizardScreenButtonSettings.BackButtonCaptionByLanguage"/>: zelfde
    /// per-taal-vertaling van <see cref="Caption"/>, met Engels (via Caption hierboven) als
    /// universele terugvalwaarde voor elke taal zonder eigen vertaling hier.</summary>
    public Dictionary<string, string> CaptionByLanguage { get; set; } = new();

    /// <summary>Zie <see cref="CaptionByLanguage"/>, maar dan voor <see cref="Tooltip"/>.</summary>
    public Dictionary<string, string> TooltipByLanguage { get; set; } = new();
}
