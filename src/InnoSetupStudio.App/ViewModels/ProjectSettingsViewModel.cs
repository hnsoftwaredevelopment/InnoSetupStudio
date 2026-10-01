using System.IO;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InnoSetupStudio.App.Localization;
using InnoSetupStudio.Core.Project;
using Microsoft.Win32;

namespace InnoSetupStudio.App.ViewModels;

/// <summary>
/// ViewModel voor het projectinstellingen-scherm: naam, ontwikkelaar, contactgegevens en de
/// bestandslocaties van één <see cref="InstallerProject"/>. Bewaart naar een .issproj-bestand
/// via <see cref="IInstallerProjectService"/>.
/// </summary>
public sealed partial class ProjectSettingsViewModel : DirtyTrackingViewModel
{
    private readonly IInstallerProjectService _projectService;

    // Bewaard vanuit het project waarmee dit venster is geopend, zodat SaveAsync deze waarden kan
    // meenemen in het opgeslagen project: dit scherm toont en wijzigt alleen de algemene
    // instellingen, dus zonder deze velden zou een simpele naam- of paden-wijziging de elders (in
    // de schermeditor) gekozen licentiebestand, installatiemap, wizardafbeeldingen en
    // knopinstellingen per scherm stilzwijgend terugzetten naar de standaardwaarden.
    // WizardImageFile/WizardSmallImageFile staan hier sinds backlogitem 1 (sectie 14) om dezelfde
    // reden als de eerste drie: ze worden voortaan op het Standaardscherm in de schermeditor
    // bewerkt, niet meer hier, dus dit scherm mag ze alleen ongewijzigd doorgeven. De vijf
    // knopinstellingen-velden (WelcomeScreenButtons t/m DefaultScreenButtons) ontbraken hier tot
    // sectie 16 van de architectuurdoc: zonder pass-through zette Opslaan vanuit dit scherm elke
    // in de schermeditor gekozen tekstkleur/lettertype/Enabled/Visible per scherm stilzwijgend
    // terug naar leeg/onbepaald.
    //
    // WizardScreens (welke schermen meedoen) en SupportedLanguageIds (welke talen) staan sinds
    // sectie 21 niet meer in deze pass-through lijst, maar zijn hier juist wél bewerkbaar
    // geworden: Herbert wilde ze allebei in Projectinstellingen ("dit zie ik allemaal als
    // projectinstellingen"), niet als eigen knop/venster op het hoofdscherm. Reuse van de
    // bestaande WizardScreensViewModel/LanguagesViewModel als sub-viewmodel in plaats van hier
    // een tweede implementatie van dezelfde rijenlijst te schrijven — hun eigen Save/Cancel/
    // RequestClose blijven ongebruikt, dit venster stuurt zijn eigen Opslaan/Annuleren aan. Dit
    // repareert meteen een bug: SupportedLanguageIds ontbrak hier volledig, dus Opslaan vanuit dit
    // scherm zette een via het (inmiddels verwijderde) Talen-venster gekozen meertalige selectie
    // stilzwijgend terug naar alleen Engels (InstallerProject.SupportedLanguageIds' eigen
    // standaardwaarde), op precies dezelfde manier als de knopinstellingen-bug uit sectie 16.
    private readonly WizardScreensViewModel _wizardScreensSubViewModel;
    private readonly LanguagesViewModel _languagesSubViewModel;
    private readonly string _licenseFilePath;
    private readonly string _defaultDirName;
    private readonly bool _allowUserToChangeDir;
    private readonly string _wizardImageFile;
    private readonly string _wizardSmallImageFile;
    private readonly WizardScreenButtonSettings _welcomeScreenButtons;
    private readonly WizardScreenButtonSettings _licenseScreenButtons;
    private readonly WizardScreenButtonSettings _selectDestinationScreenButtons;
    private readonly BrowseButtonSettings _selectDestinationBrowseButton;
    private readonly WizardScreenButtonSettings _defaultScreenButtons;

    public ProjectSettingsViewModel(InstallerProject project, IInstallerProjectService projectService, string? projectFilePath)
    {
        _projectService = projectService;
        BeginInit();
        _projectFilePath = projectFilePath;

        _wizardScreensSubViewModel = new WizardScreensViewModel(project.WizardScreens);
        _languagesSubViewModel = new LanguagesViewModel(project.SupportedLanguageIds);
        _wizardScreensSubViewModel.PropertyChanged += (_, _) => MarkDirty();
        _languagesSubViewModel.PropertyChanged += (_, _) => MarkDirty();

        _licenseFilePath = project.LicenseFilePath;
        _defaultDirName = project.DefaultDirName;
        _allowUserToChangeDir = project.AllowUserToChangeDir;
        _wizardImageFile = project.WizardImageFile;
        _wizardSmallImageFile = project.WizardSmallImageFile;
        _welcomeScreenButtons = project.WelcomeScreenButtons;
        _licenseScreenButtons = project.LicenseScreenButtons;
        _selectDestinationScreenButtons = project.SelectDestinationScreenButtons;
        _selectDestinationBrowseButton = project.SelectDestinationBrowseButton;
        _defaultScreenButtons = project.DefaultScreenButtons;

        AppId = project.AppId;
        AppName = project.AppName;
        AppVersion = project.AppVersion;
        Publisher = project.Publisher;
        PublisherEmail = project.PublisherEmail;
        PublisherUrl = project.PublisherUrl;
        SourceFilesPath = project.SourceFilesPath;
        OutputPath = project.OutputPath;
        CustomImagesPath = project.CustomImagesPath;
        SetupIconFile = project.SetupIconFile;

        EndInit();

        // Bij een al bestaand (opgeslagen) project doet Annuleren feitelijk niets anders dan het
        // venster sluiten zonder de instellingen te wijzigen — het project blijft gewoon actief.
        // "Openen" beschrijft dat beter dan "Annuleren"; bij een nieuw, nog niet opgeslagen
        // project betekent dezelfde knop wel echt het project verwerpen, dus daar blijft
        // "Annuleren" staan. In tegenstelling tot de standaard van DirtyTrackingViewModel bepaalt
        // hier dus niet de dirty-status maar het bestaan van het project wat de knop doet, vandaar
        // de overrides van CancelButtonText/CancelButtonIconKey hieronder.
        IsExistingProject = !string.IsNullOrWhiteSpace(projectFilePath);
    }

    /// <summary>True als dit venster is geopend voor een al bestaand (opgeslagen) project, false
    /// voor een nieuw project. Bepaalt welk label/icoon de knop naast Opslaan toont (zie
    /// <see cref="CancelButtonText"/>/<see cref="CancelButtonIconKey"/>).</summary>
    public bool IsExistingProject { get; }

    /// <summary>De elf standaard wizardschermen met hun aan/uit-vinkje (tabblad Schermen, sectie
    /// 21) — voorheen het eigen WizardScreensWindow.</summary>
    public IReadOnlyList<WizardScreenRow> WizardScreens => _wizardScreensSubViewModel.Screens;

    /// <summary>De 33 ondersteunde talen met hun aan/uit-vinkje (tabblad Talen, sectie 19/21) —
    /// voorheen het eigen LanguagesWindow. Engels blijft aangevinkt en uitgeschakeld
    /// (<see cref="LanguageRow.IsLocked"/>).</summary>
    public IReadOnlyList<LanguageRow> Languages => _languagesSubViewModel.Languages;

    /// <inheritdoc/>
    public override string CancelButtonText => IsExistingProject
        ? LocalizationManager.Instance["ButtonOpen"]
        : LocalizationManager.Instance["ButtonCancel"];

    /// <inheritdoc/>
    public override string CancelButtonIconKey => IsExistingProject ? "Folder" : "Close";

    /// <summary>Wordt gevuld zodra <see cref="SaveCommand"/> succesvol heeft opgeslagen, zodat de
    /// aanroepende code (MainWindow) weet welk projectbestand actief is geworden.</summary>
    public string? SavedProjectFilePath { get; private set; }

    /// <summary>Het exacte project zoals het net is opgeslagen, zodat de aanroepende code
    /// (MainWindow) dit als actief project kan bijhouden zonder het bestand opnieuw te hoeven
    /// inlezen.</summary>
    public InstallerProject? SavedProject { get; private set; }

    /// <summary>Vuurt wanneer het venster moet sluiten: true bij Opslaan, false bij Annuleren.</summary>
    public event EventHandler<bool>? RequestClose;

    [ObservableProperty]
    private string? _projectFilePath;

    // AppId wordt één keer gegenereerd bij het aanmaken van een project (zie
    // InstallerProject.CreateNew) en blijft daarna vast: een gewijzigd AppId laat Inno Setup een
    // eerdere installatie niet meer herkennen. Vandaar alleen-lezen in de UI.
    [ObservableProperty]
    private string _appId = string.Empty;

    [ObservableProperty]
    private string _appName = string.Empty;

    [ObservableProperty]
    private string _appVersion = string.Empty;

    [ObservableProperty]
    private string _publisher = string.Empty;

    [ObservableProperty]
    private string _publisherEmail = string.Empty;

    [ObservableProperty]
    private string _publisherUrl = string.Empty;

    [ObservableProperty]
    private string _sourceFilesPath = string.Empty;

    [ObservableProperty]
    private string _outputPath = string.Empty;

    [ObservableProperty]
    private string _customImagesPath = string.Empty;

    [ObservableProperty]
    private string _setupIconFile = string.Empty;

    [RelayCommand]
    private void BrowseSourceFiles() => SourceFilesPath = BrowseForFolder(SourceFilesPath) ?? SourceFilesPath;

    [RelayCommand]
    private void BrowseOutput() => OutputPath = BrowseForFolder(OutputPath) ?? OutputPath;

    [RelayCommand]
    private void BrowseCustomImages() => CustomImagesPath = BrowseForFolder(CustomImagesPath) ?? CustomImagesPath;

    [RelayCommand]
    private void BrowseIcon()
    {
        var dialog = new OpenFileDialog
        {
            Filter = LocalizationManager.Instance["DialogFilterIconFiles"],
        };

        if (!string.IsNullOrWhiteSpace(SetupIconFile))
        {
            dialog.InitialDirectory = Path.GetDirectoryName(SetupIconFile);
        }

        if (dialog.ShowDialog() == true)
        {
            SetupIconFile = dialog.FileName;
        }
    }

    // True zolang SaveAsync bezig is. De velden worden hiermee uitgeschakeld (zie CanEdit) zodat
    // een wijziging tijdens de lopende await niet stilzwijgend verloren gaat: zonder deze guard
    // zou een edit tijdens het opslaan de dirty-vlag weer op true zetten, waarna SaveAsync die na
    // een geslaagde save alsnog terug op false zet en de wijziging zo verstopt.
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    [NotifyCanExecuteChangedFor(nameof(CancelCommand))]
    [ObservableProperty]
    private bool _isSaving;

    partial void OnIsSavingChanged(bool value) => OnPropertyChanged(nameof(CanEdit));

    /// <summary>Bepaalt of de invoervelden en de Openen/Annuleren-knop bewerkbaar zijn: uit
    /// tijdens het opslaan.</summary>
    public bool CanEdit => !IsSaving;

    private bool CanSave() => !IsSaving && IsDirty && !string.IsNullOrWhiteSpace(AppName);

    // Eén gedeelde hook voor alle bewerkbare velden: gebruikt via de On<Property>Changed-hooks
    // hieronder. Overschrijft DirtyTrackingViewModel.MarkDirty om ook SaveCommand te laten
    // herevalueren of het al enabled mag worden.
    protected override void MarkDirty()
    {
        if (IsInitializing)
        {
            return;
        }

        // Altijd herevalueren, ook als IsDirty al true was: CanSave() controleert behalve de
        // dirty-vlag ook AppName, dus als de gebruiker AppName leegmaakt nadat een ander veld al
        // dirty maakte, moet Opslaan alsnog uitschakelen. Met een vroege return alleen op basis
        // van IsDirty zou die herevaluatie gemist worden.
        base.MarkDirty();
        SaveCommand.NotifyCanExecuteChanged();
    }

    partial void OnAppNameChanged(string value) => MarkDirty();

    partial void OnAppVersionChanged(string value) => MarkDirty();

    partial void OnPublisherChanged(string value) => MarkDirty();

    partial void OnPublisherEmailChanged(string value) => MarkDirty();

    partial void OnPublisherUrlChanged(string value) => MarkDirty();

    partial void OnSourceFilesPathChanged(string value) => MarkDirty();

    partial void OnOutputPathChanged(string value) => MarkDirty();

    partial void OnCustomImagesPathChanged(string value) => MarkDirty();

    partial void OnSetupIconFileChanged(string value) => MarkDirty();

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync() => await SaveCoreAsync();

    /// <summary>
    /// Daadwerkelijke opslaanlogica achter <see cref="SaveAsync"/> (de Opslaan-knop), met een
    /// expliciet succes/mislukt-resultaat in plaats van dat achteraf via <see cref="IsDirty"/> af
    /// te leiden (CodeRabbit, PR #20) — <see cref="SaveAsync"/> zelf blijft de void-Task-vorm
    /// behouden die <c>[RelayCommand]</c> voor de knopbinding verwacht; <see
    /// cref="ConfirmDiscardChangesAsync"/> roept dit rechtstreeks aan voor een betrouwbare
    /// succes-check in plaats van IsDirty als zijkanaal te gebruiken.
    /// </summary>
    private async Task<bool> SaveCoreAsync()
    {
        var targetPath = ProjectFilePath;
        if (string.IsNullOrWhiteSpace(targetPath))
        {
            var dialog = new SaveFileDialog
            {
                Filter = LocalizationManager.Instance["DialogFilterProjectFiles"],
                FileName = string.IsNullOrWhiteSpace(AppName) ? LocalizationManager.Instance["DialogDefaultNewProjectName"] : AppName,
            };

            if (dialog.ShowDialog() != true)
            {
                return false;
            }

            targetPath = dialog.FileName;
        }

        var project = new InstallerProject
        {
            AppId = AppId,
            AppName = AppName,
            AppVersion = AppVersion,
            Publisher = Publisher,
            PublisherEmail = PublisherEmail,
            PublisherUrl = PublisherUrl,
            SourceFilesPath = SourceFilesPath,
            OutputPath = OutputPath,
            CustomImagesPath = CustomImagesPath,
            SetupIconFile = SetupIconFile,
            WizardImageFile = _wizardImageFile,
            WizardSmallImageFile = _wizardSmallImageFile,
            WizardScreens = _wizardScreensSubViewModel.ToSelection(),
            SupportedLanguageIds = _languagesSubViewModel.ToSelection(),
            LicenseFilePath = _licenseFilePath,
            DefaultDirName = _defaultDirName,
            AllowUserToChangeDir = _allowUserToChangeDir,
            WelcomeScreenButtons = _welcomeScreenButtons,
            LicenseScreenButtons = _licenseScreenButtons,
            SelectDestinationScreenButtons = _selectDestinationScreenButtons,
            SelectDestinationBrowseButton = _selectDestinationBrowseButton,
            DefaultScreenButtons = _defaultScreenButtons,
        };

        IsSaving = true;
        try
        {
            await _projectService.SaveAsync(targetPath, project);
        }
        catch (Exception ex)
        {
            // Specifieke, bruikbare foutmelding tonen in plaats van de wijzigingen stilzwijgend
            // te verliezen: het venster blijft open zodat de gebruiker het opnieuw kan proberen.
            MessageBox.Show(ex.Message, "Inno Setup Studio", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }
        finally
        {
            IsSaving = false;
        }

        // Bij een nieuw project (nog geen ProjectFilePath) onthouden welk bestand net via de
        // Opslaan-dialoog is gekozen, zodat een volgende Opslaan-klik in dezelfde sessie niet
        // opnieuw om een locatie vraagt.
        ProjectFilePath = targetPath;
        SavedProjectFilePath = targetPath;
        SavedProject = project;
        IsDirty = false;
        RequestClose?.Invoke(this, true);
        return true;
    }

    private bool CanCancel() => !IsSaving;

    [RelayCommand(CanExecute = nameof(CanCancel))]
    private async Task CancelAsync()
    {
        // Bij een nieuw project ís deze knopklik (Annuleren) zelf al de expliciete keuze om de
        // wijzigingen te verwerpen — zie CancelButtonText hierboven, die dan ook "Annuleren" toont
        // in plaats van "Openen". Geen aparte waarschuwing nodig; Herbert bevestigde dit expliciet
        // (2026-09-30): bewust geen melding bij Annuleren op een nieuw project. Bij een al bestaand
        // project heet deze knop "Openen" — het project blijft open met de instellingen zoals ze op
        // schijf staan, wat dubbelzinnig is zodra er nog niet-opgeslagen veldwijzigingen zijn (die
        // worden dan alsnog stilzwijgend weggegooid tenzij we hier waarschuwen). Zelfde
        // Ja/Nee/Annuleren-vraag als X-sluiten (zie ProjectSettingsWindow_Closing).
        if (IsExistingProject)
        {
            var decision = await ConfirmDiscardChangesAsync();
            if (decision != UnsavedChangesDecision.Proceed)
            {
                // Abort: gebruiker annuleerde, venster blijft open. AlreadyClosing: SaveAsync
                // heeft zelf al RequestClose(true) gevuurd, hieronder dus niet nogmaals sluiten.
                return;
            }
        }

        RequestClose?.Invoke(this, false);
    }

    /// <summary>
    /// Vraagt de gebruiker om niet-opgeslagen wijzigingen in dit scherm op te slaan of te
    /// verwerpen, gebruikt door zowel <see cref="CancelAsync"/> (alleen bij een al bestaand
    /// project, zie daar) als <see cref="Views.ProjectSettingsWindow"/>'s Closing-handler (X-
    /// sluiten in de titelbalk, altijd — dat is dubbelzinnig ongeacht nieuw/bestaand project).
    /// Zelfde patroon als MainWindow.ConfirmDiscardUnsavedScreenChangesAsync.
    /// </summary>
    /// <returns>
    /// <see cref="UnsavedChangesDecision.Proceed"/> als er niets te verliezen was of de gebruiker
    /// expliciet voor verwerpen koos (de aanroeper mag zelf RequestClose(false) vuren);
    /// <see cref="UnsavedChangesDecision.AlreadyClosing"/> als opslaan is gelukt (SaveAsync heeft
    /// dan al RequestClose(true) gevuurd, de aanroeper hoeft niets meer te doen);
    /// <see cref="UnsavedChangesDecision.Abort"/> als de gebruiker annuleerde, of opslaan niet kon
    /// (bijvoorbeeld een leeg verplicht veld) of mislukte — dan blijft het venster open.
    /// </returns>
    public async Task<UnsavedChangesDecision> ConfirmDiscardChangesAsync()
    {
        if (!IsDirty)
        {
            return UnsavedChangesDecision.Proceed;
        }

        var result = MessageBox.Show(
            LocalizationManager.Instance["UnsavedProjectSettingsMessage"],
            LocalizationManager.Instance["UnsavedChangesTitle"],
            MessageBoxButton.YesNoCancel,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Cancel)
        {
            return UnsavedChangesDecision.Abort;
        }

        if (result == MessageBoxResult.No)
        {
            return UnsavedChangesDecision.Proceed;
        }

        // MessageBoxResult.Yes: CanSave() controleert behalve IsDirty ook verplichte velden
        // (AppName) — zonder deze check zou SaveCoreAsync stilzwijgend niets doen en het venster
        // alsnog dicht lijken te moeten gaan terwijl er niets is opgeslagen.
        if (!CanSave())
        {
            MessageBox.Show(
                LocalizationManager.Instance["UnsavedProjectSettingsCannotSaveMessage"],
                LocalizationManager.Instance["UnsavedChangesTitle"],
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return UnsavedChangesDecision.Abort;
        }

        // CodeRabbit (PR #20): expliciet succes/mislukt-resultaat van SaveCoreAsync gebruiken in
        // plaats van dat achteraf via IsDirty af te leiden — dat zijkanaal klopt hier in de
        // praktijk altijd (CanEdit/IsSaving schakelt de velden uit tijdens het opslaan, dus geen
        // race met een nieuwe wijziging zoals bij MainWindow's ScreenEditor), maar een
        // rechtstreeks resultaat is ondubbelzinnig en blijft dat ook als die aanname ooit wijzigt.
        return await SaveCoreAsync() ? UnsavedChangesDecision.AlreadyClosing : UnsavedChangesDecision.Abort;
    }

    private static string? BrowseForFolder(string currentPath)
    {
        var dialog = new OpenFolderDialog();
        if (!string.IsNullOrWhiteSpace(currentPath) && Directory.Exists(currentPath))
        {
            dialog.InitialDirectory = currentPath;
        }

        return dialog.ShowDialog() == true ? dialog.FolderName : null;
    }
}

/// <summary>Resultaat van <see cref="ProjectSettingsViewModel.ConfirmDiscardChangesAsync"/> —
/// zie die methode voor de precieze betekenis van elke waarde.</summary>
public enum UnsavedChangesDecision
{
    Proceed,
    Abort,
    AlreadyClosing,
}
