using System.Windows.Controls;

namespace InnoSetupStudio.Wizard.Screens;

/// <summary>
/// Voorvertoning van de Select Start Menu Folder-pagina. Geen eigen logica: alle tekst komt via
/// binding uit de DataContext die de schermeditor (InnoSetupStudio.App) meegeeft
/// (SelectProgramGroupPageEditorViewModel).
/// </summary>
public partial class SelectProgramGroupPagePreview : UserControl
{
    public SelectProgramGroupPagePreview() => InitializeComponent();
}
