using System.Globalization;
using System.Windows.Data;

namespace InnoSetupStudio.App.Converters;

/// <summary>
/// Zet een bool (of null) om naar een Opacity-waarde voor de Terug-/Volgende-knoppen in de
/// schermeditorvoorvertoning: true of null (Inno Setup's eigen standaardgedrag) geeft volledig
/// zichtbaar, expliciet false (gebruiker heeft de knop uitgeschakeld voor dit scherm) geeft een
/// gedimd uiterlijk. Bewust geen echte IsEnabled-binding: Terug/Volgende zijn ook de navigatie van
/// de schermeditor zelf (zie WizardEditorViewModel.Back/Next), dus die moeten altijd klikbaar
/// blijven — dit is puur een visuele voorvertoning van hoe de knop er in de echte installer uit
/// zou zien.
/// </summary>
public sealed class BooleanToOpacityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is false ? 0.4 : 1.0;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
