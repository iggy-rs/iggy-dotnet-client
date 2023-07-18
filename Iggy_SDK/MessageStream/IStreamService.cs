using Iggy_SDK.Contracts;
using Iggy_SDK.Contracts.Http;

namespace Iggy_SDK.MessageStream;

public interface IStreamClient
{
	Task<bool> CreateStreamAsync(StreamRequest request);
	Task<StreamResponse?> GetStreamByIdAsync(int streamId);
	Task<IEnumerable<StreamResponse>> GetStreamsAsync();
	Task<bool> DeleteStreamAsync(int streamId);

}