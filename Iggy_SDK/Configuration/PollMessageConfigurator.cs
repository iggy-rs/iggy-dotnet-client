using Iggy_SDK.Enums;
namespace Iggy_SDK.Configuration;

public sealed class PollMessageConfigurator
{
    public TimeSpan PollingInterval { get; set; } = TimeSpan.FromMilliseconds(100);
    public StoreOffset StoreOffsetKind { get; set; } = StoreOffset.WhenMessagesAreProcessed;
}