using System.Net.Http.Headers;
using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Enums;

namespace Iggy_SDK.Configuration;

public sealed class MessageStreamConfigurator : IMessageStreamConfigurator
{
    public string BaseAdress { get; set; }
    public Protocol Protocol { get; set; } = Protocol.Http;
    public IEnumerable<HttpRequestHeaderContract>? Headers { get; set; } = null;
    public int ReceiveBufferSize { get; set; } = 8192;
    public int SendBufferSize { get; set; } = 8192;
}