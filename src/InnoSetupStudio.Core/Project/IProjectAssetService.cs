namespace InnoSetupStudio.Core.Project;

/// <summary>
/// Zorgt dat een bestand dat de gebruiker via een bestandsdialoog kiest (een licentiebestand, en
/// straks bijvoorbeeld een eigen achtergrondafbeelding) binnen de projectmap terechtkomt, zodat
/// het project zelf verplaatsbaar blijft: verplaats je de .issproj-map naar een andere schijf of
/// machine, dan gaat een verwijzing naar een bestand ergens anders op de oorspronkelijke schijf
/// stuk, terwijl een verwijzing naar een bestand in de eigen projectmap gewoon meeverhuist.
/// </summary>
public interface IProjectAssetService
{
    /// <summary>
    /// Als <paramref name="sourceFilePath"/> al binnen de map van <paramref name="projectFilePath"/>
    /// staat, wordt dat pad ongewijzigd teruggegeven. Staat het bestand ergens anders, dan wordt
    /// het gekopieerd naar een vaste submap naast het projectbestand en wordt het pad naar die
    /// kopie teruggegeven (bij een naamsbotsing met een eerdere kopie krijgt de nieuwe kopie een
    /// volgnummer, het bestaande bestand wordt nooit overschreven). Is
    /// <paramref name="projectFilePath"/> leeg (het project is nog niet opgeslagen, er is dus nog
    /// geen projectmap om naartoe te kopiëren), dan wordt <paramref name="sourceFilePath"/>
    /// ongewijzigd teruggegeven.
    /// </summary>
    string Import(string? projectFilePath, string sourceFilePath);
}
