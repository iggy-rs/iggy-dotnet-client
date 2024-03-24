using Iggy_SDK_Tests.E2ETests.Fixtures.Bootstraps;

namespace Iggy_SDK_Tests.E2ETests.Fixtures.Tcp;

public sealed class IggyTcpSendMessagesFixture : IggyBaseFixture
{
    public IggyTcpSendMessagesFixture() : base(new SendMessagesFixtureBootstrap(),
        IggyFixtureClientMessagingSettings.PollingSettings,
        IggyFixtureClientMessagingSettings.BatchingSettingsSendFixture)
    {
    }
}