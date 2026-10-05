namespace InnoSetupStudio.Core.Project;

/// <summary>
/// Voor welke Windows-architectuur de installer zijn programma installeert. De generator (fase 5)
/// vertaalt <see cref="X64"/> naar Inno Setup's <c>ArchitecturesInstallIn64BitMode</c>-richtlijn;
/// bij <see cref="X86"/> laat de generator die richtlijn weg, zodat Setup in 32-bit modus draait.
/// De exacte identifier in het .iss (bijvoorbeeld <c>x64compatible</c> of <c>x64</c>) wordt in de
/// generator vastgelegd en met ISCC gecontroleerd, niet in dit model.
/// </summary>
public enum InstallerArchitecture
{
    /// <summary>32-bit programma (Setup draait in 32-bit modus).</summary>
    X86,

    /// <summary>64-bit programma (Setup installeert in 64-bit modus).</summary>
    X64,
}
