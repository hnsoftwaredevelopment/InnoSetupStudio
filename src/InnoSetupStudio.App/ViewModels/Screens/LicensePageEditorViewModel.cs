using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InnoSetupStudio.App.Localization;
using InnoSetupStudio.Core.Project;
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
    private readonly string? _projectFilePath;
    private readonly IProjectAssetService _assetService;

    public LicensePageEditorViewModel(string licenseFilePath, string? projectFilePath, IProjectAssetService assetService)
        : base("ShowLicensePage", LocalizationManager.Instance["WizardScreenLicense"], "Document")
    {
        _projectFilePath = projectFilePath;
        _assetService = assetService;
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
            // Kopieert het gekozen bestand naar de projectmap zodra het van elders komt, zodat
            // het project zelf verplaatsbaar blijft (zie IProjectAssetService). Bij een nog niet
            // opgeslagen project (_projectFilePath leeg) geeft dit ongewijzigd het gekozen pad
            // terug: er is dan nog geen projectmap om naartoe te kopiëren.
            LicenseFilePath = _assetService.Import(_projectFilePath, dialog.FileName);
        }
    }

    private static string LoadLicenseText(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || IsUncOrDevicePath(path))
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

    // LicenseFilePath komt niet alleen uit de eigen bladerdialoog van de gebruiker, maar ook
    // rechtstreeks uit een geladen .issproj-projectbestand. Zonder deze check zou het openen van
    // een projectbestand met een UNC-pad (\\host\share\...) hier automatisch, zonder verdere
    // gebruikersactie, een SMB-verbinding naar die host opzetten. Blokkeer daarom UNC- en
    // apparaatpaden (die beginnen alle met "\\") vóór elke bestandstoegang.
    private static bool IsUncOrDevicePath(string path) => path.StartsWith(@"\\", StringComparison.Ordinal);
}
