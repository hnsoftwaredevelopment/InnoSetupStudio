using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InnoSetupStudio.App.ViewModels.Screens;
using InnoSetupStudio.Core.Project;

namespace InnoSetupStudio.App.ViewModels;

/// <summary>
/// ViewModel voor de schermeditor (fase 4): de inhoud bewerken van de wizardschermen die in fase
/// 3 zijn aangevinkt, met een voorvertoning die Inno Setup's eigen weergave benadert (zie
/// ScreenEditorPreviewDisclaimer in de vertalingen). Alleen schermen waarvoor al een editor
/// gebouwd is staan in de lijst: Welkom, Licentieovereenkomst, Info Before, User Info,
/// Installatiemap kiezen, Select Start Menu Folder, Klaar om te installeren en Info After. Select
/// Components, Select Tasks en Voltooid volgen in een latere PR van deze fase. Een aangevinkt
/// scherm zonder editor verschijnt dus nog niet hier — bewuste, tijdelijke scope-afbakening.
/// </summary>
public sealed partial class WizardEditorViewModel : DirtyTrackingViewModel
{
    private readonly List<WizardScreenEditorViewModel> _screens;
    private readonly DefaultScreenEditorViewModel _defaultScreen;

    public WizardEditorViewModel(InstallerProject project, string? projectFilePath, IProjectAssetService assetService)
    {
        BeginInit();

        // Meertalige knopteksten (sectie 14-backlogitem): één keer berekend voor de hele
        // schermeditor-sessie en aan elk scherm (inclusief het Standaardscherm) meegegeven, zie
        // WizardScreenEditorViewModel.NonEnglishLanguageIds. Project.SupportedLanguageIds bevat
        // altijd Engels (InnoLanguageCatalog.EnglishId, zie InstallerProject/LanguagesViewModel),
        // dat hier dus wordt uitgefilterd — Engels gebruikt gewoon de bestaande Caption/Tooltip-
        // velden, geen eigen rij in de per-taal-lijst. SetActiveProject bouwt deze hele
        // WizardEditorViewModel opnieuw op na elke Projectinstellingen-opslag (zie MainWindow),
        // dus een gewijzigde talenselectie komt hier vanzelf weer vers binnen.
        var nonEnglishLanguageIds = project.SupportedLanguageIds
            .Where(id => id != InnoLanguageCatalog.EnglishId)
            .ToList();

        // Het Standaardscherm (§12.6/§12.7): één instantie voor de hele sessie, hieronder aan elk
        // scherm doorgegeven via de required Defaults-eigenschap, vóórdat die schermen zelf
        // aangemaakt worden. Geen aan/uit-vinkje zoals de echte schermen (WizardScreens uit fase
        // 3) — dit scherm bestaat altijd, ongeacht welke installerschermen aan staan. Sinds
        // backlogitem 1 (sectie 14) is dit ook de plek waar WizardImageFile/WizardSmallImageFile
        // vandaan komen (voorheen hier zelf één keer opgezocht via WizardImageResolver); elk scherm
        // leest ze voortaan live van hier via WizardScreenEditorViewModel.WizardImage/
        // WizardSmallImage, in plaats van een eenmalige init-waarde te krijgen.
        _defaultScreen = new DefaultScreenEditorViewModel(project.DefaultScreenButtons, project.WizardImageFile, project.WizardSmallImageFile, projectFilePath, assetService, nonEnglishLanguageIds);

        // Bewust GEEN collectie-expressie ([_defaultScreen]) hier: de compiler bakt die voor een
        // IReadOnlyList<T>-doeltype met precies één element in tot een intern eenmalig-element-
        // type (<>z__ReadOnlySingleElementList<T>) waarvan de expliciete IList.Contains(object)
        // ongeconditioneerd naar T cast in plaats van eerst een is-check te doen. WPF's Selector
        // (de linkerlijst-ListBox hieronder in WizardEditorWindow.xaml) roept die Contains aan
        // zodra de twee ListBoxen hun gedeelde SelectedScreen coeren — zodra dat een
        // WizardScreenEditorViewModel is (elk echt scherm, niet het Standaardscherm zelf), gooide
        // dat een InvalidCastException naar DefaultScreenEditorViewModel. Een gewone List<T> heeft
        // wél een veilige IList.Contains (eerst is-check, dan pas casten), dus dat gebruiken we
        // hier in plaats van de kortere collectie-expressie-syntax.
        DefaultScreenRow = new List<DefaultScreenEditorViewModel> { _defaultScreen };

        _screens = [];
        if (project.WizardScreens.ShowWelcomePage)
        {
            _screens.Add(new WelcomePageEditorViewModel(project.AppName, project.AppVersion)
            {
                ButtonSettings = project.WelcomeScreenButtons,
                Defaults = _defaultScreen,
                NonEnglishLanguageIds = nonEnglishLanguageIds,
            });
        }

        if (project.WizardScreens.ShowLicensePage)
        {
            _screens.Add(new LicensePageEditorViewModel(project.LicenseFilePath, projectFilePath, assetService)
            {
                ButtonSettings = project.LicenseScreenButtons,
                Defaults = _defaultScreen,
                NonEnglishLanguageIds = nonEnglishLanguageIds,
            });
        }

        if (project.WizardScreens.ShowInfoBeforePage)
        {
            _screens.Add(new InfoBeforePageEditorViewModel(project.InfoBeforeFilePath, projectFilePath, assetService)
            {
                ButtonSettings = project.InfoBeforeScreenButtons,
                Defaults = _defaultScreen,
                NonEnglishLanguageIds = nonEnglishLanguageIds,
            });
        }

        if (project.WizardScreens.ShowUserInfoPage)
        {
            _screens.Add(new UserInfoPageEditorViewModel(project.DefaultUserInfoName, project.DefaultUserInfoOrg, project.DefaultUserInfoSerial, project.UsePreviousUserInfo)
            {
                ButtonSettings = project.UserInfoScreenButtons,
                Defaults = _defaultScreen,
                NonEnglishLanguageIds = nonEnglishLanguageIds,
            });
        }

        if (project.WizardScreens.ShowSelectDestinationPage)
        {
            _screens.Add(new SelectDestinationPageEditorViewModel(project.AppName, project.DefaultDirName, project.AllowUserToChangeDir, project.SelectDestinationBrowseButton)
            {
                ButtonSettings = project.SelectDestinationScreenButtons,
                Defaults = _defaultScreen,
                NonEnglishLanguageIds = nonEnglishLanguageIds,
            });
        }

        // Inno Setup's eigen volgorde plaatst Select Components en Select Tasks hiertussen; die
        // twee hebben nog geen editor (zie de klasse-doccomment hierboven), dus Select Program
        // Group en Ready sluiten hier rechtstreeks aan op Select Destination.
        if (project.WizardScreens.ShowSelectProgramGroupPage)
        {
            _screens.Add(new SelectProgramGroupPageEditorViewModel(project.DefaultGroupName, project.AppendDefaultGroupName, project.AlwaysUsePersonalGroup)
            {
                ButtonSettings = project.SelectProgramGroupScreenButtons,
                Defaults = _defaultScreen,
                NonEnglishLanguageIds = nonEnglishLanguageIds,
            });
        }

        if (project.WizardScreens.ShowReadyPage)
        {
            _screens.Add(new ReadyPageEditorViewModel(project.DisableReadyMemo, project.AlwaysShowDirOnReadyPage, project.AlwaysShowGroupOnReadyPage)
            {
                ButtonSettings = project.ReadyScreenButtons,
                Defaults = _defaultScreen,
                NonEnglishLanguageIds = nonEnglishLanguageIds,
            });
        }

        if (project.WizardScreens.ShowInfoAfterPage)
        {
            _screens.Add(new InfoAfterPageEditorViewModel(project.InfoAfterFilePath, projectFilePath, assetService)
            {
                ButtonSettings = project.InfoAfterScreenButtons,
                Defaults = _defaultScreen,
                NonEnglishLanguageIds = nonEnglishLanguageIds,
            });
        }

        // Elk scherm is een los object (geen [ObservableProperty] van deze klasse zelf), dus we
        // luisteren naar PropertyChanged van elk scherm om de dirty-status bij te houden — zelfde
        // patroon als WizardScreensViewModel in fase 3. De schermen zijn hierboven net aangemaakt
        // met hun beginwaarde via de constructor, niet via een property-setter, dus dit abonneren
        // zelf triggert nog geen PropertyChanged en dus ook geen valse dirty-melding. Het
        // Standaardscherm telt hier ook mee: wijzig je daar iets, dan is de schermeditor als
        // geheel net zo goed gewijzigd als bij een van de echte schermen.
        foreach (var screen in _screens)
        {
            screen.PropertyChanged += (_, _) => MarkDirty();
        }

        _defaultScreen.PropertyChanged += (_, _) => MarkDirty();

        // Het Standaardscherm bestaat altijd (zie hierboven), dus zonder aangevinkte echte
        // schermen valt de selectie daarop terug in plaats van op null — anders opent de
        // schermeditor met niets geselecteerd terwijl er wél iets te bewerken is.
        _selectedScreen = _screens.Count > 0 ? (object)_screens[0] : _defaultScreen;

        EndInit();
    }

    /// <summary>De schermen die (a) aan staan in de wizardschermen-selectie (fase 3) en (b) al een
    /// editor hebben, in Inno Setup's eigen volgorde.</summary>
    public IReadOnlyList<WizardScreenEditorViewModel> Screens => _screens;

    /// <summary>Enkel-item lijst voor de eigen rij van het Standaardscherm bovenaan de
    /// linkerlijst (WizardEditorWindow.xaml), boven de scheidingslijn met <see cref="Screens"/>.
    /// Een losse lijst (in plaats van dit gewoon vóór de echte schermen in <see cref="Screens"/>
    /// te zetten) zodat het Standaardscherm buiten Back/Next-navigatie (<see cref="SelectedIndex"/>)
    /// blijft — het is bewust geen "scherm nul" tussen de echte installerschermen, zie §12.7.</summary>
    public IReadOnlyList<DefaultScreenEditorViewModel> DefaultScreenRow { get; }

    /// <summary>True als er tenminste één echt scherm te bewerken is. Het Standaardscherm bestaat
    /// altijd (zie <see cref="DefaultScreenRow"/>) en blijft dus ook bereikbaar/bewerkbaar als dit
    /// false is; WizardEditorWindow.xaml toont in dat geval alleen een aanvullende toelichting
    /// (ScreenEditorNoScreens) naast de drie panelen, niet in plaats daarvan.</summary>
    public bool HasScreens => _screens.Count > 0;

    /// <summary>Exacte tegenhanger van <see cref="HasScreens"/>, puur zodat WizardEditorWindow.xaml
    /// met dezelfde (niet-inverterende) BooleanToVisibilityConverter kan werken, in plaats van
    /// daar een tweede, inverterende converter voor te schrijven.</summary>
    public bool HasNoScreens => !HasScreens;

    // Type object (niet WizardScreenEditorViewModel) omdat SelectedScreen ook het Standaardscherm
    // moet kunnen zijn (DefaultScreenEditorViewModel), en die twee typen bewust geen gemeenschappelijke
    // basisklasse delen (zie DefaultScreenEditorViewModel's doc-comment). WizardEditorWindow.xaml
    // kan de juiste template nog steeds automatisch per runtime-type kiezen; alleen SelectedIndex/
    // Back/Next hieronder moeten expliciet met een is-check omgaan met het geval dat het
    // Standaardscherm geselecteerd is (dan -1, buiten de echte-schermen-navigatie).
    [NotifyCanExecuteChangedFor(nameof(BackCommand))]
    [NotifyCanExecuteChangedFor(nameof(NextCommand))]
    [NotifyPropertyChangedFor(nameof(IsDefaultScreenSelected))]
    [NotifyPropertyChangedFor(nameof(IsRealScreenSelected))]
    [ObservableProperty]
    private object? _selectedScreen;

    /// <summary>True zodra het Standaardscherm geselecteerd is, in plaats van een van de echte
    /// installerschermen. Bepaalt in WizardEditorWindow.xaml of de installervoorvertoning of de
    /// toelichtende tekst voor het Standaardscherm getoond wordt (zie DefaultScreenEditorViewModel).</summary>
    public bool IsDefaultScreenSelected => SelectedScreen is DefaultScreenEditorViewModel;

    /// <summary>Exacte tegenhanger van <see cref="IsDefaultScreenSelected"/>, puur zodat
    /// WizardEditorWindow.xaml met dezelfde (niet-inverterende) BooleanToVisibilityConverter kan
    /// werken voor beide kanten, net als <see cref="HasNoScreens"/> hierboven.</summary>
    public bool IsRealScreenSelected => !IsDefaultScreenSelected;

    private int SelectedIndex => SelectedScreen is WizardScreenEditorViewModel screen ? _screens.IndexOf(screen) : -1;

    private bool CanGoBack() => SelectedIndex > 0;

    [RelayCommand(CanExecute = nameof(CanGoBack))]
    private void Back() => SelectedScreen = _screens[SelectedIndex - 1];

    private bool CanGoNext() => SelectedIndex >= 0 && SelectedIndex < _screens.Count - 1;

    [RelayCommand(CanExecute = nameof(CanGoNext))]
    private void Next() => SelectedScreen = _screens[SelectedIndex + 1];

    /// <summary>Vuurt wanneer het venster moet sluiten: true bij Opslaan, false bij Sluiten/Annuleren.</summary>
    public event EventHandler<bool>? RequestClose;

    // Net als bij WizardScreensViewModel (fase 3) staat Opslaan hier niet uit zolang er niets
    // gewijzigd is: opnieuw opslaan van een ongewijzigd scherm is onschadelijk, en dat houdt dit
    // venster consistent met dat andere venster in plaats van CanSave-gedrag alleen hier toe te
    // voegen.
    [RelayCommand]
    private void Save() => RequestClose?.Invoke(this, true);

    [RelayCommand]
    private void Cancel() => RequestClose?.Invoke(this, false);

    /// <summary>Schrijft de bewerkte velden terug naar het project. Alleen de velden van de
    /// schermen die in deze editor stonden (zie <see cref="Screens"/>) worden bijgewerkt; de rest
    /// van het project blijft ongewijzigd.</summary>
    public void ApplyTo(InstallerProject project)
    {
        foreach (var screen in _screens)
        {
            switch (screen)
            {
                case WelcomePageEditorViewModel welcome:
                    project.WelcomeScreenButtons = welcome.ReadButtonSettings();
                    break;
                case LicensePageEditorViewModel license:
                    project.LicenseFilePath = license.LicenseFilePath;
                    project.LicenseScreenButtons = license.ReadButtonSettings();
                    break;
                case InfoBeforePageEditorViewModel infoBefore:
                    project.InfoBeforeFilePath = infoBefore.InfoFilePath;
                    project.InfoBeforeScreenButtons = infoBefore.ReadButtonSettings();
                    break;
                case UserInfoPageEditorViewModel userInfo:
                    project.DefaultUserInfoName = userInfo.DefaultUserInfoName;
                    project.DefaultUserInfoOrg = userInfo.DefaultUserInfoOrg;
                    project.DefaultUserInfoSerial = userInfo.DefaultUserInfoSerial;
                    project.UsePreviousUserInfo = userInfo.UsePreviousUserInfo;
                    project.UserInfoScreenButtons = userInfo.ReadButtonSettings();
                    break;
                case SelectDestinationPageEditorViewModel destination:
                    project.DefaultDirName = destination.DefaultDirName;
                    project.AllowUserToChangeDir = destination.AllowUserToChangeDir;
                    project.SelectDestinationScreenButtons = destination.ReadButtonSettings();
                    project.SelectDestinationBrowseButton = destination.ReadBrowseButtonSettings();
                    break;
                case SelectProgramGroupPageEditorViewModel programGroup:
                    project.DefaultGroupName = programGroup.DefaultGroupName;
                    project.AppendDefaultGroupName = programGroup.AppendDefaultGroupName;
                    project.AlwaysUsePersonalGroup = programGroup.AlwaysUsePersonalGroup;
                    project.SelectProgramGroupScreenButtons = programGroup.ReadButtonSettings();
                    break;
                case ReadyPageEditorViewModel ready:
                    project.DisableReadyMemo = ready.DisableReadyMemo;
                    project.AlwaysShowDirOnReadyPage = ready.AlwaysShowDirOnReadyPage;
                    project.AlwaysShowGroupOnReadyPage = ready.AlwaysShowGroupOnReadyPage;
                    project.ReadyScreenButtons = ready.ReadButtonSettings();
                    break;
                case InfoAfterPageEditorViewModel infoAfter:
                    project.InfoAfterFilePath = infoAfter.InfoFilePath;
                    project.InfoAfterScreenButtons = infoAfter.ReadButtonSettings();
                    break;
            }
        }

        project.DefaultScreenButtons = _defaultScreen.ReadButtonSettings();
        project.WizardImageFile = _defaultScreen.WizardImageFile;
        project.WizardSmallImageFile = _defaultScreen.WizardSmallImageFile;
    }
}
