using InnoSetupStudio.Core.Settings;

namespace InnoSetupStudio.Tests;

public class AppSettingsTests
{
    [Fact]
    public void DefaultsToLightThemeAndDutch()
    {
        var settings = new AppSettings();

        Assert.Equal("Light", settings.Theme);
        Assert.Equal("nl-NL", settings.Language);
    }
}
