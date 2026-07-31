using System;
using System.Text.Json;

namespace BabaPlay.Web.Serialization;

internal static class EnumStringJsonHelper
{
    public static string ReadEnumString<TEnum>(ref Utf8JsonReader reader)
        where TEnum : struct, Enum
    {
        if (reader.TokenType == JsonTokenType.Number)
        {
            var numericValue = reader.GetInt32();
            if (Enum.IsDefined(typeof(TEnum), numericValue))
                return Enum.GetName(typeof(TEnum), numericValue)!;

            throw new JsonException($"Unknown {typeof(TEnum).Name} value: {numericValue}.");
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            var rawValue = reader.GetString();
            if (string.IsNullOrWhiteSpace(rawValue))
                return Enum.GetName(typeof(TEnum), 0)!;

            if (Enum.TryParse<TEnum>(rawValue, ignoreCase: true, out var parsedByName))
                return Enum.GetName(typeof(TEnum), parsedByName)!;

            if (int.TryParse(rawValue, out var numericFromString) && Enum.IsDefined(typeof(TEnum), numericFromString))
                return Enum.GetName(typeof(TEnum), numericFromString)!;

            throw new JsonException($"Unknown {typeof(TEnum).Name} value: '{rawValue}'.");
        }

        throw new JsonException(
            $"Unexpected token {reader.TokenType} when parsing {typeof(TEnum).Name} as string.");
    }
}
