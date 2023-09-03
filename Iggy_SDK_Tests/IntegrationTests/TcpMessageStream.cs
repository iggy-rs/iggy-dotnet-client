using Iggy_SDK;
using Iggy_SDK_Tests.Utils;
using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Enums;
using Iggy_SDK.Factory;
using Iggy_SDK.MessageStream;

namespace Iggy_SDK_Tests.IntegrationTests;

[TestCaseOrderer("Iggy_SDK_Tests.Utils.PriorityOrderer", "Iggy_SDK_Tests")]
public sealed class TcpMessageStream : IClassFixture<IggyFixture> 
{
	private IggyFixture _fixture;
	private readonly IMessageStream _sut;

	public TcpMessageStream(IggyFixture fixture)
	{
		_fixture = fixture;
		_sut = MessageStreamFactory.CreateMessageStream(options =>
		{
			options.BaseAdress = $"127.0.0.1:{_fixture.Container.GetMappedPublicPort(8090)}";
			options.Protocol = Protocol.Tcp;
			options.SendMessagesOptions = x =>
			{
				x.MaxMessagesPerBatch = 1000;
				x.PollingInterval = TimeSpan.FromMilliseconds(100);
			};
		});
	}

	[Fact, TestPriority(1)]
	public async Task Test()
	{
		await _sut.CreateStreamAsync(new StreamRequest
		{
			Name = "Test",
			StreamId = 1
		});
		var stream = await _sut.GetStreamByIdAsync(Identifier.Numeric(1));
		Assert.Equal(stream.Name, "test");
	}

	[Fact, TestPriority(2)]
	public async Task xP()
	{
		await _sut.DeleteStreamAsync(Identifier.Numeric(1));
	}

	[Fact, TestPriority(3)]
	public async Task xD()
	{
		var stream = await _sut.GetStreamByIdAsync(Identifier.Numeric(1));
		Assert.Equal(stream.Name, "test");
	}
		
}