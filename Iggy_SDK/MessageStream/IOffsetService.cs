using Iggy_SDK.Contracts.Http;

namespace Iggy_SDK.MessageStream;

public interface IOffsetClient
{
    Task StoreOffsetAsync(StoreOffsetRequest contract, CancellationToken token = default);
    Task<OffsetResponse?> GetOffsetAsync(OffsetRequest request, CancellationToken token = default);
}