using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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

    public ProjectSettingsViewModel(InstallerProject project, IInstallerProjectService projectService, string? projectFilePath)
    {
        _projectService = projectService;
        _projectFilePath = projectFilePath;

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
    }

    /// <summary>Wordt gevuld zodra <see cref="SaveCommand"/> succesvol heeft opgeslagen, zodat de
    /// aanroepende code (MainWindow) weet welk projectbestand actief is geworden.</summary>
    public string? SavedProjectFilePath { get; private set; }

    /// <summary>Vuurt wanneer het venster moet sluiten: true bij Opslaan, false bij Annuleren.</summary>
    public event EventHandler<bool>? RequestClose;

    [ObservableProperty]
    private string? _projectFilePath;

    // AppId wordt één keer gegenereerd bij het aanmaken van een project (zie
    // InstallerProject.CreateNew) en blijft daarna vast: een gewijzigd AppId laat Inno Setup een
    // eerdere installatie niet meer herkennen. Vandaar alleen-lezen in de UI.
    [ObservableProperty]
    private string _appId = string.Empty;

    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
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
            Filter = "Icon-bestanden (*.ico)|*.ico|Alle bestanden (*.*)|*.*",
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

    private bool CanSave() => !string.IsNullOrWhiteSpace(AppName);

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        var targetPath = ProjectFilePath;
        if (string.IsNullOrWhiteSpace(targetPath))
        {
            var dialog = new SaveFileDialog
            {
                Filter = "Inno Setup Studio-project (*.issproj)|*.issproj",
                FileName = string.IsNullOrWhiteSpace(AppName) ? "Nieuw project" : AppName,
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
        };

        await _projectService.SaveAsync(targetPath, project);

        SavedProjectFilePath = targetPath;
        RequestClose?.Invoke(this, true);
    }

    [RelayCommand]
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
