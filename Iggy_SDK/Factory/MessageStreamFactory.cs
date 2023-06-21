using System.ComponentModel;
using Iggy_SDK.Configuration;
using Iggy_SDK.MessageStream;
using Iggy_SDK.Protocols;

namespace Iggy_SDK.Factory;

public static class MessageStreamFactory
{
    public static IMessageStream CreateMessageStream(Action<IMessageStreamConfigurator> options)
    {
        var config = new MessageStreamConfigurator();
        options.Invoke(config);

        return config.Protocol switch
        {
            Protocol.Http => new HttpMessageStream(config.BaseAdress),
            _ => throw new InvalidEnumArgumentException()
        };
    }
}