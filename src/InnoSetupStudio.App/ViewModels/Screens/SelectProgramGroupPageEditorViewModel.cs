using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using InnoSetupStudio.App.Localization;
using InnoSetupStudio.Core.Project;

namespace InnoSetupStudio.App.ViewModels.Screens;

/// <summary>
/// Select Start Menu Folder-pagina (Inno Setup's eigen naam voor deze pagina; de interne
/// <c>WizardScreenSelection.ShowSelectProgramGroupPage</c>-vlag heet naar Inno Setup's
/// Pascal Script-kant "ProgramGroup", zie de toelichting daar). Net als de User Info-pagina geen
/// bestandskeuze of eigen knop: alleen een vooringevulde groepsnaam, twee vinkjes en (sinds
/// 2026-10-02, Herberts verzoek) een drie-waardige paginazichtbaarheid.
/// </summary>
public sealed partial class SelectProgramGroupPageEditorViewModel : WizardScreenEditorViewModel
{
    private readonly string _appName;

    public SelectProgramGroupPageEditorViewModel(
        string appName,
        string defaultGroupName,
        bool appendDefaultGroupName,
        bool alwaysUsePersonalGroup,
        DisablePageMode groupPageMode)
        : base("ShowSelectProgramGroupPage", LocalizationManager.Instance["WizardScreenSelectProgramGroup"], "Folder")
    {
        _appName = appName;
        _defaultGroupName = defaultGroupName;
        _appendDefaultGroupName = appendDefaultGroupName;
        _alwaysUsePersonalGroup = alwaysUsePersonalGroup;
        _groupPageMode = groupPageMode;
    }

    [ObservableProperty]
    private string _defaultGroupName;

    [ObservableProperty]
    private bool _appendDefaultGroupName;

    [ObservableProperty]
    private bool _alwaysUsePersonalGroup;

    [ObservableProperty]
    private DisablePageMode _groupPageMode;

    /// <summary>Toont een toelichting zodra deze pagina nooit getoond wordt (de voorgestelde
    /// groepsnaam ligt dan vast) — zie <see cref="DisablePageMode"/>.</summary>
    public Visibility IsNeverShowHintVisible => GroupPageMode == DisablePageMode.NeverShow ? Visibility.Visible : Visibility.Collapsed;

    /// <summary>Toont een toelichting zodra deze pagina bij een update automatisch wordt
    /// overgeslagen (Herberts verzoek, 2026-10-02) — de voorvertoning simuleert hier bewust een
    /// eerste installatie, zie <see cref="DisablePageMode.AutoSkipIfKnown"/>.</summary>
    public Visibility IsAutoSkipHintVisible => GroupPageMode == DisablePageMode.AutoSkipIfKnown ? Visibility.Visible : Visibility.Collapsed;

    partial void OnGroupPageModeChanged(DisablePageMode value)
    {
        OnPropertyChanged(nameof(IsNeverShowHintVisible));
        OnPropertyChanged(nameof(IsAutoSkipHintVisible));
    }

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
