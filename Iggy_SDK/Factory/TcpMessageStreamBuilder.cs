using Iggy_SDK.Configuration;
using Iggy_SDK.Contracts.Http;
using Iggy_SDK.MessagesDispatcher;
using Iggy_SDK.MessageStream.Implementations;
using Microsoft.Extensions.Logging;
using System.Net.Sockets;
using System.Threading.Channels;
namespace Iggy_SDK.Factory;

internal class TcpMessageStreamBuilder
{
    private readonly Socket _socket;
    private readonly IntervalBatchingSettings _options;
    private Channel<MessageSendRequest>? _channel;
    private MessageSenderDispatcher? _messageSenderDispatcher;
    private readonly ILoggerFactory _loggerFactory;
    private TcpMessageInvoker? _messageInvoker;

    internal TcpMessageStreamBuilder(Socket socket, IMessageStreamConfigurator options, ILoggerFactory loggerFactory)
    {
        var sendMessagesOptions = new IntervalBatchingSettings();
        options.IntervalBatchingConfig.Invoke(sendMessagesOptions);
        _options = sendMessagesOptions;
        _socket = socket;
        _loggerFactory = loggerFactory;
    }
    //TODO - this channel will probably need to be refactored, to accept a lambda instead of MessageSendRequest
    internal TcpMessageStreamBuilder WithSendMessagesDispatcher()
    {
        if (_options.Enabled)
        {
            _channel = Channel.CreateBounded<MessageSendRequest>(_options.MaxRequests);
            _messageInvoker = new TcpMessageInvoker(_socket);
            _messageSenderDispatcher =
                new MessageSenderDispatcher(_options, _channel, _messageInvoker, _loggerFactory);
        }
        else
        {
            _messageInvoker = new TcpMessageInvoker(_socket);
        }
        return this;
    }
    internal TcpMessageStream Build()
    {
        _messageSenderDispatcher?.Start();
        return _options.Enabled switch
        {
            true => new TcpMessageStream(_socket, _channel, _loggerFactory),
            false => new TcpMessageStream(_socket, _channel, _loggerFactory, _messageInvoker)
        };
    }
    
}