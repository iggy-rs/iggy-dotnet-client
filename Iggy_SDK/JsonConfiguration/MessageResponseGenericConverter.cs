using System.Text.Json;
using System.Text.Json.Serialization;
using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Extensions;
using Iggy_SDK.Headers;

namespace Iggy_SDK.JsonConfiguration;

public sealed class MessageResponseGenericConverter<TMessage> : JsonConverter<IReadOnlyList<MessageResponse<TMessage>>>
{
	private readonly Func<byte[], TMessage> _serializer;
	private readonly Func<byte[], byte[]>? _decryptor;

	public MessageResponseGenericConverter(Func<byte[], TMessage> serializer, Func<byte[], byte[]>? decryptor)
	{
		_serializer = serializer;
		_decryptor = decryptor;
	}
	public override IReadOnlyList<MessageResponse<TMessage>>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var messageResponses = new List<MessageResponse<TMessage>>();
		using var doc = JsonDocument.ParseValue(ref reader);
		
		var root = doc.RootElement;
		foreach (var element in root.EnumerateArray())
		{
			var offset = element.GetProperty(nameof(MessageResponse.Offset).ToSnakeCase()).GetUInt64();
			var timestamp = element.GetProperty(nameof(MessageResponse.Timestamp).ToSnakeCase()).GetUInt64();
			var id = element.GetProperty(nameof(MessageResponse.Id).ToSnakeCase()).GetUInt128();
			var payload = element.GetProperty(nameof(MessageResponse.Payload).ToSnakeCase()).GetBytesFromBase64();
			var headersElement = element.GetProperty(nameof(MessageResponse.Headers).ToSnakeCase());

			var headers = new Dictionary<HeaderKey, HeaderValue>();
			if (headersElement.ValueKind != JsonValueKind.Null)
			{
				var headersJsonArray = headersElement.EnumerateObject();
				foreach (var header in headersJsonArray)
				{
					//TODO - look into getting rid of this boxing 
					var headerKey = header.Name;
					var headerObj = header.Value.EnumerateObject();
					var headerKind = headerObj.First().Value.GetString();
					var headerValue = headerObj.Last().Value.GetBytesFromBase64();
					headers.Add(HeaderKey.New(headerKey), new HeaderValue
					{
						Kind = headerKind switch
						{
							"bool" => HeaderKind.Bool,
							"int32" => HeaderKind.Int32,
							"int64" => HeaderKind.Int64,
							"int128" => HeaderKind.Int128,
							"uint32" => HeaderKind.Uint32,
							"uint64" => HeaderKind.Uint64,
							"uint128" => HeaderKind.Uint128,
							"float32" => HeaderKind.Float32,
							"float64" => HeaderKind.Float64,
							"string" => HeaderKind.String,
							"raw" => HeaderKind.Raw,
							_ => throw new ArgumentOutOfRangeException()
						},
						Value = headerValue
					});
				}
			}
			messageResponses.Add(new MessageResponse<TMessage>
			{
				Offset = offset,
				Timestamp = timestamp,
				Id = new Guid(id.GetBytesFromUInt128()), 
				Headers = headers.Count > 0 ? headers : null,
				Message = _decryptor is not null ? _serializer(_decryptor(payload)) : _serializer(payload)
			});
		}

		return messageResponses.AsReadOnly();
	}

	public override void Write(Utf8JsonWriter writer, IReadOnlyList<MessageResponse<TMessage>> value, JsonSerializerOptions options)
	{
		throw new NotImplementedException();
	}
}