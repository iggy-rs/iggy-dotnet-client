using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Extensions;

namespace Iggy_SDK.JsonConfiguration;

public sealed class TopicResponseConverter : JsonConverter<TopicResponse>
{
	private readonly JsonSerializerOptions _options;
	public TopicResponseConverter()
	{
		_options = new JsonSerializerOptions();
		_options.PropertyNamingPolicy = new ToSnakeCaseNamingPolicy();
	}
	
	public override TopicResponse? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		using var doc = JsonDocument.ParseValue(ref reader);
		var root = doc.RootElement;
		var id = root.GetProperty(nameof(TopicResponse.Id).ToSnakeCase()).GetInt32();
		var name = root.GetProperty(nameof(TopicResponse.Name).ToSnakeCase()).GetString();
		var sizeBytes = root.GetProperty(nameof(TopicResponse.SizeBytes).ToSnakeCase()).GetUInt64();
		var messageExpiryProperty = root.GetProperty(nameof(TopicResponse.MessageExpiry).ToSnakeCase());
		
		var messageExpiry = messageExpiryProperty.ValueKind switch
		{
			JsonValueKind.Null => 0,
			JsonValueKind.Number => messageExpiryProperty.GetInt32(),
			_ => throw new InvalidEnumArgumentException("Error Wrong JsonValueKind when deserializing MessageExpiry")
		};
		var messagesCount = root.GetProperty(nameof(TopicResponse.MessagesCount).ToSnakeCase()).GetUInt64();
		var partitionsCount = root.GetProperty(nameof(TopicResponse.PartitionsCount).ToSnakeCase()).GetInt32();
		root.TryGetProperty(nameof(TopicResponse.Partitions).ToSnakeCase(), out var partitionsProperty);
		var partitions = partitionsProperty.ValueKind switch
		{
			JsonValueKind.Null => null,
			JsonValueKind.Undefined => null,
			JsonValueKind.Array => DeserializePartitions(partitionsProperty),
			_ => throw new InvalidEnumArgumentException("Error Wrong JsonValueKind when deserializing Partitions")
		};
		return new TopicResponse
		{
			Id = id,
			Name = name!,
			SizeBytes = sizeBytes,
			MessageExpiry = messageExpiry,
			MessagesCount = messagesCount,
			PartitionsCount = partitionsCount,
			Partitions = partitions
		};

	}
	private IEnumerable<PartitionContract> DeserializePartitions(JsonElement partitionsElement)
	{
		return JsonSerializer.Deserialize<IEnumerable<PartitionContract>>(partitionsElement.GetRawText(), _options)!;
	}

	public override void Write(Utf8JsonWriter writer, TopicResponse value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();

		writer.WriteNumber(nameof(TopicResponse.Id).ToSnakeCase(), value.Id);
		writer.WriteString(nameof(TopicResponse.Name).ToSnakeCase(), value.Name);
		writer.WriteNumber(nameof(TopicResponse.SizeBytes).ToSnakeCase(), value.SizeBytes);
		writer.WriteNumber(nameof(TopicResponse.MessageExpiry).ToSnakeCase(), value.MessageExpiry);
		writer.WriteNumber(nameof(TopicResponse.MessagesCount).ToSnakeCase(), value.MessagesCount);
		writer.WriteNumber(nameof(TopicResponse.PartitionsCount).ToSnakeCase(), value.PartitionsCount);

		if (value.Partitions != null)
		{
			writer.WriteStartArray(nameof(TopicResponse.Partitions).ToSnakeCase());

			foreach (var partition in value.Partitions)
			{
				var partitionJson = JsonSerializer.Serialize(partition, options);
				writer.WriteRawValue(partitionJson);
			}

			writer.WriteEndArray();
		}
		writer.WriteEndObject();
	}
}