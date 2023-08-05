using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Identifiers;

namespace Iggy_SDK.MessageStream;

public interface IPartitionClient
{
	public Task DeletePartitionsAsync(Identifier streamId, Identifier topicId, DeletePartitionsRequest request);
	public Task CreatePartitionsAsync(Identifier streamId, Identifier topicId, CreatePartitionsRequest request);
}