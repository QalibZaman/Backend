using System.Text.Json;
using System.Text.Json.Serialization;

namespace NettoRetail.API.Infrastructure.JsonConverters;

public class ProductDetailsValueConverter : JsonConverter<object>
{
    public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.String => reader.GetString(),
            JsonTokenType.Number => reader.TryGetDecimal(out decimal l) ? l : JsonSerializer.Deserialize<object>(ref reader, options),
            JsonTokenType.True => true,
            JsonTokenType.False => false,
            _ => JsonSerializer.Deserialize<object>(ref reader, options)
        };
    }

    public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case string s:
                writer.WriteStringValue(s);
                break;
            case long l:
                writer.WriteNumberValue(l);
                break;
            case int i:
                writer.WriteNumberValue(i);
                break;
            case double d:
                writer.WriteNumberValue(d);
                break;
            case bool b:
                writer.WriteBooleanValue(b);
                break;
            default:
                JsonSerializer.Serialize(writer, value, options);
                break;
        }
    }
}