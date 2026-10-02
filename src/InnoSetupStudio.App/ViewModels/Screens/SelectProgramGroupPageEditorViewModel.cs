using CommunityToolkit.Mvvm.ComponentModel;
using InnoSetupStudio.App.Localization;

namespace InnoSetupStudio.App.ViewModels.Screens;

/// <summary>
/// Select Start Menu Folder-pagina (Inno Setup's eigen naam voor deze pagina; de interne
/// <c>WizardScreenSelection.ShowSelectProgramGroupPage</c>-vlag heet naar Inno Setup's
/// Pascal Script-kant "ProgramGroup", zie de toelichting daar). Net als de User Info-pagina geen
/// bestandskeuze of eigen knop: alleen een vooringevulde groepsnaam en twee vinkjes.
/// </summary>
public sealed partial class SelectProgramGroupPageEditorViewModel : WizardScreenEditorViewModel
{
    private readonly string _appName;

    public SelectProgramGroupPageEditorViewModel(
        string appName,
        string defaultGroupName,
        bool appendDefaultGroupName,
        bool alwaysUsePersonalGroup)
        : base("ShowSelectProgramGroupPage", LocalizationManager.Instance["WizardScreenSelectProgramGroup"], "Folder")
    {
        _appName = appName;
        _defaultGroupName = defaultGroupName;
        _appendDefaultGroupName = appendDefaultGroupName;
        _alwaysUsePersonalGroup = alwaysUsePersonalGroup;
    }

    [ObservableProperty]
    private string _defaultGroupName;

    [ObservableProperty]
    private bool _appendDefaultGroupName;

    [ObservableProperty]
    private bool _alwaysUsePersonalGroup;

    /// <summary>Wat de voorvertoning in het mapveld toont: de ingevulde groepsnaam, of anders
    /// Inno Setup's eigen terugvalwaarde (de toepassingsnaam, zie InstallerProject.DefaultGroupName)
    /// als voorbeeld — zelfde aanpak als SelectDestinationPageEditorViewModel.DisplayDirName
    /// (CodeRabbit, PR #23: een leeg veld op een nieuw project toonde hier niets, terwijl het
    /// model al een terugval naar AppName belooft).</summary>
    public string EffectiveGroupName => string.IsNullOrWhiteSpace(DefaultGroupName)
        ? (string.IsNullOrWhiteSpace(_appName) ? "App" : _appName)
        : DefaultGroupName;

    partial void OnDefaultGroupNameChanged(string value) => OnPropertyChanged(nameof(EffectiveGroupName));
}
