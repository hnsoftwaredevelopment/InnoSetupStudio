using System.Text.Json;
using System.Text.Json.Serialization;

namespace InnoSetupStudio.Core.Project;

/// <summary>
/// Leest <see cref="DisablePageMode"/> uit JSON in twee vormen: de nieuwe tekstwaarde (bijvoorbeeld
/// <c>"AlwaysShow"</c>) en het oudere boolean-formaat (<c>true</c>/<c>false</c>) dat
/// <see cref="InstallerProject.DirPageMode"/> had vóór de Auto-optie, onder de JSON-sleutel
/// <c>AllowUserToChangeDir</c> (zie die eigenschap's doccomment) — zodat een ouder projectbestand
/// zonder handmatige migratiecode blijft werken: <c>true</c> (gebruiker mocht de map wijzigen)
/// wordt <see cref="DisablePageMode.AlwaysShow"/>, <c>false</c> wordt
/// <see cref="DisablePageMode.NeverShow"/>. Schrijft altijd als tekstwaarde.
/// </summary>
public sealed class DisablePageModeJsonConverter : JsonConverter<DisablePageMode>
{
    public override DisablePageMode Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.True)
        {
            return DisablePageMode.AlwaysShow;
        }

        if (reader.TokenType == JsonTokenType.False)
        {
            return DisablePageMode.NeverShow;
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            var text = reader.GetString();
            // Alleen een gedefinieerde enumnaam accepteren: Enum.TryParse laat ook numerieke tekst
            // zoals "999" toe en zou dan een ongedefinieerde enumwaarde teruggeven (die later weer
            // zou worden weggeschreven). Eerst tegen de namen controleren dus.
            if (text is not null
                && Array.Exists(Enum.GetNames<DisablePageMode>(), name => string.Equals(name, text, StringComparison.OrdinalIgnoreCase))
                && Enum.TryParse<DisablePageMode>(text, ignoreCase: true, out var parsed))
            {
                return parsed;
            }
        }

        // Alleen bereikt bij een handmatig corrupt bewerkt projectbestand (de sleutel is wél
        // aanwezig, maar met een onverwachte waarde) — een ontbrekende sleutel komt hier nooit:
        // dan gebruikt System.Text.Json gewoon de eigenschap's eigen C#-standaardwaarde
        // (AlwaysShow voor DirPageMode, AutoSkipIfKnown voor GroupPageMode) zonder deze converter
        // aan te roepen. Hardop falen is hier veiliger dan stilzwijgend raden.
        throw new JsonException(
            $"Onverwachte waarde voor {typeToConvert.Name}: verwacht true/false of een van " +
            $"{string.Join(", ", Enum.GetNames<DisablePageMode>())}.");
    }

    public override void Write(Utf8JsonWriter writer, DisablePageMode value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString());
}
