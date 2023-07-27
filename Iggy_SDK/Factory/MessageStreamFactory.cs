using System.ComponentModel;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Quic;
using System.Net.Security;
using System.Net.Sockets;
using System.Runtime.Versioning;
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
            Protocol.Http => CreateHttpMessageStream(config),
            Protocol.Tcp => CreateTcpMessageStream(config),
            _ => throw new InvalidEnumArgumentException()
        };
    }

   

    private static TcpMessageStream CreateTcpMessageStream(IMessageStreamConfigurator options)
    {
        var urlPortSplitter = options.BaseAdress.Split(":");
        if (urlPortSplitter.Length > 2)
        {
            throw new InvalidBaseAdressException();
        }
        var client = new TcpClient(urlPortSplitter[0], int.Parse(urlPortSplitter[1]));
        client.SendBufferSize = options.SendBufferSize;
        options.ReceiveBufferSize = options.ReceiveBufferSize;
        return new TcpMessageStream(client);
    }

    private static HttpMessageStream CreateHttpMessageStream(IMessageStreamConfigurator options)
    {
        var client = new HttpClient();
        client.BaseAddress = new Uri(options.BaseAdress);
        if (options.Headers is not null)
        {
            foreach (var header in options.Headers)
            {
                client.DefaultRequestHeaders.Add(header.Name, header.Values);
            }
        }
        
        return new HttpMessageStream(client);
    }
}