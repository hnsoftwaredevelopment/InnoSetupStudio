using CommunityToolkit.Mvvm.ComponentModel;

namespace InnoSetupStudio.App.ViewModels;

/// <summary>
/// Eén rij in het wizardschermen-overzicht: een aan/uit-vinkje voor één standaard Inno
/// Setup-scherm, met een icoon en vertaalde naam voor herkenning. <see cref="Id"/> koppelt de
/// rij terug naar de bijbehorende eigenschap op <see cref="Core.Project.WizardScreenSelection"/>.
/// </summary>
public sealed partial class WizardScreenRow : ObservableObject
{
    public WizardScreenRow(string id, string displayName, string iconKey, bool isEnabled)
    {
        Id = id;
        DisplayName = displayName;
        IconKey = iconKey;
        _isEnabled = isEnabled;
    }

    public string Id { get; }

    public string DisplayName { get; }

    public string IconKey { get; }

    [ObservableProperty]
    private bool _isEnabled;
}
