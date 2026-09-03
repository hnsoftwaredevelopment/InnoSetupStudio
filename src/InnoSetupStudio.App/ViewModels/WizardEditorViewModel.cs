using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InnoSetupStudio.App.ViewModels.Screens;
using InnoSetupStudio.Core.Project;

namespace InnoSetupStudio.App.ViewModels;

/// <summary>
/// ViewModel voor de schermeditor (fase 4): de inhoud bewerken van de wizardschermen die in fase
/// 3 zijn aangevinkt, met een voorvertoning die Inno Setup's eigen weergave benadert (zie
/// ScreenEditorPreviewDisclaimer in de vertalingen). Alleen schermen waarvoor al een editor
/// gebouwd is (Welkom, Licentieovereenkomst, Installatiemap kiezen) staan in de lijst; de overige
/// acht standaardschermen volgen in latere PR's van deze fase. Een aangevinkt scherm zonder editor
/// verschijnt dus nog niet hier — bewuste, tijdelijke scope-afbakening voor deze eerste PR.
/// </summary>
public sealed partial class WizardEditorViewModel : DirtyTrackingViewModel
{
    private readonly List<WizardScreenEditorViewModel> _screens;

    public WizardEditorViewModel(InstallerProject project, string? projectFilePath, IProjectAssetService assetService)
    {
        BeginInit();

        _screens = [];
        if (project.WizardScreens.ShowWelcomePage)
        {
            _screens.Add(new WelcomePageEditorViewModel(project.AppName, project.AppVersion));
        }

        if (project.WizardScreens.ShowLicensePage)
        {
            _screens.Add(new LicensePageEditorViewModel(project.LicenseFilePath, projectFilePath, assetService));
        }

        if (project.WizardScreens.ShowSelectDestinationPage)
        {
            _screens.Add(new SelectDestinationPageEditorViewModel(project.AppName, project.DefaultDirName, project.AllowUserToChangeDir));
        }

        // Elk scherm is een los object (geen [ObservableProperty] van deze klasse zelf), dus we
        // luisteren naar PropertyChanged van elk scherm om de dirty-status bij te houden — zelfde
        // patroon als WizardScreensViewModel in fase 3. De schermen zijn hierboven net aangemaakt
        // met hun beginwaarde via de constructor, niet via een property-setter, dus dit abonneren
        // zelf triggert nog geen PropertyChanged en dus ook geen valse dirty-melding.
        foreach (var screen in _screens)
        {
            screen.PropertyChanged += (_, _) => MarkDirty();
        }

        _selectedScreen = _screens.Count > 0 ? _screens[0] : null;

        EndInit();
    }

    /// <summary>De schermen die (a) aan staan in de wizardschermen-selectie (fase 3) en (b) al een
    /// editor hebben, in Inno Setup's eigen volgorde.</summary>
    public IReadOnlyList<WizardScreenEditorViewModel> Screens => _screens;

    /// <summary>True als er tenminste één scherm te bewerken is; anders toont het venster een
    /// toelichting (ScreenEditorNoScreens) in plaats van de drie panelen.</summary>
    public bool HasScreens => _screens.Count > 0;

    /// <summary>Exacte tegenhanger van <see cref="HasScreens"/>, puur zodat WizardEditorWindow.xaml
    /// met dezelfde (niet-inverterende) BooleanToVisibilityConverter kan werken voor zowel de
    /// toelichting als de drie panelen, in plaats van daar een tweede, inverterende converter
    /// voor te schrijven.</summary>
    public bool HasNoScreens => !HasScreens;

    [NotifyCanExecuteChangedFor(nameof(BackCommand))]
    [NotifyCanExecuteChangedFor(nameof(NextCommand))]
    [ObservableProperty]
    private WizardScreenEditorViewModel? _selectedScreen;

    private int SelectedIndex => SelectedScreen is null ? -1 : _screens.IndexOf(SelectedScreen);

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
                case LicensePageEditorViewModel license:
                    project.LicenseFilePath = license.LicenseFilePath;
                    break;
                case SelectDestinationPageEditorViewModel destination:
                    project.DefaultDirName = destination.DefaultDirName;
                    project.AllowUserToChangeDir = destination.AllowUserToChangeDir;
                    break;
            }
        }
    }
}
