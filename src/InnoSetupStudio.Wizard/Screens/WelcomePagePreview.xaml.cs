using System.Windows.Controls;

namespace InnoSetupStudio.Wizard.Screens;

/// <summary>
/// Voorvertoning van de Welkomstpagina. Geen eigen logica: alle tekst komt via binding uit de
/// DataContext die de schermeditor (InnoSetupStudio.App) meegeeft (WelcomePageEditorViewModel).
/// </summary>
public partial class WelcomePagePreview : UserControl
{
    public WelcomePagePreview() => InitializeComponent();
}
