using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Enums;
using Iggy_SDK.Extensions;
using Iggy_SDK.Messages;

namespace Iggy_SDK.JsonConfiguration;

internal sealed class MessagesConverter : JsonConverter<MessageSendRequest>
{
	public override MessageSendRequest? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		throw new NotImplementedException();
	}

	public override void Write(Utf8JsonWriter writer, MessageSendRequest value, JsonSerializerOptions options)
	{
		if (!value.Messages.Any())
		{
			writer.WriteStringValue("");
		}
		else
		{
			var msgList = new List<HttpMessage>();
			foreach (var message in value.Messages)
			{
				var base64 = Convert.ToBase64String(message.Payload);
				var id = message.Id.ToByteArray();
				msgList.Add(new HttpMessage
				{
					Id = message.Id.ToUInt128(),
					Payload = base64
				});
			}

			writer.WriteStartObject();
			writer.WriteString(nameof(MessageSendRequest.KeyKind).ToSnakeCase(), value: value.KeyKind switch
			{
				Keykind.EntityId => "entity_id",
				Keykind.PartitionId => "partition_id",
				_ => throw new InvalidEnumArgumentException()
			});
			writer.WriteNumber(nameof(MessageSendRequest.KeyValue).ToSnakeCase(), value.KeyValue);
			writer.WritePropertyName(nameof(MessageSendRequest.Messages).ToSnakeCase());
			JsonSerializer.Serialize(writer, msgList, options);
			writer.WriteEndObject();
		}
	}
}