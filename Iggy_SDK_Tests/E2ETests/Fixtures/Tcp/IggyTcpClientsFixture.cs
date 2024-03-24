using Iggy_SDK_Tests.E2ETests.Fixtures.Bootstraps;

namespace Iggy_SDK_Tests.E2ETests.Fixtures.Tcp;

public sealed class IggyTcpClientsFixture : IggyBaseFixture 
{
    public IggyTcpClientsFixture() : base(new ClientsFixtureBootstrap(),
        IggyFixtureClientMessagingSettings.PollingSettings, IggyFixtureClientMessagingSettings.BatchingSettings)
    {
    }
}