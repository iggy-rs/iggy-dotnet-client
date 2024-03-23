using Iggy_SDK.Configuration;
using Iggy_SDK.Enums;

namespace Iggy_SDK_Tests.E2ETests.Fixtures;

public static class IggyFixtureClientMessagingSettings
{
    public static Action<MessagePollingSettings> PollingSettings { get; set; } = options =>
    {
        options.Interval = TimeSpan.FromMilliseconds(100);
        options.StoreOffsetStrategy = StoreOffset.WhenMessagesAreReceived;
    };
    public static Action<MessageBatchingSettings> BatchingSettings { get; set; } = options =>
    {
        options.Interval = TimeSpan.FromMilliseconds(100);
        options.MaxMessagesPerBatch = 1000;
        options.MaxRequests = 4096;
    };
}