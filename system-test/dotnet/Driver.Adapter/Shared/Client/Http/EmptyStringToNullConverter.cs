using System.Text.Json;
using System.Text.Json.Serialization;

namespace Driver.Adapter.Shared.Client.Http;

public class EmptyStringToNullConverter : JsonConverter<string?>
{
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => reader.GetString();

    public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options)
    {
        if (string.IsNullOrWhiteSpace(value))
            writer.WriteNullValue();
        else
            writer.WriteStringValue(value);
    }
}
