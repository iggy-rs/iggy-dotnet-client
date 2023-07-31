using Iggy_SDK.Contracts.Http;

namespace Iggy_SDK.MessageStream;

public interface IPartitionClient
{
	public Task DeletePartitionsAsync(int streamId, int topicId, DeletePartitionsRequest request);
	public Task CreatePartitionsAsync(int streamId, int topicId, CreatePartitionsRequest request);
}