using System.Windows;

namespace InnoSetupStudio.App.Controls;

/// <summary>
/// Bindbare "spooktekst" (watermark) voor TextBox en de editable ComboBox uit Styles.xaml -
/// Herberts feedback (2026-09-28): de instellingenvelden per scherm (Terug-/Volgende-/
/// Annuleren-knop in WizardEditorWindow.xaml) toonden alleen wat dát scherm zelf had ingevuld,
/// nooit wat er via het Standaardscherm (of, op het Standaardscherm zelf, via Inno Setup's eigen
/// ingebouwde tekst) daadwerkelijk gold zolang het eigen veld leeg is. <see cref="TextProperty"/>
/// hier, gezet op zo'n veld (meestal gebonden aan de bijbehorende EffectiveXxx-eigenschap), toont
/// die waarde als lichtgrijze, niet-interactieve tekst zodra het echte, opgeslagen veld leeg is -
/// puur ter info, nooit onderdeel van de opgeslagen waarde zelf.
///
/// Losse attached property in plaats van een eigen UserControl: TextBox/ComboBox blijven de
/// normale WPF-besturingselementen (dezelfde Text-binding als altijd), dit voegt alleen een extra
/// stukje aan hun bestaande ControlTemplate toe (zie Styles.xaml) dat op deze eigenschap let.
/// </summary>
public static class Placeholder
{
    public static readonly DependencyProperty TextProperty = DependencyProperty.RegisterAttached(
        "Text", typeof(string), typeof(Placeholder), new PropertyMetadata(string.Empty));

    public static string GetText(DependencyObject element) => (string)element.GetValue(TextProperty);

    public static void SetText(DependencyObject element, string value) => element.SetValue(TextProperty, value);
}
