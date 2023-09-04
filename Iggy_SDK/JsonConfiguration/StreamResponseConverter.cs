using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Extensions;
using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Iggy_SDK.JsonConfiguration;
public sealed class StreamResponseConverter : JsonConverter<StreamResponse>
{
    public override StreamResponse? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        var id = root.GetProperty(nameof(StreamResponse.Id).ToSnakeCase()).GetInt32();
        var name = root.GetProperty(nameof(StreamResponse.Name).ToSnakeCase()).GetString();
        var createdAt = root.GetProperty(nameof(StreamResponse.CreatedAt).ToSnakeCase()).GetUInt64();
        var sizeBytes = root.GetProperty(nameof(StreamResponse.SizeBytes).ToSnakeCase()).GetUInt64();
        var messagesCount = root.GetProperty(nameof(StreamResponse.MessagesCount).ToSnakeCase()).GetUInt64();
        var topicsCount = root.GetProperty(nameof(StreamResponse.TopicsCount).ToSnakeCase()).GetInt32();
        root.TryGetProperty(nameof(StreamResponse.Topics).ToSnakeCase(), out var topicsProperty);
        var topics = topicsProperty.ValueKind switch
        {
            JsonValueKind.Null => null,
            JsonValueKind.Undefined => null,
            JsonValueKind.Array => DeserializeTopics(topicsProperty),
            _ => throw new InvalidEnumArgumentException("Error Wrong JsonValueKind when deserializing Topics")
        };

        return new StreamResponse
        {
            Id = id,
            Name = name!,
            SizeBytes = sizeBytes,
            CreatedAt = DateTimeOffsetUtils.FromUnixTimeMicroSeconds(createdAt),
            MessagesCount = messagesCount,
            TopicsCount = topicsCount,
            Topics = topics
        };
    }

    private IEnumerable<TopicResponse> DeserializeTopics(JsonElement jsonProp)
    {
        var topics = jsonProp.EnumerateArray();
        var result = new List<TopicResponse>();
        foreach (var topic in topics)
        {
            var topicString = topic.GetRawText();
            var topicDeserialized = JsonSerializer.Deserialize<TopicResponse>(topicString, new JsonSerializerOptions
            {
                Converters = { new TopicResponseConverter() }
            });
            result.Add(topicDeserialized!);
        }

        return result;
    }

    public override void Write(Utf8JsonWriter writer, StreamResponse value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}