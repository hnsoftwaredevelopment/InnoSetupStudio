using System.Windows.Controls;

namespace InnoSetupStudio.Wizard.Screens;

/// <summary>
/// Voorvertoning van de bestemmingspagina. Geen eigen logica: alle tekst komt via binding uit
/// SelectDestinationPageEditorViewModel (InnoSetupStudio.App).
/// </summary>
public partial class SelectDestinationPagePreview : UserControl
{
    public SelectDestinationPagePreview() => InitializeComponent();
}
