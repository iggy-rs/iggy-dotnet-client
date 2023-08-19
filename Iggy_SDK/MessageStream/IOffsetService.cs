using Iggy_SDK.Contracts;
using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Kinds;
using Iggy_SDK.Utils;

namespace Iggy_SDK.MessageStream;

public interface IOffsetClient
{
	Task StoreOffsetAsync(Identifier streamId, Identifier topicId, OffsetContract contract, CancellationToken token = default);
	Task<OffsetResponse?> GetOffsetAsync(OffsetRequest request, CancellationToken token = default);
}