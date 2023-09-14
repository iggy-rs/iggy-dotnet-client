using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Enums;
using Microsoft.Extensions.Logging;

namespace Iggy_SDK.Configuration;

public interface IMessageStreamConfigurator
{
    public string BaseAdress { get; set; }
    public Protocol Protocol { get; set; }
    public Action<SendMessageConfigurator> SendMessagesOptions { get; set; }
    public IEnumerable<HttpRequestHeaderContract>? Headers { get; set; }
    public ILoggerFactory? LoggerFactory { get; set; } 
    public int ReceiveBufferSize { get; set; }
    public int SendBufferSize { get; set; }
}