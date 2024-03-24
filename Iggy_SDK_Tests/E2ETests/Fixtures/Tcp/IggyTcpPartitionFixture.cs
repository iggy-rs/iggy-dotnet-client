using DotNet.Testcontainers.Builders;
using Iggy_SDK;
using Iggy_SDK_Tests.E2ETests.Fixtures.Bootstraps;
using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Enums;
using Iggy_SDK.Factory;
using Iggy_SDK_Tests.Utils.Streams;
using Iggy_SDK_Tests.Utils.Topics;
using Iggy_SDK.Configuration;
using Iggy_SDK.IggyClient;
using Microsoft.Extensions.Logging.Abstractions;
using IContainer = DotNet.Testcontainers.Containers.IContainer;

namespace Iggy_SDK_Tests.E2ETests.Fixtures.Tcp;

public sealed class IggyTcpPartitionFixture : IggyBaseFixture
{
    public IggyTcpPartitionFixture() : base(new PartitionsFixtureBootstrap(),
        IggyFixtureClientMessagingSettings.PollingSettings,
        IggyFixtureClientMessagingSettings.BatchingSettings
    )
    {
    }
}