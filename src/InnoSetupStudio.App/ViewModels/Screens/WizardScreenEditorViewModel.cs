using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace InnoSetupStudio.App.ViewModels.Screens;

/// <summary>
/// Basisklasse voor een enkel scherm in de schermeditor (fase 4). Eén instantie is tegelijk de
/// DataContext voor drie plekken: de rij in de linkerlijst van <see cref="WizardEditorViewModel"/>,
/// de voorvertoning in het midden (een DataTemplate in WizardEditorWindow kiest op basis van het
/// type de bijbehorende UserControl uit InnoSetupStudio.Wizard) en het instellingenpaneel rechts
/// (een DataTemplate die in InnoSetupStudio.App zelf staat, omdat die de LocalizationManager
/// gebruikt). <see cref="WizardEditorViewModel"/> abonneert zich op PropertyChanged van elke
/// instantie om zijn eigen dirty-status bij te houden, hetzelfde patroon als
/// <see cref="WizardScreensViewModel"/> gebruikt voor zijn rijen in fase 3.
/// </summary>
public abstract class WizardScreenEditorViewModel : ObservableObject
{
    protected WizardScreenEditorViewModel(string id, string title, string iconKey)
    {
        Id = id;
        Title = title;
        IconKey = iconKey;
    }

    /// <summary>Komt overeen met de bijbehorende Show*Page-eigenschap in WizardScreenSelection.</summary>
    public string Id { get; }

    /// <summary>Vertaalde naam, getoond in de linkerlijst.</summary>
    public string Title { get; }

    /// <summary>Iconsleutel uit Icons.xaml, getoond naast de naam in de linkerlijst.</summary>
    public string IconKey { get; }

    // De twee wizardafbeeldingen staan hier op de basisklasse (in plaats van alleen op de
    // schermen die ze nodig hebben) omdat het projectbrede instellingen zijn (Inno Setup's
    // WizardImageFile/WizardSmallImageFile), niet iets per scherm: WizardEditorViewModel bepaalt
    // ze één keer bij het openen van de schermeditor (zie WizardImageResolver) en geeft ze aan elk
    // scherm door, zodat een toekomstig scherm dat ze nodig heeft ze automatisch al beschikbaar
    // heeft. Alleen-lezen: binnen één schermeditor-sessie wijzigen deze niet, ze veranderen pas
    // wanneer de gebruiker in de projectinstellingen een andere afbeelding kiest en de
    // schermeditor opnieuw opent.
    public required ImageSource WizardImage { get; init; }

    /// <summary>Zie <see cref="WizardImage"/>, maar dan de kleine afbeelding rechtsboven.</summary>
    public required ImageSource WizardSmallImage { get; init; }
}
