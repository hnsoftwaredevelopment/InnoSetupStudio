using System.Text.Json;
using System.Text.Json.Serialization;

namespace InnoSetupStudio.Core.Project;

/// <summary>
/// Leest en schrijft een enum als tekstnaam (bijvoorbeeld <c>"X64"</c>) en weigert al het andere:
/// getallen, numerieke tekst zoals <c>"999"</c> en onbekende namen leiden tot een
/// <see cref="JsonException"/>. De standaard <see cref="JsonStringEnumConverter"/> accepteert
/// getallen en zou een ongedefinieerde enumwaarde teruggeven, die later door de generator als
/// onzin in het .iss terecht kan komen. Zelfde strengheid als
/// <see cref="DisablePageModeJsonConverter"/>, maar zonder de verouderde boolean-vorm.
///
/// Een ontbrekende JSON-sleutel komt hier nooit: dan houdt de eigenschap zijn eigen C#-
/// standaardwaarde en wordt deze converter niet aangeroepen.
/// </summary>
/// <typeparam name="TEnum">Het enumtype dat deze converter leest en schrijft.</typeparam>
public sealed class StrictEnumJsonConverter<TEnum> : JsonConverter<TEnum>
    where TEnum : struct, Enum
{
    public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var text = reader.GetString();

            // Eerst tegen de gedefinieerde namen controleren: Enum.TryParse accepteert ook
            // numerieke tekst en geeft dan een ongedefinieerde waarde terug.
            if (text is not null
                && Array.Exists(Enum.GetNames<TEnum>(), name => string.Equals(name, text, StringComparison.OrdinalIgnoreCase))
                && Enum.TryParse<TEnum>(text, ignoreCase: true, out var parsed))
            {
                return parsed;
            }
        }

        throw new JsonException(
            $"Onverwachte waarde voor {typeToConvert.Name}: verwacht een van {string.Join(", ", Enum.GetNames<TEnum>())}.");
    }

    public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString());
}
