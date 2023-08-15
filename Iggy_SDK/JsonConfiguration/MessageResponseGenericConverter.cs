using System.Text.Json;
using System.Text.Json.Serialization;
using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Extensions;

namespace Iggy_SDK.JsonConfiguration;

public sealed class MessageResponseGenericConverter<TMessage> : JsonConverter<IEnumerable<MessageResponse<TMessage>>>
{
	private readonly Func<byte[], TMessage> _serializer;
	private readonly Func<byte[], byte[]>? _decryptor;

	public MessageResponseGenericConverter(Func<byte[], TMessage> serializer, Func<byte[], byte[]>? decryptor)
	{
		_serializer = serializer;
		_decryptor = decryptor;
	}
	public override IEnumerable<MessageResponse<TMessage>>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		//TODO - mby get rid of this allocation as well 
		var messageResponses = new List<MessageResponse<TMessage>>();
		using var doc = JsonDocument.ParseValue(ref reader);
		
		var root = doc.RootElement;
		foreach (var element in root.EnumerateArray())
		{
			var offset = element.GetProperty(nameof(MessageResponse.Offset).ToSnakeCase()).GetUInt64();
			var timestamp = element.GetProperty(nameof(MessageResponse.Timestamp).ToSnakeCase()).GetUInt64();
			var id = element.GetProperty(nameof(MessageResponse.Id).ToSnakeCase()).GetUInt128();
			var payload = element.GetProperty(nameof(MessageResponse.Payload).ToSnakeCase()).GetBytesFromBase64();

			messageResponses.Add(new MessageResponse<TMessage>
			{
				Offset = offset,
				Timestamp = timestamp,
				Id = new Guid(id.GetBytesFromUInt128()), 
				Message = _decryptor is not null ? _serializer(_decryptor(payload)) : _serializer(payload)
			});
		}

		return messageResponses;
	}

	public override void Write(Utf8JsonWriter writer, IEnumerable<MessageResponse<TMessage>> value, JsonSerializerOptions options)
	{
		throw new NotImplementedException();
	}
}