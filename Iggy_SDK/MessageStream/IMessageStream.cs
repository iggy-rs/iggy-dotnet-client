
namespace Iggy_SDK.MessageStream;

//TODO - explore using System.IO.Pipelines library for more efficient buffer management with sockets  
public interface IMessageStream : IStreamClient, ITopicClient, IMessageClient, IOffsetClient, IConsumerGroupClient,
	IUtilsClient, IPartitionClient
{
}
