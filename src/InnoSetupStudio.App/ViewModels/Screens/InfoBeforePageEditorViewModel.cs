using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InnoSetupStudio.App.Localization;
using InnoSetupStudio.Core.Project;
using Microsoft.Win32;

namespace InnoSetupStudio.App.ViewModels.Screens;

/// <summary>
/// Info Before-pagina: laat de gebruiker een leesmij-/infobestand (.txt of .rtf) kiezen dat vóór
/// de bestemmingspagina wordt getoond (Inno Setup's <c>InfoBeforeFile</c>-richtlijn). Zelfde
/// bewuste vereenvoudiging als de licentiepagina (zie LicensePageEditorViewModel): een .rtf-bestand
/// wordt hier als platte tekst getoond, geen echte RTF-rendering. Anders dan de licentiepagina
/// heeft Inno Setup hier geen "akkoord"-keuzerondjes: dit scherm is puur informatief, met alleen
/// een Volgende-knop.
/// </summary>
public sealed partial class InfoBeforePageEditorViewModel : WizardScreenEditorViewModel
{
    // Zie LicensePageEditorViewModel voor waarom deze twee hier apart staan in plaats van op de
    // basisklasse: InfoFilePath gaat, anders dan een knopbitmap, via IProjectAssetService naar de
    // projectmap.
    private readonly string? _projectFilePath;
    private readonly IProjectAssetService _assetService;

    public InfoBeforePageEditorViewModel(string infoFilePath, string? projectFilePath, IProjectAssetService assetService)
        : base("ShowInfoBeforePage", LocalizationManager.Instance["WizardScreenInfoBefore"], "Document")
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
            // Kopieert het gekozen bestand naar de projectmap zodra het van elders komt, zodat het
            // project zelf verplaatsbaar blijft (zie IProjectAssetService). Bij een nog niet
            // opgeslagen project (_projectFilePath leeg) geeft dit ongewijzigd het gekozen pad
            // terug: er is dan nog geen projectmap om naartoe te kopiëren.
            InfoFilePath = _assetService.Import(_projectFilePath, dialog.FileName);
        }
    }

    private static string LoadInfoText(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || IsUncOrDevicePath(path))
        {
            return LocalizationManager.Instance["ScreenEditorInfoBeforeNoFile"];
        }

        try
        {
            return File.ReadAllText(path);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Bestand (nog) niet leesbaar, bijvoorbeeld net verwijderd of vergrendeld: geen
            // uitzondering laten doorsijpelen naar de UI, gewoon de "geen bestand"-tekst tonen.
            return LocalizationManager.Instance["ScreenEditorInfoBeforeNoFile"];
        }
    }

    // InfoFilePath komt niet alleen uit de eigen bladerdialoog van de gebruiker, maar ook
    // rechtstreeks uit een geladen .issproj-projectbestand. Zonder deze check zou het openen van
    // een projectbestand met een UNC-pad (\\host\share\...) hier automatisch, zonder verdere
    // gebruikersactie, een SMB-verbinding naar die host opzetten. Blokkeer daarom UNC- en
    // apparaatpaden (die beginnen alle met "\\") vóór elke bestandstoegang. Zelfde beveiliging als
    // LicensePageEditorViewModel.IsUncOrDevicePath.
    private static bool IsUncOrDevicePath(string path) => path.StartsWith(@"\\", StringComparison.Ordinal);
}
