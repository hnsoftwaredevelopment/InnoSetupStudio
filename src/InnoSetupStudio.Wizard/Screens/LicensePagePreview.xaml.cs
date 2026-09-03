using System.Windows.Controls;

namespace InnoSetupStudio.Wizard.Screens;

/// <summary>
/// Voorvertoning van de licentiepagina. Geen eigen logica: LicenseText komt via binding uit
/// LicensePageEditorViewModel (InnoSetupStudio.App), die het gekozen bestand van schijf leest.
/// </summary>
public partial class LicensePagePreview : UserControl
{
    public LicensePagePreview() => InitializeComponent();
}
