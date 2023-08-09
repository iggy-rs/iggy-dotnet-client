using Iggy_SDK.Contracts;
using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Kinds;
using Iggy_SDK.Utils;

namespace Iggy_SDK.MessageStream;

public interface IStreamClient
{
	Task CreateStreamAsync(StreamRequest request);
	Task<StreamResponse?> GetStreamByIdAsync(Identifier streamId);
	Task<IEnumerable<StreamResponse>> GetStreamsAsync();
	Task DeleteStreamAsync(Identifier streamId);

}