using Iggy_SDK.Configuration;
using Iggy_SDK.Contracts.Http;
using Iggy_SDK.MessagesDispatcher;
using Iggy_SDK.MessageStream.Implementations;
using System.Net.Sockets;
using System.Threading.Channels;
using HttpMessageInvoker = Iggy_SDK.MessagesDispatcher.HttpMessageInvoker;
namespace Iggy_SDK.Factory;

internal class HttpMessageStreamBuilder
{
    private readonly HttpClient _client;
    private readonly SendMessageConfigurator _options;
    private Channel<MessageSendRequest> _channel;
    private MessageSenderDispatcher _messageSenderDispatcher;

    internal HttpMessageStreamBuilder(HttpClient client, SendMessageConfigurator options)
    {
        _client = client;
        _options = options;
    }
    //TODO - this channel will probably need to be refactored, to accept a lambda instead of MessageSendRequest
    internal HttpMessageStreamBuilder CreateChannel()
    {
        _channel = Channel.CreateBounded<MessageSendRequest>(_options.MaxRequestsInPoll);
        return this;
    }
    internal HttpMessageStreamBuilder WithSendMessagesDispatcher()
    {
        var messageInvoker = new HttpMessageInvoker(_client);
        _messageSenderDispatcher = _options.PollingInterval.Ticks switch
        {
            0 => new MessageSenderDispatcherNoBatching(_channel, messageInvoker),
            > 0 => new MessageSenderDispatcherWithBatching(_options, _channel, messageInvoker),
            _ => throw new ArgumentException($"{nameof(_options.PollingInterval)} has to be greater or equal than 0"),
        };
        return this;
    }
    internal HttpMessageStream Build()
    {
        _messageSenderDispatcher.Start();
        return new(_client, _channel);
    }
    
}