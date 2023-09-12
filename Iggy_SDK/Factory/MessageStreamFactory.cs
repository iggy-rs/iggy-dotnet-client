using Iggy_SDK.Configuration;
using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Enums;
using Iggy_SDK.Exceptions;
using Iggy_SDK.MessagesDispatcher;
using Iggy_SDK.MessageStream;
using Iggy_SDK.MessageStream.Implementations;
using System.ComponentModel;
using System.Net.Sockets;
using System.Threading.Channels;

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
        var sendMessagesOptions = new SendMessageConfigurator();
        options.SendMessagesOptions.Invoke(sendMessagesOptions);

        var urlPortSplitter = options.BaseAdress.Split(":");
        if (urlPortSplitter.Length > 2)
        {
            throw new InvalidBaseAdressException();
        }
        
        var socket = CreateTcpSocket(options, urlPortSplitter);
        return new TcpMessageStreamBuilder(socket, sendMessagesOptions)
            .CreateChannel()
            .WithSendMessagesDispatcher()
            .Build();
    }

    private static Socket CreateTcpSocket(IMessageStreamConfigurator options, string[] urlPortSplitter)
    {
        var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Connect(urlPortSplitter[0], int.Parse(urlPortSplitter[1]));
        socket.SendBufferSize = options.SendBufferSize;
        socket.ReceiveBufferSize = options.ReceiveBufferSize;
        return socket;
    }

    private static HttpMessageStream CreateHttpMessageStream(IMessageStreamConfigurator options)
    {
        var sendMessagesOptions = new SendMessageConfigurator();
        options.SendMessagesOptions.Invoke(sendMessagesOptions);

        var client = CreateHttpClient(options);
        var channel = Channel.CreateBounded<MessageSendRequest>(sendMessagesOptions.MaxRequestsInPoll);

        return new HttpMessageStreamBuilder(client, sendMessagesOptions)
            .CreateChannel()
            .WithSendMessagesDispatcher()
            .Build();
    }
    private static HttpClient CreateHttpClient(IMessageStreamConfigurator options)
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
        return client;
    }
}