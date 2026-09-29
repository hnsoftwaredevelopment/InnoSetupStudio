using CommunityToolkit.Mvvm.ComponentModel;

namespace InnoSetupStudio.App.ViewModels;

/// <summary>
/// Eén rij in het talenoverzicht: een aan/uit-vinkje voor één taal uit
/// <see cref="Core.Project.InnoLanguageCatalog"/> (backlogitem 4, sectie 14). <see cref="Id"/>
/// koppelt de rij terug naar <see cref="Core.Project.InstallerProject.SupportedLanguageIds"/>.
/// </summary>
public sealed partial class LanguageRow : ObservableObject
{
    public LanguageRow(string id, string displayName, bool isLocked, bool isSelected)
    {
        Id = id;
        DisplayName = displayName;
        IsLocked = isLocked;
        _isSelected = isSelected;
    }

    public string Id { get; }

    public string DisplayName { get; }

    /// <summary>True voor Engels: altijd aangevinkt en niet uit te zetten, Inno Setup toont die
    /// taal sowieso zonder eigen taalbestand (zie <see cref="Core.Project.InnoLanguageCatalog.EnglishId"/>).</summary>
    public bool IsLocked { get; }

    [ObservableProperty]
    private bool _isSelected;
}
