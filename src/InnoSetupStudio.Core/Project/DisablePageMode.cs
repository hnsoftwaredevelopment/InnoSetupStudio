namespace InnoSetupStudio.Core.Project;

/// <summary>
/// Hoe Inno Setup een wizardpagina behandelt die een standaardwaarde aanbiedt met de optie om die
/// te wijzigen: de Installatiemap-pagina (Select Destination Location) en de Start Menu-map-pagina
/// (Select Start Menu Folder). Komt overeen met Inno Setup's eigen drie-waardige
/// <c>DisableDirPage</c>/<c>DisableProgramGroupPage</c>-richtlijnen (geverifieerd via de officiele
/// Inno Setup-documentatie, 2026-10-02, n.a.v. Herberts verzoek om een "Auto"-optie: bij een
/// update, als Setup bij het opstarten in het register ziet dat dezelfde applicatie al
/// geinstalleerd is, hoeft deze pagina niet meer getoond te worden). Beide richtlijnen
/// ondersteunen <c>no</c>/<c>yes</c>/<c>auto</c>, maar met een verschillende standaardwaarde per
/// richtlijn — zie <see cref="InstallerProject.DirPageMode"/> (standaard <see cref="AlwaysShow"/>,
/// Inno Setup's eigen standaard voor <c>DisableDirPage</c> is <c>no</c>) versus
/// <see cref="InstallerProject.GroupPageMode"/> (standaard <see cref="AutoSkipIfKnown"/>, Inno
/// Setup's eigen standaard voor <c>DisableProgramGroupPage</c> is al <c>auto</c>).
/// </summary>
public enum DisablePageMode
{
    /// <summary>Pagina altijd tonen; de gebruiker mag de voorgestelde waarde wijzigen. Komt
    /// overeen met <c>no</c>.</summary>
    AlwaysShow,

    /// <summary>Pagina nooit tonen; de voorgestelde waarde ligt vast voor de hele installatie.
    /// Komt overeen met <c>yes</c>.</summary>
    NeverShow,

    /// <summary>
    /// Pagina overslaan zodra Setup bij het opstarten in het register detecteert dat dezelfde
    /// applicatie al geinstalleerd is (dus bij een update, met de eerder gekozen map als vaste
    /// waarde) — bij een eerste installatie gewoon tonen, net als <see cref="AlwaysShow"/>. Komt
    /// overeen met <c>auto</c>. Dit gedrag hangt af van de installatie zelf (runtime), dus de
    /// voorvertoning in de schermeditor kan dit niet daadwerkelijk simuleren: die toont deze
    /// instelling als een eerste installatie (dus bewerkbaar), met een eigen toelichtende tekst
    /// in plaats van de "nooit bewerkbaar"-waarschuwing van <see cref="NeverShow"/>.
    /// </summary>
    AutoSkipIfKnown,
}
