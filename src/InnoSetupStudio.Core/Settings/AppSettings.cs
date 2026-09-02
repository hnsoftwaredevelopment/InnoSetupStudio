namespace InnoSetupStudio.Core.Settings;

/// <summary>Kale app-brede instellingen (taal/thema). Projectinhoud staat los hiervan.</summary>
public sealed class AppSettings
{
    public string Theme { get; set; } = "Light";

    public string Language { get; set; } = "nl-NL";
}
