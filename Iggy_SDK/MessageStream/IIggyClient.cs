namespace Iggy_SDK.MessageStream;

//TODO - explore using System.IO.Pipelines library for more efficient buffer management with sockets  
public interface IIggyClient : IIggyPublisher, IIggyStream, IIggyTopic, IIggyConsumer, IIggyOffset, IIggyConsumerGroup,
    IIggyUtils, IIggyPartition
{
}