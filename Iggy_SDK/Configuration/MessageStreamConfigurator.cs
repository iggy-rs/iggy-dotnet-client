using Iggy_SDK.Enums;

namespace Iggy_SDK.Configuration;

public sealed class MessageStreamConfigurator : IMessageStreamConfigurator
{
    public string BaseAdress { get; set; }
    public Protocol Protocol { get; set; } = Protocol.Http;
}