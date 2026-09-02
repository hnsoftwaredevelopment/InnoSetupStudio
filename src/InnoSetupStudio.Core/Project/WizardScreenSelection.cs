namespace InnoSetupStudio.Core.Project;

/// <summary>
/// Welke standaard Inno Setup-wizardschermen deze installer toont. Komt overeen met de
/// Disable*Page/-Info*File/UserInfoPage-richtlijnen uit de [Setup]-sectie; de daadwerkelijke
/// koppeling naar die richtlijnen (en naar bijbehorende inhoud zoals een licentiebestand) volgt
/// in een latere fase (generator, fase 5, en schermeditor, fase 4). Dit model bepaalt alleen of
/// een scherm in de wizard wordt opgenomen.
/// </summary>
public sealed class WizardScreenSelection
{
    /// <summary>Begroetingsscherm bij de start van de installatie.</summary>
    public bool ShowWelcomePage { get; set; } = true;

    /// <summary>Toont een licentieovereenkomst die de gebruiker moet accepteren.</summary>
    public bool ShowLicensePage { get; set; }

    /// <summary>Toont informatie vóór de installatie (bijvoorbeeld een leesmij-bestand).</summary>
    public bool ShowInfoBeforePage { get; set; }

    /// <summary>Vraagt naam en organisatie van de gebruiker op.</summary>
    public bool ShowUserInfoPage { get; set; }

    /// <summary>Laat de gebruiker de installatiemap kiezen of bevestigen.</summary>
    public bool ShowSelectDestinationPage { get; set; } = true;

    /// <summary>Laat de gebruiker optionele onderdelen (components) selecteren.</summary>
    public bool ShowSelectComponentsPage { get; set; }

    /// <summary>Laat de gebruiker de Startmenu-map kiezen of bevestigen.</summary>
    public bool ShowSelectProgramGroupPage { get; set; } = true;

    /// <summary>Laat de gebruiker optionele taken (tasks) selecteren.</summary>
    public bool ShowSelectTasksPage { get; set; }

    /// <summary>Toont een overzicht van de gekozen opties vlak voor de installatie start.</summary>
    public bool ShowReadyPage { get; set; } = true;

    /// <summary>Toont informatie ná de installatie (bijvoorbeeld release notes).</summary>
    public bool ShowInfoAfterPage { get; set; }

    /// <summary>Afsluitend scherm dat de installatie als voltooid meldt.</summary>
    public bool ShowFinishedPage { get; set; } = true;
}
