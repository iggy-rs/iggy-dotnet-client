using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Extensions;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Iggy_SDK.JsonConfiguration;

internal sealed class CreateTopicConverter : JsonConverter<TopicRequest>
{
    public override TopicRequest? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override void Write(Utf8JsonWriter writer, TopicRequest value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        if (value.TopicId is not null)
        {
            writer.WriteNumber(nameof(value.TopicId).ToSnakeCase(), (int)value.TopicId);
        }
        writer.WriteString(nameof(value.Name).ToSnakeCase(), value.Name);
        if (value.MessageExpiry is not null)
        {
            writer.WriteNumber(nameof(value.MessageExpiry).ToSnakeCase(), (int)value.MessageExpiry);
        }
        writer.WriteNumber(nameof(value.PartitionsCount).ToSnakeCase(), value.PartitionsCount);
        writer.WriteString(nameof(value.MaxTopicSize).ToSnakeCase(), value.MaxTopicSize.ToString());
        writer.WriteNumber(nameof(value.ReplicationFactor).ToSnakeCase(), value.ReplicationFactor);
        writer.WriteEndObject();
    }
}