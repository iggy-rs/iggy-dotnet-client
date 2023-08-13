using System.Text.Json;
using System.Text.Json.Serialization;
using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Extensions;

namespace Iggy_SDK.JsonConfiguration;

internal sealed class MessageResponseConverter : JsonConverter<IEnumerable<MessageResponse>>
{
	public override IEnumerable<MessageResponse> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		//TODO - mby get rid of this allocation by using ArrayPools
		var messageResponses = new List<MessageResponse>();
		using var doc = JsonDocument.ParseValue(ref reader);
		
		var root = doc.RootElement;
		foreach (var element in root.EnumerateArray())
		{
			var offset = element.GetProperty(nameof(MessageResponse.Offset).ToSnakeCase()).GetUInt64();
			var timestamp = element.GetProperty(nameof(MessageResponse.Timestamp).ToSnakeCase()).GetUInt64();
			var id = element.GetProperty(nameof(MessageResponse.Id).ToSnakeCase()).GetUInt128();
			var payload = element.GetProperty(nameof(MessageResponse.Payload).ToSnakeCase()).GetBytesFromBase64();

			messageResponses.Add(new MessageResponse
			{
				Offset = offset,
				Timestamp = timestamp,
				Id = new Guid(id.GetBytesFromUInt128()), 
				Payload = payload
			});
		}

		return messageResponses;
	}

	public override void Write(Utf8JsonWriter writer, IEnumerable<MessageResponse> value, JsonSerializerOptions options)
	{
		throw new NotImplementedException();
	}
}
