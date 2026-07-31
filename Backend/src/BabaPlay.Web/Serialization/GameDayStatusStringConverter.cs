using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BabaPlay.Web.Serialization;

public sealed class GameDayStatusStringConverter : JsonConverter<string>
{
    public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => EnumStringJsonHelper.ReadEnumString<WebGameDayStatus>(ref reader);

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        => writer.WriteStringValue(value);
}
