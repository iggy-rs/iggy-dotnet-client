using DotNet.Testcontainers.Builders;
using Iggy_SDK_Tests.E2ETests.Fixtures.Bootstraps;
using Iggy_SDK.Contracts.Http;
using Iggy_SDK_Tests.Utils.Users;
using Iggy_SDK.Configuration;
using Iggy_SDK.Enums;
using Iggy_SDK.Factory;
using Iggy_SDK.IggyClient;
using Microsoft.Extensions.Logging.Abstractions;
using IContainer = DotNet.Testcontainers.Containers.IContainer;

namespace Iggy_SDK_Tests.E2ETests.Fixtures.Tcp;

public sealed class IggyTcpUsersFixture : IggyBaseFixture
{
    public IggyTcpUsersFixture() : base(new UsersFixtureBootstrap(),
        IggyFixtureClientMessagingSettings.PollingSettings,
        IggyFixtureClientMessagingSettings.BatchingSettings)
    {
    }
}