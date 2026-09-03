using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InnoSetupStudio.App.Localization;
using Microsoft.Win32;

namespace InnoSetupStudio.App.ViewModels.Screens;

/// <summary>
/// Licentiepagina: laat de gebruiker een licentiebestand (.txt of .rtf) kiezen; de voorvertoning
/// toont de inhoud ervan. Bewuste vereenvoudiging: een .rtf-bestand wordt hier als platte tekst
/// getoond (inclusief de RTF-opmaakcodes), geen echte RTF-rendering — Inno Setup zelf toont een
/// .rtf bestand wel opgemaakt. Voor een eerste versie van de schermeditor is dat acceptabel: de
/// meeste licentiebestanden zijn platte tekst.
/// </summary>
public sealed partial class LicensePageEditorViewModel : WizardScreenEditorViewModel
{
    public LicensePageEditorViewModel(string licenseFilePath)
        : base("ShowLicensePage", LocalizationManager.Instance["WizardScreenLicense"], "Document")
    {
        _licenseFilePath = licenseFilePath;
        _licenseText = LoadLicenseText(licenseFilePath);
    }

    [ObservableProperty]
    private string _licenseFilePath;

    [ObservableProperty]
    private string _licenseText;

    partial void OnLicenseFilePathChanged(string value) => LicenseText = LoadLicenseText(value);

    [RelayCommand]
    private void Browse()
    {
        var dialog = new OpenFileDialog
        {
            Filter = LocalizationManager.Instance["DialogFilterLicenseFiles"],
        };

        if (!string.IsNullOrWhiteSpace(LicenseFilePath))
        {
            dialog.InitialDirectory = Path.GetDirectoryName(LicenseFilePath);
        }

        if (dialog.ShowDialog() == true)
        {
            LicenseFilePath = dialog.FileName;
        }
    }

    private static string LoadLicenseText(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return LocalizationManager.Instance["ScreenEditorLicenseNoFile"];
        }

        try
        {
            return File.ReadAllText(path);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Bestand (nog) niet leesbaar, bijvoorbeeld net verwijderd of vergrendeld: geen
            // uitzondering laten doorsijpelen naar de UI, gewoon de "geen bestand"-tekst tonen.
            return LocalizationManager.Instance["ScreenEditorLicenseNoFile"];
        }
    }
}
