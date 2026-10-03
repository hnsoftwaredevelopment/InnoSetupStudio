using System.Windows.Controls;

namespace InnoSetupStudio.Wizard.Screens;

/// <summary>
/// Voorvertoning van de Klaar-om-te-installeren-pagina. Geen eigen logica: alle tekst komt via
/// binding uit de DataContext die de schermeditor (InnoSetupStudio.App) meegeeft
/// (ReadyPageEditorViewModel).
/// </summary>
public partial class ReadyPagePreview : UserControl
{
    public ReadyPagePreview() => InitializeComponent();
}
