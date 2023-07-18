using Iggy_SDK.Contracts;
using Iggy_SDK.Contracts.Http;

namespace Iggy_SDK.MessageStream;

public interface IOffsetClient
{
	Task<bool> StoreOffsetAsync(int streamId, int topicId, OffsetContract contract);
	Task<OffsetResponse?> GetOffsetAsync(OffsetRequest request);
}