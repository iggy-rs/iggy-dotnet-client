using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Enums;

namespace Iggy_SDK.Configuration;

public sealed class MessageStreamConfigurator : IMessageStreamConfigurator
{
    public string BaseAdress { get; set; } = "http://127.0.0.1:3000";
    public Protocol Protocol { get; set; } = Protocol.Http;
    public IEnumerable<HttpRequestHeaderContract>? Headers { get; set; } = null;

    public Action<SendMessageConfigurator> SendMessagesOptions { get; set; } = options =>
    {
        options.PollingInterval = TimeSpan.FromMilliseconds(100);
        options.MaxMessagesPerBatch = 1000;
        options.MaxRequestsInPoll = 4096;
    };
    public int ReceiveBufferSize { get; set; } = 4096;
    public int SendBufferSize { get; set; } = 4096;
}