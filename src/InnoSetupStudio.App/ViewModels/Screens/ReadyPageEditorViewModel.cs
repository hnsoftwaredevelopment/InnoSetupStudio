using CommunityToolkit.Mvvm.ComponentModel;
using InnoSetupStudio.App.Localization;

namespace InnoSetupStudio.App.ViewModels.Screens;

/// <summary>
/// Klaar-om-te-installeren-pagina (Ready to Install): toont vlak voor de echte installatie een
/// samenvatting van de gemaakte keuzes. Zoals WelcomePageEditorViewModel: geen bestandskeuze of
/// eigen knop, alleen drie vinkjes die bepalen wat die samenvatting toont.
/// </summary>
public sealed partial class ReadyPageEditorViewModel : WizardScreenEditorViewModel
{
    public ReadyPageEditorViewModel(
        bool disableReadyMemo,
        bool alwaysShowDirOnReadyPage,
        bool alwaysShowGroupOnReadyPage)
        : base("ShowReadyPage", LocalizationManager.Instance["WizardScreenReady"], "Check")
    {
        _disableReadyMemo = disableReadyMemo;
        _alwaysShowDirOnReadyPage = alwaysShowDirOnReadyPage;
        _alwaysShowGroupOnReadyPage = alwaysShowGroupOnReadyPage;
    }

    [ObservableProperty]
    private bool _disableReadyMemo;

    [ObservableProperty]
    private bool _alwaysShowDirOnReadyPage;

    [ObservableProperty]
    private bool _alwaysShowGroupOnReadyPage;
}
