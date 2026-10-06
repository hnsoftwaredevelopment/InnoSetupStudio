using System.Windows;

namespace InnoSetupStudio.Wizard;

/// <summary>
/// De maten van de knoppen onder aan het wizardvenster van Inno Setup, gemeten met Setup 7.1.0 op
/// 96 DPI (docs/Ontwerp-Vaste-Knopbreedte-Voorvertoning.md, sectie 2): Terug, Volgende, Annuleren en
/// de Bladeren-knoppen zijn 75 bij 23 en groeien niet mee met hun tekst. Tussen Volgende en
/// Annuleren zit 10 eenheden ruimte, Terug en Volgende liggen tegen elkaar aan. De voorvertoningen
/// in de App en in dit project gebruiken deze ene plek, zodat een breedte per knop later alleen de
/// vaste waarde hoeft te vervangen.
/// </summary>
public static class SetupButtonMetrics
{
    /// <summary>Breedte van een knop in apparaatonafhankelijke eenheden (pixels op 96 DPI).</summary>
    public const double Width = 75;

    /// <summary>Hoogte van een knop.</summary>
    public const double Height = 23;

    /// <summary>Ruimte tussen Volgende en Annuleren.</summary>
    public const double Gap = 10;

    /// <summary>Breedte van een knop als kolombreedte in een Grid.</summary>
    public static readonly GridLength ColumnWidth = new(Width);

    /// <summary>Breedte van de ruimte tussen Volgende en Annuleren als kolombreedte.</summary>
    public static readonly GridLength GapWidth = new(Gap);

    /// <summary>Lettergrootte van Setup (Segoe UI 9 punten) in eenheden, voor een knop zonder eigen lettergrootte.</summary>
    public const double DefaultFontSize = 12;
}
