namespace InnoSetupStudio.Core.Project;

/// <summary>
/// Uiterlijk van de installer-wizard, Inno Setup's <c>WizardStyle</c>-richtlijn. Inno Setup kent
/// meer stijlen en modifiers (onder andere dark, polar en slate); die horen niet bij deze eerste
/// versie. Inno Setup's eigen standaard is <see cref="Classic"/>, dit project kiest bewust
/// <see cref="Modern"/> als standaard, zoals het HNSoftwareInstallerFramework.
/// </summary>
public enum InstallerWizardStyle
{
    /// <summary>Het oorspronkelijke Inno Setup-uiterlijk.</summary>
    Classic,

    /// <summary>Het moderne Inno Setup-uiterlijk.</summary>
    Modern,
}
