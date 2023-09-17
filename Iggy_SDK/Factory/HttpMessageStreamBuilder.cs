using Iggy_SDK.Configuration;
using Iggy_SDK.Contracts.Http;
using Iggy_SDK.MessagesDispatcher;
using Iggy_SDK.MessageStream.Implementations;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;
using HttpMessageInvoker = Iggy_SDK.MessagesDispatcher.HttpMessageInvoker;
namespace Iggy_SDK.Factory;

internal class HttpMessageStreamBuilder
{
    private readonly HttpClient _client;
    private readonly IntervalBatchingSettings _options;
    private Channel<MessageSendRequest>? _channel;
    private MessageSenderDispatcher? _messageSenderDispatcher;
    private readonly ILoggerFactory _loggerFactory;

    internal HttpMessageStreamBuilder(HttpClient client, IMessageStreamConfigurator options)
    {
        var sendMessagesOptions = new IntervalBatchingSettings();
        options.IntervalBatchingConfig.Invoke(sendMessagesOptions);
        _options = sendMessagesOptions;
        
        _loggerFactory = options.LoggerFactory ?? LoggerFactory.Create(builder =>
        {
            builder
                .AddFilter("Microsoft", LogLevel.Warning)
                .AddFilter("System", LogLevel.Warning)
                .AddFilter("Iggy_SDK.MessageStream.Implementations", LogLevel.Trace)
                .AddConsole();
        });
        _client = client;
    }
    //TODO - this channel will probably need to be refactored, to accept a lambda instead of MessageSendRequest
    internal HttpMessageStreamBuilder WithSendMessagesDispatcher()
    {
        if (_options.Enabled)
        {
            _channel = Channel.CreateBounded<MessageSendRequest>(_options.MaxRequests);
            var messageInvoker = new HttpMessageInvoker(_client);
            _messageSenderDispatcher =
                new MessageSenderDispatcher(_options, _channel, messageInvoker, _loggerFactory);
        }
        return this;
    }
    internal HttpMessageStream Build()
    {
        _messageSenderDispatcher?.Start();
        return new HttpMessageStream(_client, _channel, _loggerFactory);
    }
    
}