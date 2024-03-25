using Iggy_SDK_Tests.E2ETests.Fixtures.Bootstraps;

namespace Iggy_SDK_Tests.E2ETests.Fixtures.Tcp;

public sealed class IggyTcpGeneralFixture : IggyBaseFixture
{
    public IggyTcpGeneralFixture() : base(new GeneralFixtureBootstrap(),
        IggyFixtureClientMessagingSettings.PollingSettings,
        IggyFixtureClientMessagingSettings.BatchingSettings)
    {
    }
}