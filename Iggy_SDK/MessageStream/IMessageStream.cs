
namespace Iggy_SDK.MessageStream;

//TODO - refactor all the kinds, message and header to readonly structs
public interface IMessageStream : IStreamClient, ITopicClient, IMessageClient, IOffsetClient, IConsumerGroupClient,
	IUtilsClient, IPartitionClient
{
}
