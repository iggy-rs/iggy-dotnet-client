using Iggy_SDK.Configuration;
using Iggy_SDK.Contracts.Http;
using Iggy_SDK.IggyClient.Implementations;
using Iggy_SDK.MessagesDispatcher;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;
using HttpMessageInvoker = Iggy_SDK.MessagesDispatcher.HttpMessageInvoker;
namespace Iggy_SDK.Factory;

internal class HttpMessageStreamBuilder
{
    private readonly HttpClient _client;
    private readonly MessageBatchingSettings _messageBatchingSettings;
    private readonly MessagePollingSettings _messagePollingSettings;
    private Channel<MessageSendRequest>? _channel;
    private MessageSenderDispatcher? _messageSenderDispatcher;
    private readonly ILoggerFactory _loggerFactory;
    private HttpMessageInvoker? _messageInvoker;

    internal HttpMessageStreamBuilder(HttpClient client, IMessageStreamConfigurator options, ILoggerFactory loggerFactory)
    {
        var sendMessagesOptions = new MessageBatchingSettings();
        var messagePollingOptions = new MessagePollingSettings();
        options.MessageBatchingSettings.Invoke(sendMessagesOptions);
        options.MessagePollingSettings.Invoke(messagePollingOptions);
        _messageBatchingSettings = sendMessagesOptions;
        _messagePollingSettings = messagePollingOptions;
        _client = client;
        _loggerFactory = loggerFactory;
    }
    //TODO - this channel will probably need to be refactored, to accept a lambda instead of MessageSendRequest
    internal HttpMessageStreamBuilder WithSendMessagesDispatcher()
    {
        if (_messageBatchingSettings.Enabled)
        {
            _channel = Channel.CreateBounded<MessageSendRequest>(_messageBatchingSettings.MaxRequests);
            _messageInvoker =  new HttpMessageInvoker(_client);
            _messageSenderDispatcher =
                new MessageSenderDispatcher(_messageBatchingSettings, _channel, _messageInvoker, _loggerFactory);
        }
        else
        {
            _messageInvoker =  new HttpMessageInvoker(_client);
        }
        return this;
    }
    internal HttpMessageStream Build()
    {
        _messageSenderDispatcher?.Start();
        return _messageBatchingSettings.Enabled switch
        {
            true => new HttpMessageStream(_client, _channel, _messagePollingSettings, _loggerFactory),
            false => new HttpMessageStream(_client, _channel, _messagePollingSettings, _loggerFactory, _messageInvoker)
        };
    }
    
}