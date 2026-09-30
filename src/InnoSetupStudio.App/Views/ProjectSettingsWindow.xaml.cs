using System.ComponentModel;
using System.Windows;
using InnoSetupStudio.App.ViewModels;

namespace InnoSetupStudio.App.Views;

public partial class ProjectSettingsWindow : Window
{
    public ProjectSettingsViewModel ViewModel { get; }

    // Onderscheidt een Close() die via ViewModel.RequestClose (Opslaan- of Annuleren/Openen-knop,
    // die hun eigen niet-opgeslagen-wijzigingen-afweging al gemaakt hebben — zie
    // ProjectSettingsViewModel.CancelAsync) binnenkomt van een echte X-klik op de titelbalk, die
    // als enige nog rechtstreeks bij Closing hieronder terechtkomt.
    private bool _programmaticClose;

    public ProjectSettingsWindow(ProjectSettingsViewModel viewModel)
    {
        InitializeComponent();

        ViewModel = viewModel;
        DataContext = viewModel;
        viewModel.RequestClose += OnRequestClose;
        Closing += ProjectSettingsWindow_Closing;
    }

    private void OnRequestClose(object? sender, bool saved)
    {
        _programmaticClose = true;
        DialogResult = saved;
        Close();
    }

    // Herbert (2026-09-30): X-sluiten van dit venster met niet-opgeslagen wijzigingen (bijvoorbeeld
    // de Applicatienaam getypt zonder op Opslaan te klikken) gooide die wijzigingen stilzwijgend
    // weg, zowel bij een nieuw als een al bestaand project — in tegenstelling tot de
    // Annuleren/Openen-knop (ProjectSettingsViewModel.CancelAsync), die dat al deels afving. X-
    // sluiten loopt buiten CancelCommand om (WPF sluit het venster rechtstreeks zonder een
    // command aan te roepen), dus dit is de enige plek waar dat pad zelf nog afgevangen kan
    // worden. Geldt bewust voor élk project (nieuw of bestaand): anders dan de Annuleren-knop op
    // een nieuw project (waar de klik zelf al de expliciete keuze voor verwerpen is) zegt de
    // titelbalk-X niets over de bedoeling van de gebruiker.
    private async void ProjectSettingsWindow_Closing(object? sender, CancelEventArgs e)
    {
        if (_programmaticClose || !ViewModel.IsDirty)
        {
            return;
        }

        e.Cancel = true;

        var decision = await ViewModel.ConfirmDiscardChangesAsync();
        switch (decision)
        {
            case UnsavedChangesDecision.Proceed:
                _programmaticClose = true;
                Close();
                break;
            case UnsavedChangesDecision.AlreadyClosing:
                // SaveAsync heeft al RequestClose(true) gevuurd (via OnRequestClose hierboven,
                // dat _programmaticClose al zette) — hier niets meer te doen.
                break;
            case UnsavedChangesDecision.Abort:
                // Gebruiker annuleerde, of opslaan kon/lukte niet: venster blijft open, e.Cancel
                // staat al op true.
                break;
        }
    }
}
