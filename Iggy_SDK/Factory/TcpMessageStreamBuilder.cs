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

    internal TcpMessageStreamBuilder(Socket socket, IMessageStreamConfigurator options)
    {
        var sendMessagesOptions = new IntervalBatchingSettings();
        options.IntervalBatchingConfig.Invoke(sendMessagesOptions);
        _options = sendMessagesOptions;
        
        _loggerFactory = options.LoggerFactory ?? LoggerFactory.Create(builder =>
        {
            builder
                .AddFilter("Microsoft", LogLevel.Warning)
                .AddFilter("System", LogLevel.Warning)
                .AddFilter("Iggy_SDK.MessageStream.Implementations;", LogLevel.Trace)
                .AddConsole();
        });
        
        _socket = socket;
    }
    //TODO - this channel will probably need to be refactored, to accept a lambda instead of MessageSendRequest
    internal TcpMessageStreamBuilder WithSendMessagesDispatcher()
    {
        if (_options.Enabled)
        {
            _channel = Channel.CreateBounded<MessageSendRequest>(_options.MaxRequests);
            var messageInvoker = new TcpMessageInvoker(_socket);
            _messageSenderDispatcher =
                new MessageSenderDispatcher(_options, _channel, messageInvoker, _loggerFactory);
        }
        return this;
    }
    internal TcpMessageStream Build()
    {
        _messageSenderDispatcher?.Start();
        return new(_socket, _channel, _loggerFactory);
    }
    
}