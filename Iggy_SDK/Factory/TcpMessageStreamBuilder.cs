using Iggy_SDK.Configuration;
using Iggy_SDK.Contracts.Http;
using Iggy_SDK.MessagesDispatcher;
using Iggy_SDK.MessageStream.Implementations;
using System.Net.Sockets;
using System.Threading.Channels;
namespace Iggy_SDK.Factory;

internal class TcpMessageStreamBuilder
{
    private readonly Socket _socket;
    private readonly SendMessageConfigurator _options;
    private Channel<MessageSendRequest> _channel;
    private MessageSenderDispatcher _messageSenderDispatcher;

    internal TcpMessageStreamBuilder(Socket socket, SendMessageConfigurator options)
    {
        _socket = socket;
        _options = options;
    }
    //TODO - explore making this bounded ?
    //TODO - this channel will probably need to be refactored, to accept a lambda instead of MessageSendRequest
    internal TcpMessageStreamBuilder CreateChannel()
    {
        _channel = Channel.CreateUnbounded<MessageSendRequest>(new UnboundedChannelOptions
        {
        });
        return this;
    }
    internal TcpMessageStreamBuilder WithSendMessagesDispatcher()
    {
        var messageInvoker = new TcpMessageInvoker(_socket);
        _messageSenderDispatcher = _options.PollingInterval.Ticks switch
        {
            0 => new MessageSenderDispatcherNoBatching(_channel, messageInvoker),
            > 0 => new MessageSenderDispatcherWithBatching(_options, _channel, messageInvoker),
            _ => throw new ArgumentException($"{nameof(_options.PollingInterval)} has to be greater or equal than 0"),
        };
        return this;
    }
    internal TcpMessageStream Build()
    {
        _messageSenderDispatcher.Start();
        return new(_socket, _channel);
    }
    
}