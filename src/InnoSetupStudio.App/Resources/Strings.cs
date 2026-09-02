using System.Resources;

namespace InnoSetupStudio.App.Resources;

/// <summary>
/// Lichte, handgeschreven wrapper rond de ResX-vertalingen in dit project.
/// Bewust geen Visual Studio-gegenereerde Designer-klasse: de "PublicResXFileCodeGenerator"
/// custom tool draait alleen binnen de VS-IDE, niet tijdens `dotnet build`/CSC. De .resx-bestanden
/// (Strings.resx = Nederlands/standaard, Strings.en-US.resx, Strings.de-DE.resx) worden door de
/// .NET SDK automatisch als embedded/satellite resources meegenomen; deze klasse ontsluit ze
/// alleen via een ResourceManager.
/// </summary>
public static class Strings
{
    private static readonly ResourceManager ResourceManagerInstance =
        new("InnoSetupStudio.App.Resources.Strings", typeof(Strings).Assembly);

    public static ResourceManager ResourceManager => ResourceManagerInstance;
}
