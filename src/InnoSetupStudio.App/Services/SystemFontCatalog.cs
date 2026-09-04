using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace InnoSetupStudio.App.Services;

/// <summary>
/// Namen van de lettertypen die op deze machine geïnstalleerd zijn, voor de lettertype-
/// keuzelijsten in de schermeditor (backlogitem 3, sectie 14). Herbert vroeg hier expliciet om
/// (2026-09-04): zelf een lettertypenaam intikken is foutgevoelig, een keuzelijst voorkomt
/// typefouten. Geeft de systeemfamilies van deze machine terug — er is geen andere praktische
/// bron; welke lettertypen op de doelmachine tijdens de installatie beschikbaar zijn is sowieso
/// niet vooraf te kennen, zie WizardScreenButtonSettings over Font.Name als naamverwijzing.
/// </summary>
public static class SystemFontCatalog
{
    public static IReadOnlyList<string> FontFamilyNames { get; } = Fonts.SystemFontFamilies
        .Select(family => family.Source)
        .Distinct()
        .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
        .ToList();
}
