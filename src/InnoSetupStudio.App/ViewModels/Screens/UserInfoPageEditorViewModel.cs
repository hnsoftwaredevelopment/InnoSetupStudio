using CommunityToolkit.Mvvm.ComponentModel;
using InnoSetupStudio.App.Localization;

namespace InnoSetupStudio.App.ViewModels.Screens;

/// <summary>
/// User Info-pagina: vraagt de eindgebruiker om naam, organisatie en (optioneel) een
/// serienummer. Geen bestandskeuze of eigen knop zoals Licentie/Bestemming: alleen drie
/// vooringevulde tekstvelden en een "onthoud bij update"-vinkje, vergelijkbaar met
/// SelectProgramGroupPageEditorViewModel maar zonder de Bladeren-knop-infrastructuur die dat
/// scherm wél nodig heeft.
/// </summary>
public sealed partial class UserInfoPageEditorViewModel : WizardScreenEditorViewModel
{
    public UserInfoPageEditorViewModel(
        string defaultUserInfoName,
        string defaultUserInfoOrg,
        string defaultUserInfoSerial,
        bool usePreviousUserInfo)
        : base("ShowUserInfoPage", LocalizationManager.Instance["WizardScreenUserInfo"], "Document")
    {
        _defaultUserInfoName = defaultUserInfoName;
        _defaultUserInfoOrg = defaultUserInfoOrg;
        _defaultUserInfoSerial = defaultUserInfoSerial;
        _usePreviousUserInfo = usePreviousUserInfo;
    }

    [ObservableProperty]
    private string _defaultUserInfoName;

    [ObservableProperty]
    private string _defaultUserInfoOrg;

    [ObservableProperty]
    private string _defaultUserInfoSerial;

    /// <summary>Zie InstallerProject.UsePreviousUserInfo: hier bewerkbaar, net als de andere
    /// velden van dit scherm, in plaats van op het tabblad Overige instellingen — de gebruiker
    /// bewerkt alles over de User Info-pagina op één plek.</summary>
    [ObservableProperty]
    private bool _usePreviousUserInfo;
}
