using Iggy_SDK.Contracts.Http;

namespace Iggy_SDK.MessageStream;

public interface IUtilsClient
{
	public Task<Stats?> GetStatsAsync();
}