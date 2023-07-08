using System.ComponentModel;
using System.Net.Sockets;
using Iggy_SDK.Configuration;
using Iggy_SDK.Enums;
using Iggy_SDK.Exceptions;
using Iggy_SDK.MessageStream;
using Iggy_SDK.MessageStream.Implementations;

namespace Iggy_SDK.Factory;

public static class MessageStreamFactory
{
    //TODO - this whole setup will have to be refactored later,when adding support for ASP.NET Core DI
    public static IMessageStream CreateMessageStream(Action<IMessageStreamConfigurator> options)
    {
        var config = new MessageStreamConfigurator();
        options.Invoke(config);

        return config.Protocol switch
        {
            Protocol.Http => CreateHttpMessageStream(config.BaseAdress),
            Protocol.Tcp => CreateTcpMessageStream(config.BaseAdress),
            _ => throw new InvalidEnumArgumentException()
        };
    }

    private static TcpMessageStream CreateTcpMessageStream(string configBaseAdress)
    {
        var urlPortSplitter = configBaseAdress.Split(":");
        if (urlPortSplitter.Length > 2)
        {
            throw new InvalidBaseAdressException();
        }
        var client = new TcpClient(urlPortSplitter[0], int.Parse(urlPortSplitter[1]));
        return new TcpMessageStream(client);
    }

    private static HttpMessageStream CreateHttpMessageStream(string baseAdress)
    {
        var client = new HttpClient();
        client.BaseAddress = new Uri(baseAdress);
        return new HttpMessageStream(client);
    }
}