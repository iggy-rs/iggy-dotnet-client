using Iggy_SDK_Tests.E2ETests.Fixtures.Bootstraps;

namespace Iggy_SDK_Tests.E2ETests.Fixtures.Tcp;

public sealed class IggyTcpFetchMessagesFixture : IggyBaseFixture
{
    public IggyTcpFetchMessagesFixture() : base(new FetchMessagesFixtureBootstrap(),
        IggyFixtureClientMessagingSettings.PollingSettings, IggyFixtureClientMessagingSettings.BatchingSettingsFetchFixture)
    {
    }
}