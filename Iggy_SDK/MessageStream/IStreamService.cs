using Iggy_SDK.Contracts;
using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Utils;

namespace Iggy_SDK.MessageStream;

public interface IStreamClient
{
	Task<Result> CreateStreamAsync(StreamRequest request);
	Task<StreamResponse?> GetStreamByIdAsync(int streamId);
	Task<IEnumerable<StreamResponse>> GetStreamsAsync();
	Task<Result> DeleteStreamAsync(int streamId);

}