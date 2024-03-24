using Iggy_SDK_Tests.E2ETests.Fixtures.Bootstraps;

namespace Iggy_SDK_Tests.E2ETests.Fixtures.Tcp;

public sealed class IggyTcpTopicFixture : IggyBaseFixture
{
    public IggyTcpTopicFixture() : base(new TopicsFixtureBootstrap(),  
        IggyFixtureClientMessagingSettings.PollingSettings,
        IggyFixtureClientMessagingSettings.BatchingSettings)
    {
    }
}