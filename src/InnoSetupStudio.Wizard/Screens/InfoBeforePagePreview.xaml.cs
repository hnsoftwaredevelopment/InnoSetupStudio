using System.Windows.Controls;

namespace InnoSetupStudio.Wizard.Screens;

/// <summary>
/// Voorvertoning van de Info Before-pagina. Geen eigen logica: alle tekst komt via binding uit de
/// DataContext die de schermeditor (InnoSetupStudio.App) meegeeft (InfoBeforePageEditorViewModel).
/// </summary>
public partial class InfoBeforePagePreview : UserControl
{
    public InfoBeforePagePreview() => InitializeComponent();
}
