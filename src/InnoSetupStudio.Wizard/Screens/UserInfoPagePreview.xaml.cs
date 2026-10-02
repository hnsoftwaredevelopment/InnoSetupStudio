using System.Windows.Controls;

namespace InnoSetupStudio.Wizard.Screens;

/// <summary>
/// Voorvertoning van de User Info-pagina. Geen eigen logica: alle tekst komt via binding uit de
/// DataContext die de schermeditor (InnoSetupStudio.App) meegeeft (UserInfoPageEditorViewModel).
/// </summary>
public partial class UserInfoPagePreview : UserControl
{
    public UserInfoPagePreview() => InitializeComponent();
}
