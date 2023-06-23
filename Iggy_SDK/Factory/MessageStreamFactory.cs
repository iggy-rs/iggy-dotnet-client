using System.ComponentModel;
using Iggy_SDK.Configuration;
using Iggy_SDK.Enums;
using Iggy_SDK.MessageStream;

namespace Iggy_SDK.Factory;

public static class MessageStreamFactory
{
    //this whole setup will have to be refactored later,when adding support for ASP.NET Core DI
    public static IMessageStream CreateMessageStream(Action<IMessageStreamConfigurator> options)
    {
        var config = new MessageStreamConfigurator();
        options.Invoke(config);

        return config.Protocol switch
        {
            Protocol.Http => CreateHttpMessageStream(config.BaseAdress),
            _ => throw new InvalidEnumArgumentException()
        };
    }

    private static HttpMessageStream CreateHttpMessageStream(string baseAdress)
    {
        var client = new HttpClient();
        client.BaseAddress = new Uri(baseAdress);
        return new HttpMessageStream(client);
    }
}