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
				msgList.Add(new HttpMessage
				{
					Id = message.Id.ToUInt128(),
					Payload = base64
				});
			}

			writer.WriteStartObject();
			writer.WriteStartObject("key");
			
			writer.WriteString(nameof(MessageSendRequest.Key.Kind).ToSnakeCase(), value: value.Key.Kind switch
			{
				KeyKind.None => "none",
				KeyKind.EntityId => "entity_id",
				KeyKind.PartitionId => "partition_id",
				_ => throw new InvalidEnumArgumentException()
			});
			writer.WriteBase64String(nameof(MessageSendRequest.Key.Value).ToSnakeCase(), value.Key.Value);
			writer.WriteEndObject();
			
			writer.WriteStartArray("messages");
			foreach (var msg in msgList)
			{
				JsonSerializer.Serialize(writer, msg, options);
			}
			writer.WriteEndArray();
			
			writer.WriteEndObject();
		}
	}
}