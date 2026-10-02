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
    public SelectProgramGroupPageEditorViewModel(
        string defaultGroupName,
        bool appendDefaultGroupName,
        bool alwaysUsePersonalGroup)
        : base("ShowSelectProgramGroupPage", LocalizationManager.Instance["WizardScreenSelectProgramGroup"], "Folder")
    {
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
}
