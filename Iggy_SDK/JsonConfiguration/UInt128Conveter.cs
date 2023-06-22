using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Iggy_SDK.JsonConfiguration;

public sealed class UInt128Conveter : JsonConverter<UInt128>
{
	public override UInt128 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.Number)
		{
			throw new JsonException();
		}

		return UInt128.Parse(Encoding.UTF8.GetString(reader.ValueSpan));
	}

	public override void Write(Utf8JsonWriter writer, UInt128 value, JsonSerializerOptions options)
	{
		writer.WriteRawValue(value.ToString());
	}
}