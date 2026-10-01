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

        // Dispatcher.BeginInvoke in plaats van DialogResult/Close rechtstreeks aan te roepen:
        // wanneer dit binnenkomt vanuit ConfirmDiscardChangesAsync (Ja/opslaan, of Nee/verwerpen)
        // tijdens een X-klik, bevinden we ons mogelijk nog steeds synchroon binnen WPF's eigen
        // Closing-dispatch (het Nee-pad heeft geen echte I/O-await, dus de async-methode keert na
        // de modale MessageBox-pomp synchroon terug zonder ooit echt naar de dispatcher-wachtrij
        // te zijn gesprongen). Zowel DialogResult zetten als Close() aanroepen terwijl het venster
        // nog "closing" is, gooit dan "Cannot ... Close ... while a Window is closing." (Herbert,
        // 2026-10-01). BeginInvoke stelt beide veilig uit tot de huidige dispatch volledig is
        // afgerond; voor de knop-paden (Opslaan/Annuleren, buiten elke Closing-dispatch) is dat
        // onmerkbaar, één dispatcher-tick later.
        // Resultaat (DispatcherOperation) bewust genegeerd: fire-and-forget is hier precies de
        // bedoeling, niet iets om op te wachten.
        _ = Dispatcher.BeginInvoke(new Action(() =>
        {
            DialogResult = saved;
            Close();
        }));
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
                // Zelfde Dispatcher.BeginInvoke-reden als in OnRequestClose hierboven: dit pad
                // (Nee/verwerpen gekozen op de X-sluit-vraag) heeft geen echte I/O-await gehad, dus
                // we zitten hier nog steeds synchroon binnen WPF's eigen Closing-dispatch.
                _ = Dispatcher.BeginInvoke(new Action(Close));
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
