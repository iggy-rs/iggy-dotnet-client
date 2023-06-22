using Iggy_SDK.Enums;

namespace Iggy_SDK.Configuration;

public interface IMessageStreamConfigurator
{
    public string BaseAdress { get; set; } 
    public Protocol Protocol { get; set; }
}