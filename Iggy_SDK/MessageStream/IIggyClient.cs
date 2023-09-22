namespace Iggy_SDK.MessageStream;

public interface IIggyClient : IIggyPublisher, IIggyStream, IIggyTopic, IIggyConsumer, IIggyOffset, IIggyConsumerGroup,
    IIggyUtils, IIggyPartition, IIggyUsers
{
}