using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace InnoSetupStudio.App.Converters;

/// <summary>
/// Visible zodra de ComboBox bewerkbaar is (IsEditable) EN de tekst leeg of enkel witruimte is,
/// anders Collapsed. Zelfde doel als BlankStringToVisibilityConverter, maar voor de editable
/// ComboBox in Styles.xaml, die naast de Text-waarde ook nog moet checken of PART_EditableTextBox
/// er wel is (IsEditable) voordat de placeholder-spooktekst zin heeft. Waarden: [0] = IsEditable
/// (bool), [1] = Text (string).
/// </summary>
public sealed class EditableBlankToVisibilityConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        var isEditable = values.Length > 0 && values[0] is true;
        var text = values.Length > 1 ? values[1] as string : null;
        return isEditable && string.IsNullOrWhiteSpace(text) ? Visibility.Visible : Visibility.Collapsed;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
