using CommunityToolkit.Mvvm.Input;
using InnoSetupStudio.Core.Project;

namespace InnoSetupStudio.App.ViewModels;

/// <summary>
/// ViewModel voor het talenoverzicht (backlogitem 4, sectie 14): één rij per taal uit
/// <see cref="InnoLanguageCatalog"/>, met een vinkje om de taal in de installer aan te bieden.
/// Zelfde opzet als <see cref="WizardScreensViewModel"/> (losse rij-objecten in plaats van eigen
/// [ObservableProperty]'s, geabonneerd op PropertyChanged van elke rij om MarkDirty aan te
/// roepen), maar zonder icoon per rij: talen hebben geen herkenningspictogram zoals
/// wizardschermen dat wel hebben.
/// </summary>
public sealed partial class LanguagesViewModel : DirtyTrackingViewModel
{
    private readonly List<string> _unknownLanguageIds;

    public LanguagesViewModel(IReadOnlyList<string> supportedLanguageIds)
    {
        // Bewaard om in ToSelection() terug te geven: een taal-id die niet (meer) in de catalogus
        // staat (bijvoorbeeld een handmatig bewerkt projectbestand, of een toekomstige wijziging
        // van InnoLanguageCatalog) krijgt hier geen rij en dus geen vinkje. Zonder deze lijst zou
        // Opslaan zo'n onbekende id stilzwijgend laten vallen, ook als de gebruiker niets aan de
        // talenselectie zelf wijzigde (CodeRabbit, PR #18).
        _unknownLanguageIds = supportedLanguageIds
            .Where(id => !InnoLanguageCatalog.Languages.Any(l => l.Id == id))
            .ToList();

        Languages = InnoLanguageCatalog.Languages
            .Select(l => new LanguageRow(l.Id, l.DisplayName, l.IsBuiltIn, supportedLanguageIds.Contains(l.Id)))
            .ToList();

        // Rijen zijn hierboven al met hun beginwaarde aangemaakt (via de constructor-parameter,
        // niet via de property-setter), dus dit abonneren zelf triggert nog geen PropertyChanged
        // en dus ook geen valse dirty-melding — zelfde reden als WizardScreensViewModel.
        foreach (var language in Languages)
        {
            language.PropertyChanged += (_, _) => MarkDirty();
        }
    }

    /// <summary>Engels eerst, daarna de overige talen — zelfde volgorde als
    /// <see cref="InnoLanguageCatalog.Languages"/>.</summary>
    public IReadOnlyList<LanguageRow> Languages { get; }

    /// <summary>Vuurt wanneer het venster moet sluiten: true bij Opslaan, false bij Annuleren.</summary>
    public event EventHandler<bool>? RequestClose;

    [RelayCommand]
    private void Save() => RequestClose?.Invoke(this, true);

    [RelayCommand]
    private void Cancel() => RequestClose?.Invoke(this, false);

    /// <summary>Bouwt de nieuwe lijst taal-id's met de huidige vinkjes. Engels staat er altijd in,
    /// ongeacht de staat van zijn (uitgeschakelde) vinkje in de UI. Taal-id's die al in het
    /// project stonden maar niet in <see cref="InnoLanguageCatalog"/> voorkomen (dus geen eigen
    /// rij/vinkje hebben) blijven ongewijzigd behouden — anders zou Opslaan zo'n onbekende id
    /// stilzwijgend laten vallen.</summary>
    public List<string> ToSelection() => Languages
        .Where(l => l.IsLocked || l.IsSelected)
        .Select(l => l.Id)
        .Concat(_unknownLanguageIds)
        .ToList();
}
