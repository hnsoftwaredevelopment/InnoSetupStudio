using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InnoSetupStudio.App.Localization;
using InnoSetupStudio.Core.Project;
using Microsoft.Win32;

namespace InnoSetupStudio.App.ViewModels.Screens;

/// <summary>
/// Info After-pagina: laat de gebruiker een leesmij-/infobestand (.txt of .rtf) kiezen dat na de
/// bestemmingspagina en vóór de Voltooid-pagina wordt getoond (Inno Setup's
/// <c>InfoAfterFile</c>-richtlijn). Verder functioneel identiek aan
/// <see cref="InfoBeforePageEditorViewModel"/>; zie die klasse voor de toelichting bij elk veld.
/// </summary>
public sealed partial class InfoAfterPageEditorViewModel : WizardScreenEditorViewModel
{
    private readonly string? _projectFilePath;
    private readonly IProjectAssetService _assetService;

    public InfoAfterPageEditorViewModel(string infoFilePath, string? projectFilePath, IProjectAssetService assetService)
        : base("ShowInfoAfterPage", LocalizationManager.Instance["WizardScreenInfoAfter"], "Document")
    {
        _projectFilePath = projectFilePath;
        _assetService = assetService;
        _infoFilePath = infoFilePath;
        _infoText = LoadInfoText(infoFilePath);
    }

    [ObservableProperty]
    private string _infoFilePath;

    [ObservableProperty]
    private string _infoText;

    partial void OnInfoFilePathChanged(string value) => InfoText = LoadInfoText(value);

    [RelayCommand]
    private void Browse()
    {
        var dialog = new OpenFileDialog
        {
            Filter = LocalizationManager.Instance["DialogFilterInfoFiles"],
        };

        if (!string.IsNullOrWhiteSpace(InfoFilePath))
        {
            dialog.InitialDirectory = Path.GetDirectoryName(InfoFilePath);
        }

        if (dialog.ShowDialog() == true)
        {
            InfoFilePath = _assetService.Import(_projectFilePath, dialog.FileName);
        }
    }

    private static string LoadInfoText(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || IsUncOrDevicePath(path))
        {
            return LocalizationManager.Instance["ScreenEditorInfoAfterNoFile"];
        }

        try
        {
            return File.ReadAllText(path);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return LocalizationManager.Instance["ScreenEditorInfoAfterNoFile"];
        }
    }

    // Zelfde beveiliging als LicensePageEditorViewModel.IsUncOrDevicePath / InfoBeforePageEditorViewModel.
    private static bool IsUncOrDevicePath(string path) => path.StartsWith(@"\\", StringComparison.Ordinal);
}
