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
    private HttpMessageInvoker? _messageInvoker;

    internal HttpMessageStreamBuilder(HttpClient client, IMessageStreamConfigurator options, ILoggerFactory loggerFactory)
    {
        var sendMessagesOptions = new IntervalBatchingSettings();
        options.IntervalBatchingConfig.Invoke(sendMessagesOptions);
        _options = sendMessagesOptions;
        _client = client;
        _loggerFactory = loggerFactory;
    }
    //TODO - this channel will probably need to be refactored, to accept a lambda instead of MessageSendRequest
    internal HttpMessageStreamBuilder WithSendMessagesDispatcher()
    {
        if (_options.Enabled)
        {
            _channel = Channel.CreateBounded<MessageSendRequest>(_options.MaxRequests);
            _messageInvoker =  new HttpMessageInvoker(_client);
            _messageSenderDispatcher =
                new MessageSenderDispatcher(_options, _channel, _messageInvoker, _loggerFactory);
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
        return _options.Enabled switch
        {
            true => new HttpMessageStream(_client, _channel, _loggerFactory),
            false => new HttpMessageStream(_client, _channel, _loggerFactory, _messageInvoker)
        };
    }
    
}