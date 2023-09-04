using Iggy_SDK.Contracts.Http;

namespace Iggy_SDK.MessageStream;

public interface IPartitionClient
{
    public Task DeletePartitionsAsync(DeletePartitionsRequest request, CancellationToken token = default);
    public Task CreatePartitionsAsync(CreatePartitionsRequest request, CancellationToken token = default);
}