using Iggy_SDK.Contracts;
using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Utils;

namespace Iggy_SDK.MessageStream;

public interface IOffsetClient
{
	Task StoreOffsetAsync(int streamId, int topicId, OffsetContract contract);
	Task<OffsetResponse?> GetOffsetAsync(OffsetRequest request);
}