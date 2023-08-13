
namespace Iggy_SDK.MessageStream;

//TODO - Add cancellation tokens to all of the methods
public interface IMessageStream : IStreamClient, ITopicClient, IMessageClient, IOffsetClient, IConsumerGroupClient,
	IUtilsClient, IPartitionClient
{
}
