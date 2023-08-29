using System.ComponentModel;
using System.Net.Sockets;
using System.Threading.Channels;
using Iggy_SDK.Configuration;
using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Enums;
using Iggy_SDK.Exceptions;
using Iggy_SDK.MessagesDispatcher;
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
        var sendMessagesOptions = new SendMessageConfigurator();
        options.SendMessagesOptions.Invoke(sendMessagesOptions);
        
        var urlPortSplitter = options.BaseAdress.Split(":");
        if (urlPortSplitter.Length > 2)
        {
            throw new InvalidBaseAdressException();
        }
        var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Connect(urlPortSplitter[0], int.Parse(urlPortSplitter[1]));
        socket.SendBufferSize = options.SendBufferSize;
        socket.ReceiveBufferSize = options.ReceiveBufferSize;

        //TODO - explore making this bounded ?
        var channel = Channel.CreateUnbounded<MessageSendRequest>(new UnboundedChannelOptions
        {
            //TODO - turn those on, for the benchmark, to see if it will work with multi threaded tasks
            //SingleWriter = true,
            //SingleReader = true,
        });
        
        var messageStream = new TcpMessageStream(socket, channel);
        var messageBus = new TcpMessageInvoker(socket);
        var messageDispatcher = new MessageSenderDispatcher(sendMessagesOptions, channel, messageBus);
        
        messageDispatcher.Start();
        return messageStream;
    }
    
    private static HttpMessageStream CreateHttpMessageStream(IMessageStreamConfigurator options)
    {
        var sendMessagesOptions = new SendMessageConfigurator();
        options.SendMessagesOptions.Invoke(sendMessagesOptions);
        
        var client = new HttpClient();
        client.BaseAddress = new Uri(options.BaseAdress);
        if (options.Headers is not null)
        {
            foreach (var header in options.Headers)
            {
                client.DefaultRequestHeaders.Add(header.Name, header.Values);
            }
        }
        
        //TODO - explore making this bounded ?
        var channel = Channel.CreateUnbounded<MessageSendRequest>(new UnboundedChannelOptions
        {
            //TODO - turn those on, for the benchmark, to see if it will work with multi threaded tasks
            //SingleWriter = true,
            //SingleReader = true,
        });
        
        var messageStream = new HttpMessageStream(client, channel);
        var messageBus = new MessagesDispatcher.HttpMessageInvoker(client);
        var messageDispatcher = new MessageSenderDispatcher(sendMessagesOptions, channel, messageBus);
        
        messageDispatcher.Start();

        return messageStream;
    }
}