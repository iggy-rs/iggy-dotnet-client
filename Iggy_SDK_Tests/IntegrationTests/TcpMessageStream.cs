namespace Iggy_SDK_Tests.IntegrationTests;

public sealed class TcpMessageStream : IClassFixture<IggyFixture>
{
	private IggyFixture _fixture;

	public TcpMessageStream(IggyFixture fixture)
	{
		_fixture = fixture;
	}
}