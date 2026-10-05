using System.Windows.Controls;

namespace InnoSetupStudio.Wizard.Screens;

/// <summary>
/// Voorvertoning van de Info After-pagina. Geen eigen logica: alle tekst komt via binding uit de
/// DataContext die de schermeditor (InnoSetupStudio.App) meegeeft (InfoAfterPageEditorViewModel).
/// </summary>
public partial class InfoAfterPagePreview : UserControl
{
    public InfoAfterPagePreview() => InitializeComponent();
}
