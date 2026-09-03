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
public sealed partial class ProjectSettingsViewModel : ObservableObject
{
    private readonly IInstallerProjectService _projectService;

    // Bewaard vanuit het project waarmee dit venster is geopend, zodat SaveAsync deze waarde kan
    // meenemen in het opgeslagen project: dit scherm toont en wijzigt alleen de algemene
    // instellingen, dus zonder dit veld zou een simpele naam- of paden-wijziging de elders
    // gekozen wizardschermen-selectie stilzwijgend terugzetten naar de standaardwaarden.
    private readonly WizardScreenSelection _wizardScreens;

    // Zolang dit venster de velden nog vult vanuit het meegegeven project (in de constructor) mag
    // dat niet als een wijziging door de gebruiker tellen: anders staat Opslaan meteen aan voor
    // een net geopend, ongewijzigd project.
    private readonly bool _isInitializing;

    // True zodra de gebruiker daadwerkelijk iets heeft aangepast sinds het openen van dit venster.
    // Opslaan is alleen zinvol (en dus alleen enabled) als hier iets te bewaren valt.
    private bool _isDirty;

    public ProjectSettingsViewModel(InstallerProject project, IInstallerProjectService projectService, string? projectFilePath)
    {
        _projectService = projectService;
        _isInitializing = true;
        _projectFilePath = projectFilePath;
        _wizardScreens = project.WizardScreens;

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

        _isInitializing = false;

        // Bij een al bestaand (opgeslagen) project doet Annuleren feitelijk niets anders dan het
        // venster sluiten zonder de instellingen te wijzigen — het project blijft gewoon actief.
        // "Openen" beschrijft dat beter dan "Annuleren"; bij een nieuw, nog niet opgeslagen
        // project betekent dezelfde knop wel echt het project verwerpen, dus daar blijft
        // "Annuleren" staan.
        IsExistingProject = !string.IsNullOrWhiteSpace(projectFilePath);
        CancelButtonText = IsExistingProject
            ? LocalizationManager.Instance["ButtonOpen"]
            : LocalizationManager.Instance["ButtonCancel"];
    }

    /// <summary>True als dit venster is geopend voor een al bestaand (opgeslagen) project, false
    /// voor een nieuw project. Bepaalt naast <see cref="CancelButtonText"/> ook welk icoon de
    /// knop toont (map-icoon bij Openen, kruis bij Annuleren).</summary>
    public bool IsExistingProject { get; }

    /// <summary>Wordt gevuld zodra <see cref="SaveCommand"/> succesvol heeft opgeslagen, zodat de
    /// aanroepende code (MainWindow) weet welk projectbestand actief is geworden.</summary>
    public string? SavedProjectFilePath { get; private set; }

    /// <summary>Het exacte project zoals het net is opgeslagen, zodat de aanroepende code
    /// (MainWindow) dit als actief project kan bijhouden zonder het bestand opnieuw te hoeven
    /// inlezen.</summary>
    public InstallerProject? SavedProject { get; private set; }

    /// <summary>Vuurt wanneer het venster moet sluiten: true bij Opslaan, false bij Annuleren.</summary>
    public event EventHandler<bool>? RequestClose;

    /// <summary>Label voor de knop naast Opslaan: "Openen" bij een al bestaand project (de knop
    /// sluit dan alleen het venster, het project blijft actief), "Annuleren" bij een nieuw,
    /// nog niet opgeslagen project (de knop verwerpt het dan echt).</summary>
    public string CancelButtonText { get; }

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

    private bool CanSave() => !IsSaving && _isDirty && !string.IsNullOrWhiteSpace(AppName);

    // Eén gedeelde hook voor alle bewerkbare velden: zet de dirty-vlag zodra de gebruiker
    // daadwerkelijk iets wijzigt (dus niet tijdens het vullen van de velden in de constructor) en
    // laat Opslaan meteen herevalueren of het al enabled mag worden.
    private void MarkDirty()
    {
        if (_isInitializing)
        {
            return;
        }

        // Altijd herevalueren, ook als _isDirty al true was: CanSave() controleert behalve de
        // dirty-vlag ook AppName, dus als de gebruiker AppName leegmaakt nadat een ander veld al
        // dirty maakte, moet Opslaan alsnog uitschakelen. Met een vroege return alleen op basis
        // van _isDirty zou die herevaluatie gemist worden.
        _isDirty = true;
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
    private async Task SaveAsync()
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
                return;
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
            WizardScreens = _wizardScreens,
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
            return;
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
        _isDirty = false;
        RequestClose?.Invoke(this, true);
    }

    private bool CanCancel() => !IsSaving;

    [RelayCommand(CanExecute = nameof(CanCancel))]
    private void Cancel() => RequestClose?.Invoke(this, false);

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
