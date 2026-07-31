using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BabaPlay.Web.Serialization;

public sealed class MatchStatusStringConverter : JsonConverter<string>
{
    public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => EnumStringJsonHelper.ReadEnumString<WebMatchStatus>(ref reader);

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        => writer.WriteStringValue(value);
}
