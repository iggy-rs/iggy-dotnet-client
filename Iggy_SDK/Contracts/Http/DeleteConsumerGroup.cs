namespace Iggy_SDK.Contracts.Http;

public sealed class DeleteConsumerGroup
{

    public required Identifier StreamId { get; init; }
    public required Identifier TopicId { get; init; }
    public required int ConsumerGroupId { get; init; }

}