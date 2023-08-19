using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Kinds;

namespace Iggy_SDK.MessageStream;

public interface IPartitionClient
{
	public Task DeletePartitionsAsync(Identifier streamId, Identifier topicId, DeletePartitionsRequest request, CancellationToken token = default);
	public Task CreatePartitionsAsync(Identifier streamId, Identifier topicId, CreatePartitionsRequest request, CancellationToken token = default);
}