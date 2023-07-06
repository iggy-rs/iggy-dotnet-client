using Iggy_SDK.Contracts;

namespace Iggy_SDK.MessageStream;

public interface IMessageStream : IStreamService, ITopicService, IMessageService, IOffsetService, IGroupService
{
	
}
