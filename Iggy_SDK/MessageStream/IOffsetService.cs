using Iggy_SDK.Contracts;

namespace Iggy_SDK.MessageStream;

public interface IOffsetService
{
	Task<bool> UpdateOffsetAsync(int streamId, int topicId, OffsetContract contract);
	Task<OffsetResponse?> GetOffsetAsync(OffsetRequest request);
}