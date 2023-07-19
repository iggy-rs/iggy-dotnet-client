using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Enums;
using Iggy_SDK.Factory;
using Iggy_SDK.MessageStream.Implementations;

namespace Shared;

public sealed class Test
{
	public void TestMethod()
	{
		var bus = MessageStreamFactory.CreateMessageStream(x =>
		{
			x.Protocol = Protocol.Http;
			x.BaseAdress = "http://localhost:8080";
		});
		
		bus.JoinConsumerGroupAsync(new JoinConsumerGroupRequest
		{
			StreamId = 1,
			TopicId = 1,
			ConsumerGroupId = 1
		});
	}
}