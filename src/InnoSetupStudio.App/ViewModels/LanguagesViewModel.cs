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
    public LanguagesViewModel(IReadOnlyList<string> supportedLanguageIds)
    {
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
    /// ongeacht de staat van zijn (uitgeschakelde) vinkje in de UI.</summary>
    public List<string> ToSelection() => Languages
        .Where(l => l.IsLocked || l.IsSelected)
        .Select(l => l.Id)
        .ToList();
}
