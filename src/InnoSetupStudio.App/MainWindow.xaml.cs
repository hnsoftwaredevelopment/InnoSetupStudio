using System.IO;
using System.Reflection;
using System.Windows;
using InnoSetupStudio.App.ViewModels;
using InnoSetupStudio.App.Views;
using InnoSetupStudio.App.Localization;
using InnoSetupStudio.Core.Generation;
using InnoSetupStudio.Core.Project;
using Microsoft.Win32;

namespace InnoSetupStudio.App;

public partial class MainWindow : Window
{
    private readonly IInstallerProjectService _projectService = new JsonInstallerProjectService();
    private readonly IProjectAssetService _assetService = new ProjectAssetService();

    // Bijgehouden zodra een project succesvol is opgeslagen via ProjectSettingsWindow, zodat
    // toekomstige functionaliteit (zoals "Installer bouwen") weet welk project actief is zonder
    // het bestand opnieuw van schijf te hoeven laden.
    private InstallerProject? _activeProject;
    private string? _activeProjectFilePath;

    // Eén gedeelde vergrendeling voor SaveActiveProjectAsync: ScreenEditor_SaveClicked en
    // OpenProjectSettings (via ProjectSettingsButton/Nieuw/Openen) roepen elk _projectService.
    // SaveAsync aan voor hetzelfde _activeProjectFilePath. Zie de oorspronkelijke bug (CodeRabbit,
    // PR #18): zonder gedeelde lock kon een save tijdens een lopende andere save gelijktijdig naar
    // hetzelfde .tmp-tijdelijke bestand schrijven.
    private readonly SemaphoreSlim _saveLock = new(1, 1);

    public MainWindow()
    {
        InitializeComponent();

        var informationalVersion = Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        VersionText.Text = string.IsNullOrWhiteSpace(informationalVersion) ? string.Empty : $"v{informationalVersion}";
    }

    private async void NewProjectButton_Click(object sender, RoutedEventArgs e)
    {
        if (!await ConfirmDiscardUnsavedScreenChangesAsync())
        {
            return;
        }

        OpenProjectSettings(InstallerProject.CreateNew(), projectFilePath: null);
    }

    private async void OpenProjectButton_Click(object sender, RoutedEventArgs e)
    {
        if (!await ConfirmDiscardUnsavedScreenChangesAsync())
        {
            return;
        }

        var dialog = new OpenFileDialog
        {
            Filter = LocalizationManager.Instance["DialogFilterProjectFiles"],
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        InstallerProject project;
        try
        {
            project = await _projectService.LoadAsync(dialog.FileName);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Inno Setup Studio", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        // Een geopend (dus al bestaand, geldig opgeslagen) project is meteen het actieve project,
        // ook als de gebruiker het zojuist geopende projectinstellingen-scherm annuleert: die
        // annulering betekent alleen dat de algemene instellingen niet gewijzigd zijn, niet dat
        // het project niet meer "open" is.
        SetActiveProject(project, dialog.FileName);

        OpenProjectSettings(project, dialog.FileName);
    }

    // Nieuw sinds sectie 21: voorheen was er geen weg terug in Projectinstellingen voor een al
    // actief project zonder het opnieuw te openen — dit scherm opende alleen automatisch direct
    // na Nieuw/Openen.
    private async void ProjectSettingsButton_Click(object sender, RoutedEventArgs e)
    {
        if (_activeProject is null)
        {
            return;
        }

        // Dit is exact het CodeRabbit-scenario (PR #19, bevinding #3, zie
        // ConfirmDiscardUnsavedScreenChangesAsync hieronder): Projectinstellingen heropenen voor
        // hetzelfde, al actieve project bouwt via OpenProjectSettings -> SetActiveProject een
        // gehele nieuwe WizardEditorViewModel, ongeacht of de huidige nog niet-opgeslagen
        // schermwijzigingen had.
        if (!await ConfirmDiscardUnsavedScreenChangesAsync())
        {
            return;
        }

        OpenProjectSettings(_activeProject, _activeProjectFilePath);
    }

    /// <summary>
    /// Stap 3 van docs/Ontwerp-Dunne-Generator.md: genereert het .iss van het actieve project. Bij
    /// fouten (ontbrekende verplichte gegevens) wordt er geen bestand geschreven en toont het
    /// resultaatvenster alleen de meldingen. Anders vraagt een SaveFileDialog waar het script moet
    /// komen (standaard naast het .issproj) en toont het resultaatvenster daarna waar het staat plus
    /// de waarschuwingen en info van de generator. Het .iss is altijd een gegenereerd bestand: de
    /// koptekst zegt dat handmatige wijzigingen verloren gaan bij opnieuw genereren.
    /// </summary>
    private async void GenerateScriptButton_Click(object sender, RoutedEventArgs e)
    {
        if (_activeProject is null)
        {
            return;
        }

        var project = _activeProject;

        // Niet-opgeslagen wijzigingen in de schermeditor zitten nog niet in het projectobject (die
        // worden pas bij Opslaan teruggeschreven). Vraag dus eerst of ze mee moeten. Bij "Nee" is het
        // script van de laatst opgeslagen versie.
        if (ScreenEditor.ViewModel is { IsDirty: true } viewModel)
        {
            var answer = MessageBox.Show(
                this,
                LocalizationManager.Instance["GenerateUnsavedMessage"],
                LocalizationManager.Instance["UnsavedChangesTitle"],
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question);

            if (answer == MessageBoxResult.Cancel)
            {
                return;
            }

            if (answer == MessageBoxResult.Yes)
            {
                // Zelfde volgorde als ScreenEditor_SaveClicked: IsDirty pas na een geslaagde save false.
                viewModel.ApplyTo(project);
                viewModel.IsDirty = false;
                if (!await SaveActiveProjectAsync())
                {
                    viewModel.IsDirty = true;
                    return;
                }
            }
        }

        GenerationResult result;
        GenerateScriptButton.IsEnabled = false;
        try
        {
            // De generator controleert of mappen en bestanden bestaan; op een netwerkschijf kan dat
            // even duren, dus niet op de UI-thread.
            result = await Task.Run(() => new IssGenerator().Generate(project));
        }
        finally
        {
            GenerateScriptButton.IsEnabled = _activeProject is not null;
        }

        if (result.HasErrors)
        {
            new GenerationResultWindow(result, writtenPath: null) { Owner = this }.ShowDialog();
            return;
        }

        var dialog = new SaveFileDialog
        {
            Filter = LocalizationManager.Instance["DialogFilterScriptFiles"],
            DefaultExt = ".iss",
            AddExtension = true,
            OverwritePrompt = true,
        };

        if (!string.IsNullOrWhiteSpace(_activeProjectFilePath))
        {
            dialog.InitialDirectory = Path.GetDirectoryName(_activeProjectFilePath);
            dialog.FileName = Path.GetFileNameWithoutExtension(_activeProjectFilePath) + ".iss";
        }

        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        try
        {
            // UTF-8 met BOM, zoals IssGenerator.ScriptEncoding voorschrijft.
            await File.WriteAllTextAsync(dialog.FileName, result.Script, IssGenerator.ScriptEncoding);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            MessageBox.Show(
                this,
                string.Format(LocalizationManager.Instance["GenerateWriteFailedFormat"], ex.Message),
                "Inno Setup Studio",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return;
        }

        new GenerationResultWindow(result, dialog.FileName) { Owner = this }.ShowDialog();
    }

    private void SettingsButton_Click(object sender, RoutedEventArgs e) =>
        new SettingsWindow { Owner = this }.ShowDialog();

    private void OpenProjectSettings(InstallerProject project, string? projectFilePath)
    {
        var viewModel = new ProjectSettingsViewModel(project, _projectService, projectFilePath);
        var window = new ProjectSettingsWindow(viewModel) { Owner = this };

        if (window.ShowDialog() == true)
        {
            // Alleen bij een succesvolle Opslaan (DialogResult true) zijn SavedProject en
            // SavedProjectFilePath gevuld. SavedProject bevat nu ook de eventueel gewijzigde
            // schermselectie/talenselectie (sectie 21, tabbladen Schermen/Talen), dus de
            // schermeditor hieronder moet zich daarop verversen.
            SetActiveProject(viewModel.SavedProject, viewModel.SavedProjectFilePath);
        }
        else if (!string.IsNullOrWhiteSpace(projectFilePath))
        {
            // Een al bestaand project sluit dit scherm via de knop die nu "Openen" heet in plaats
            // van "Annuleren" (zie ProjectSettingsViewModel.CancelButtonText): het project wordt
            // dan niet verworpen, het blijft gewoon actief met de instellingen zoals ze op schijf
            // stonden vóór dit scherm werd geopend. Alleen bij een nieuw, nog niet opgeslagen
            // project (projectFilePath null) betekent Annuleren wél het project verwerpen, dus
            // blijft er dan geen actief project achter.
            SetActiveProject(project, projectFilePath);
        }
    }

    // Sectie 21: de schermeditor (voorheen WizardEditorWindow) is nu permanent zichtbaar in
    // plaats van een venster dat bij elke klik opnieuw werd geconstrueerd. Een nieuwe
    // WizardEditorViewModel wordt daarom alleen op deze grenzen gebouwd — nieuw/geopend project,
    // of na een Opslaan in Projectinstellingen die de schermselectie kan hebben gewijzigd — niet
    // bij elke bewerking binnen de schermeditor zelf (dat zou halverwege typen de selectie en
    // invoer resetten).
    private void SetActiveProject(InstallerProject? project, string? projectFilePath)
    {
        _activeProject = project;
        _activeProjectFilePath = projectFilePath;
        SetProjectActionButtonsEnabled(_activeProject is not null);

        if (_activeProject is null)
        {
            ScreenEditor.Visibility = Visibility.Collapsed;
            WelcomeText.Visibility = Visibility.Visible;
            return;
        }

        ScreenEditor.ViewModel = new WizardEditorViewModel(_activeProject, _activeProjectFilePath, _assetService);
        ScreenEditor.Visibility = Visibility.Visible;
        WelcomeText.Visibility = Visibility.Collapsed;
    }

    /// <summary>
    /// Vraagt de gebruiker om niet-opgeslagen wijzigingen in de schermeditor op te slaan of te
    /// verwerpen, vlak vóórdat een aanroep die op <see cref="SetActiveProject"/> uitkomt (Nieuw,
    /// Openen, of Projectinstellingen heropenen) de huidige — mogelijk gewijzigde — schermeditor
    /// zonder waarschuwing zou vervangen door een gehele nieuwe <see cref="WizardEditorViewModel"/>.
    /// Zie CodeRabbit-bevinding #3, PR #19 (docs/Architectuur-en-Ontwerp.md): dat gebeurde
    /// voorheen onvoorwaardelijk, ongeacht IsDirty. Bewust hier, per aanroeppad
    /// (NewProjectButton_Click, OpenProjectButton_Click, ProjectSettingsButton_Click) vóór
    /// SetActiveProject afgevangen, in plaats van binnen SetActiveProject zelf:
    /// ProjectSettingsWindow is modaal, dus tussen zo'n aanroep en de daaropvolgende
    /// SetActiveProject-aanroepen in OpenProjectSettings kan de schermeditor niet alsnog dirty
    /// worden — één controlepunt per gebruikersactie volstaat.
    /// </summary>
    /// <returns>
    /// True als de aanroeper door mag gaan: er was niets te verliezen (geen actief project, of de
    /// schermeditor was niet dirty), de wijzigingen zijn succesvol opgeslagen, of de gebruiker
    /// koos expliciet voor verwerpen. False als de gebruiker annuleerde, of het opslaan zelf
    /// mislukte (dan is de foutmelding al getoond) — de aanroeper moet de actie dan afbreken en
    /// het actieve project/de schermeditor ongewijzigd laten.
    /// </returns>
    private async Task<bool> ConfirmDiscardUnsavedScreenChangesAsync()
    {
        if (_activeProject is null || ScreenEditor.ViewModel is not { IsDirty: true } viewModel)
        {
            return true;
        }

        var result = MessageBox.Show(
            this,
            LocalizationManager.Instance["UnsavedScreenChangesMessage"],
            LocalizationManager.Instance["UnsavedChangesTitle"],
            MessageBoxButton.YesNoCancel,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Cancel)
        {
            return false;
        }

        if (result == MessageBoxResult.No)
        {
            return true;
        }

        // MessageBoxResult.Yes: zelfde volgorde als ScreenEditor_SaveClicked hieronder — IsDirty
        // pas na een geslaagde save op false, anders zet een mislukte save (bestand in gebruik,
        // schijf vol) het scherm alsnog ten onrechte op "opgeslagen".
        viewModel.ApplyTo(_activeProject);
        viewModel.IsDirty = false;

        if (await SaveActiveProjectAsync())
        {
            // CodeRabbit (PR #20): ScreenEditor blijft tijdens deze await interactief (in
            // tegenstelling tot ProjectSettingsWindow, waar CanEdit/IsSaving de velden
            // uitschakelt) — typt de gebruiker tijdens het opslaan zelf nog iets, dan zet de
            // normale MarkDirty-route IsDirty hier opnieuw op true. Die nieuwe, niet in deze save
            // meegenomen wijziging mag de aanroeper niet alsnog stilzwijgend weggooien door toch
            // door te gaan met SetActiveProject.
            return !viewModel.IsDirty;
        }

        viewModel.IsDirty = true;
        return false;
    }

    // BuildInstallerButton blijft hier bewust buiten beschouwing (CodeRabbit, PR #19): "Installer
    // bouwen" is nog niet geïmplementeerd — alleen de plek in de bovenbalk was in scope bij sectie
    // 21, niet de bouwlogica zelf (zie docs/Architectuur-en-Ontwerp.md). Zonder deze aanpassing
    // werd de knop bij een actief project wel klikbaar, maar deed hij niets: er is geen
    // Click-handler aan gekoppeld. Blijft IsEnabled="False" (zie MainWindow.xaml) totdat die
    // functionaliteit er daadwerkelijk is.
    private void SetProjectActionButtonsEnabled(bool enabled)
    {
        ProjectSettingsButton.IsEnabled = enabled;
        GenerateScriptButton.IsEnabled = enabled;
    }

    // Inline Opslaan-knop van ScreenEditorControl (sectie 21, vervangt WizardEditorWindow's
    // Save/Cancel-dialoogbalk): schrijft de bewerkte velden terug naar _activeProject en slaat
    // op, precies zoals ScreenEditorButton_Click dat voorheen deed ná een geslaagde ShowDialog.
    // Bouwt hier bewust GEEN nieuwe WizardEditorViewModel — zie SetActiveProject hierboven.
    private async void ScreenEditor_SaveClicked(object? sender, EventArgs e)
    {
        if (_activeProject is null || ScreenEditor.ViewModel is not { } viewModel)
        {
            return;
        }

        // Volgorde is hier van belang (CodeRabbit, PR #19): IsDirty gaat al vóór de await naar
        // false, zodat een wijziging die de gebruiker tijdens het opslaan zelf nog typt via de
        // normale MarkDirty-route opnieuw IsDirty=true zet — die staat dan niet stilletjes als
        // "opgeslagen" terwijl hij niet in dit ApplyTo-moment is meegenomen. Mislukt het opslaan
        // zelf, dan wordt het scherm hieronder alsnog expliciet weer vuil gemaakt: een mislukte
        // save mag nooit als "opgeslagen" ogen.
        viewModel.ApplyTo(_activeProject);
        viewModel.IsDirty = false;

        if (!await SaveActiveProjectAsync())
        {
            viewModel.IsDirty = true;
        }
    }

    /// <summary>
    /// Slaat <see cref="_activeProject"/> op naar <see cref="_activeProjectFilePath"/>, als er een
    /// bestandspad is (bij een nog niet opgeslagen nieuw project is er niets te doen). Gedeeld
    /// door ScreenEditor_SaveClicked en OpenProjectSettings: die twee schakelden voorheen (vóór
    /// sectie 21, als losse dialoogvensters) elk alleen hun eigen knop uit tijdens het opslaan,
    /// waardoor een klik op een andere actie tijdens de lopende await gelijktijdig naar hetzelfde
    /// .tmp-tijdelijke bestand kon schrijven (CodeRabbit, PR #18). <see cref="_saveLock"/>
    /// serialiseert dat.
    /// </summary>
    /// <returns>
    /// True als het opslaan gelukt is (of als er niets op te slaan was); false als
    /// <see cref="_projectService"/>.SaveAsync een fout gooide (die dan al als MessageBox is
    /// getoond) — de aanroeper gebruikt dit om IsDirty niet ten onrechte op false te zetten na een
    /// mislukte save (CodeRabbit, PR #19).
    /// </returns>
    private async Task<bool> SaveActiveProjectAsync()
    {
        if (_activeProject is null || string.IsNullOrWhiteSpace(_activeProjectFilePath))
        {
            return true;
        }

        await _saveLock.WaitAsync();
        try
        {
            SetProjectActionButtonsEnabled(false);
            await _projectService.SaveAsync(_activeProjectFilePath, _activeProject);
            return true;
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Inno Setup Studio", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }
        finally
        {
            SetProjectActionButtonsEnabled(_activeProject is not null);
            _saveLock.Release();
        }
    }
}
