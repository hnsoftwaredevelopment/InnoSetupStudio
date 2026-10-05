namespace InnoSetupStudio.Core.Project;

/// <summary>
/// Eén taal die Inno Setup tijdens de installatie kan aanbieden: de taal-id zoals die in het
/// Name:-veld van de [Languages]-sectie van het gegenereerde .iss-bestand terechtkomt (generator,
/// fase 5, <c>IssGenerator</c>) terechtkomt, plus een leesbare naam voor de talenlijst in de UI.
/// </summary>
/// <param name="Id">Taal-id, ook de waarde van <c>Name:</c> in [Languages].</param>
/// <param name="DisplayName">Leesbare Engelse naam voor de talenlijst.</param>
/// <param name="IsBuiltIn">True voor Engels, dat Inno Setup ook zonder eigen taalbestand kent.</param>
/// <param name="MessagesFile">De waarde van <c>MessagesFile:</c> in [Languages], bijvoorbeeld
/// <c>compiler:Languages\Dutch.isl</c>. Met de bestandsnaam in de juiste hoofdletters, zoals die in
/// Inno Setup's Languages-map staat.</param>
public sealed record InnoLanguageOption(string Id, string DisplayName, bool IsBuiltIn, string MessagesFile);

/// <summary>
/// De vaste lijst talen die deze app aanbiedt bij het samenstellen van een meertalige installer
/// (backlogitem 4, sectie 14 van de architectuurdoc). Bewust een statische lijst in plaats van de
/// .isl-bestanden uit Herberts Languages-map in te lezen: dat zou een afhankelijkheid van waar
/// Inno Setup toevallig geïnstalleerd staat introduceren (pas relevant vanaf de build-integratie,
/// fase 7), en encoding-gedoe met de niet-Latijnse .isl-bestanden (Arabisch, Chinees, Thai,
/// enzovoort), terwijl de talenlijst zelf hetzelfde blijft. Dit zijn de 32 talen uit
/// Herberts kopie van Compiler\Languages, plus Engels (ingebouwd via compiler:Default.isl,
/// dus zonder eigen taalbestand). Elke <see cref="InnoLanguageOption.Id"/> is de exacte,
/// kleine-letters spelling die Inno Setup zelf in het Name:-veld van [Languages] verwacht
/// (bijvoorbeeld "german", "brazilianportuguese") — dezelfde spelling als de bestandsnaam van het
/// bijbehorende .isl-bestand.
/// </summary>
public static class InnoLanguageCatalog
{
    /// <summary>
    /// Taal-id van Engels. Altijd aanwezig in <see cref="Languages"/> en op
    /// <see cref="InstallerProject.SupportedLanguageIds"/>: Inno Setup toont deze taal ook zonder
    /// eigen [Languages]-sectie, dus een installer die verder niets kiest is impliciet Engelstalig.
    /// </summary>
    public const string EnglishId = "english";

    /// <summary>
    /// Engels eerst, daarna de overige 32 talen in de volgorde waarin Herberts kopie van de
    /// Languages-map ze bevat (alfabetisch op bestandsnaam).
    /// </summary>
    public static IReadOnlyList<InnoLanguageOption> Languages { get; } = BuildCatalog();

    private static IReadOnlyList<InnoLanguageOption> BuildCatalog()
    {
        // (Bestandsnaam van het .isl-bestand zonder extensie, leesbare naam). De leesbare naam is
        // steeds de Engelse naam van de taal, niet de vertaling in de taal zelf (het LanguageName=
        // veld in het .isl-bestand) — dat blijft zo leesbaar ongeacht welke UI-taal (nl/en/de)
        // Herbert net gekozen heeft, zonder 32 extra vertaalregels per resx-bestand voor iets dat
        // een eigennaam is.
        (string FileBaseName, string DisplayName)[] bundled =
        [
            ("Arabic", "Arabic"),
            ("Armenian", "Armenian"),
            ("BrazilianPortuguese", "Brazilian Portuguese"),
            ("Bulgarian", "Bulgarian"),
            ("Catalan", "Catalan"),
            ("ChineseSimplified", "Chinese Simplified"),
            ("ChineseTraditional", "Chinese Traditional"),
            ("Corsican", "Corsican"),
            ("Czech", "Czech"),
            ("Danish", "Danish"),
            ("Dutch", "Dutch"),
            ("Finnish", "Finnish"),
            ("French", "French"),
            ("German", "German"),
            ("Hebrew", "Hebrew"),
            ("Hungarian", "Hungarian"),
            ("Italian", "Italian"),
            ("Japanese", "Japanese"),
            ("Korean", "Korean"),
            ("Lithuanian", "Lithuanian"),
            ("Norwegian", "Norwegian"),
            ("Polish", "Polish"),
            ("Portuguese", "Portuguese"),
            ("Russian", "Russian"),
            ("Slovak", "Slovak"),
            ("Slovenian", "Slovenian"),
            ("Spanish", "Spanish"),
            ("Swedish", "Swedish"),
            ("Tamil", "Tamil"),
            ("Thai", "Thai"),
            ("Turkish", "Turkish"),
            ("Ukrainian", "Ukrainian"),
        ];

        var list = new List<InnoLanguageOption> { new(EnglishId, "English", IsBuiltIn: true, MessagesFile: "compiler:Default.isl") };
        list.AddRange(bundled.Select(l => new InnoLanguageOption(
            l.FileBaseName.ToLowerInvariant(),
            l.DisplayName,
            IsBuiltIn: false,
            MessagesFile: $"compiler:Languages\\{l.FileBaseName}.isl")));
        return list;
    }
}
