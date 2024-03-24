using Iggy_SDK_Tests.E2ETests.Fixtures.Bootstraps;

namespace Iggy_SDK_Tests.E2ETests.Fixtures.Tcp;

public sealed class IggyTcpConsumerGroupFixture : IggyBaseFixture
{
    public IggyTcpConsumerGroupFixture() : base(new ConsumerGroupFixtureBootstrap(),
        IggyFixtureClientMessagingSettings.PollingSettings, IggyFixtureClientMessagingSettings.BatchingSettings)
    {
    }
}