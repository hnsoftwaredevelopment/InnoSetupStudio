using CommunityToolkit.Mvvm.ComponentModel;
using InnoSetupStudio.App.Localization;

namespace InnoSetupStudio.App.ViewModels;

/// <summary>
/// Basisklasse voor bewerkschermen die moeten bijhouden of de gebruiker daadwerkelijk iets heeft
/// gewijzigd sinds het openen. Zorgt ervoor dat de knop naast Opslaan een label/icoon toont dat
/// overeenkomt met wat hij echt doet: "Sluiten" met een neutraal pijltje als er niets te
/// verliezen valt, "Annuleren" met een kruis zodra er onopgeslagen wijzigingen worden weggegooid.
/// Ontstaan uit het projectinstellingen-scherm (waar "Annuleren" bij een al bestaand project
/// misleidend bleek) en het wizardschermen-scherm; nieuwe bewerkschermen kunnen dezelfde
/// dirty-tracking hergebruiken door hiervan te erven.
/// </summary>
public abstract partial class DirtyTrackingViewModel : ObservableObject
{
    // Zolang een afgeleide klasse tussen BeginInit/EndInit de bewerkbare velden nog vult vanuit
    // het binnenkomende model, mag dat niet als een wijziging door de gebruiker tellen: anders
    // staat Opslaan meteen aan (en toont de knop ernaast meteen "Annuleren") voor een net geopend,
    // ongewijzigd scherm. Protected (niet private) zodat een override van MarkDirty in een
    // afgeleide klasse hier ook zelf op kan controleren vóór die iets extra's doet (zoals het
    // eigen Opslaan-commando herevalueren).
    protected bool IsInitializing { get; private set; }

    [NotifyPropertyChangedFor(nameof(CancelButtonText))]
    [NotifyPropertyChangedFor(nameof(CancelButtonIconKey))]
    [ObservableProperty]
    private bool _isDirty;

    /// <summary>Label voor de knop naast Opslaan. Standaard gebaseerd op <see cref="IsDirty"/>:
    /// "Sluiten" als er niets te verliezen valt, "Annuleren" zodra er onopgeslagen wijzigingen
    /// worden weggegooid. Een afgeleide klasse kan dit overschrijven voor een specifiekere tekst
    /// (bijvoorbeeld "Openen" bij een al bestaand project in <see cref="ProjectSettingsViewModel"/>,
    /// waar niet de dirty-status maar het bestaan van het project bepaalt wat de knop doet).</summary>
    public virtual string CancelButtonText => IsDirty
        ? LocalizationManager.Instance["ButtonCancel"]
        : LocalizationManager.Instance["ButtonClose"];

    /// <summary>Iconsleutel (zie Resources/Icons.xaml, via IconKeyToGeometryConverter) voor de
    /// knop naast Opslaan. Standaard een kruis zodra er wijzigingen worden weggegooid, anders een
    /// neutraal terug-pijltje.</summary>
    public virtual string CancelButtonIconKey => IsDirty ? "Close" : "ArrowLeft";

    /// <summary>Aanroepen vóórdat de bewerkbare velden gevuld worden vanuit het binnenkomende
    /// model, zodat die eerste toewijzingen niet als een wijziging door de gebruiker tellen.</summary>
    protected void BeginInit() => IsInitializing = true;

    /// <summary>Aanroepen nadat de bewerkbare velden gevuld zijn; latere wijzigingen tellen vanaf
    /// dit punt weer gewoon mee voor <see cref="MarkDirty"/>.</summary>
    protected void EndInit() => IsInitializing = false;

    /// <summary>Zet de dirty-vlag zodra de gebruiker daadwerkelijk iets wijzigt (dus niet tussen
    /// <see cref="BeginInit"/> en <see cref="EndInit"/>). Een afgeleide klasse met een eigen
    /// Opslaan-commando overschrijft dit meestal om ook diens <c>CanExecute</c> te laten
    /// herevalueren (en controleert daarbij zelf <see cref="IsInitializing"/> als die extra stap
    /// tijdens het initialiseren overgeslagen moet worden).</summary>
    protected virtual void MarkDirty()
    {
        if (IsInitializing)
        {
            return;
        }

        IsDirty = true;
    }
}
