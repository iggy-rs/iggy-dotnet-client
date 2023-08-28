using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Enums;

namespace Iggy_SDK.Configuration;

//TODO - refactor this to be more expressive (e.g nested configurators etc..)
public interface IMessageStreamConfigurator
{
    public string BaseAdress { get; set; } 
    public Protocol Protocol { get; set; }
    public Action<SendMessageConfigurator> SendMessagesOptions { get; set; } 
    public IEnumerable<HttpRequestHeaderContract>? Headers { get; set; }
    public int ReceiveBufferSize { get; set; }
    public int SendBufferSize { get; set; }
}