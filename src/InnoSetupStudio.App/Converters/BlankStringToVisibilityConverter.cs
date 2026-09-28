using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace InnoSetupStudio.App.Converters;

/// <summary>
/// Visible zodra de gebonden tekst leeg of enkel witruimte is, anders Collapsed. Gebruikt door de
/// Placeholder.Text-spooktekst in Styles.xaml (gewone TextBox): een veld met uitsluitend spaties
/// telt hiermee ook als "leeg", net als ResolveCaption in DefaultScreenEditorViewModel al deed -
/// zonder deze converter bleef de spooktekst verborgen achter een niet-lege maar inhoudsloze
/// waarde (CodeRabbit-opmerking op PR #16, 2026-09-28).
/// </summary>
public sealed class BlankStringToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        string.IsNullOrWhiteSpace(value as string) ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
