using System.Windows.Data;
using System.Windows.Markup;

namespace InnoSetupStudio.App.Localization;

/// <summary>
/// XAML-markup-extensie voor vertaalde teksten: gebruik als <c>{loc:Loc SleutelNaam}</c>.
/// Bindt op de indexer van <see cref="LocalizationManager"/> zodat de tekst live meewisselt met de taal.
/// </summary>
public sealed class LocExtension : MarkupExtension
{
    public LocExtension(string key)
    {
        Key = key;
    }

    public string Key { get; set; }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var binding = new Binding($"[{Key}]")
        {
            Source = LocalizationManager.Instance,
            Mode = BindingMode.OneWay
        };

        return binding.ProvideValue(serviceProvider);
    }
}
