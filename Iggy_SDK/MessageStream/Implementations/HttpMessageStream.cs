using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Exceptions;
using Iggy_SDK.Headers;
using Iggy_SDK.JsonConfiguration;
using Iggy_SDK.Kinds;
using Iggy_SDK.Messages;
using Iggy_SDK.StringHandlers;
using Iggy_SDK.Utils;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;

namespace Iggy_SDK.MessageStream.Implementations;


public class HttpMessageStream : IMessageStream
{
    //TODO - replace the HttpClient with IHttpClientFactory, when implementing support for ASP.NET Core DI
    //TODO - create a factory for all the custom JsonOptions serializers that will be used
    private readonly HttpClient _httpClient;
    private readonly Channel<MessageSendRequest> _channel;

    internal HttpMessageStream(HttpClient httpClient, Channel<MessageSendRequest> channel)
    {
        _httpClient = httpClient;
        _channel = channel;

    }
    public async Task CreateStreamAsync(StreamRequest request, CancellationToken token = default)
    {
        var json = JsonSerializer.Serialize(request, JsonConverterFactory.SnakeCaseOptions);
        var data = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("/streams", data, token);
        if (!response.IsSuccessStatusCode)
        {
            await HandleResponseAsync(response);
        }
    }
    public async Task DeleteStreamAsync(Identifier streamId, CancellationToken token = default)
    {
        var response = await _httpClient.DeleteAsync($"/streams/{streamId}", token);
        if (!response.IsSuccessStatusCode)
        {
            await HandleResponseAsync(response);
        }
    }
    public async Task<StreamResponse?> GetStreamByIdAsync(Identifier streamId, CancellationToken token = default)
    {
        var response = await _httpClient.GetAsync($"/streams/{streamId}", token);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<StreamResponse>(JsonConverterFactory.StreamResponseOptions, cancellationToken: token);
        }
        await HandleResponseAsync(response);
        return null;
    }

    public async Task UpdateStreamAsync(Identifier streamId, UpdateStreamRequest request, CancellationToken token = default)
    {
        var json = JsonSerializer.Serialize(request, JsonConverterFactory.SnakeCaseOptions);
        var data = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PutAsync($"/streams/{streamId}", data, token);
        if (!response.IsSuccessStatusCode)
        {
            await HandleResponseAsync(response);
        }
    }

    public async Task<IReadOnlyList<StreamResponse>> GetStreamsAsync(CancellationToken token = default)
    {
        var response = await _httpClient.GetAsync($"/streams", token);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<IReadOnlyList<StreamResponse>>(JsonConverterFactory.StreamResponseOptions,
                       cancellationToken: token)
                   ?? EmptyList<StreamResponse>.Instance;
        }
        await HandleResponseAsync(response);
        return EmptyList<StreamResponse>.Instance;
    }
    public async Task CreateTopicAsync(Identifier streamId, TopicRequest topic, CancellationToken token = default)
    {
        var json = JsonSerializer.Serialize(topic, JsonConverterFactory.SnakeCaseOptions);
        var data = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"/streams/{streamId}/topics", data, token);
        if (!response.IsSuccessStatusCode)
        {
            await HandleResponseAsync(response);
        }
    }

    public async Task UpdateTopicAsync(Identifier streamId, Identifier topicId, UpdateTopicRequest request, CancellationToken token = default)
    {
        var json = JsonSerializer.Serialize(request, JsonConverterFactory.SnakeCaseOptions);
        var data = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PutAsync($"/streams/{streamId}/topics/{topicId}", data, token);
        if (!response.IsSuccessStatusCode)
        {
            await HandleResponseAsync(response);
        }
    }

    public async Task DeleteTopicAsync(Identifier streamId, Identifier topicId, CancellationToken token = default)
    {
        var response = await _httpClient.DeleteAsync($"/streams/{streamId}/topics/{topicId}", token);
        if (!response.IsSuccessStatusCode)
        {
            await HandleResponseAsync(response);
        }
    }
    public async Task<IReadOnlyList<TopicResponse>> GetTopicsAsync(Identifier streamId, CancellationToken token = default)
    {
        var response = await _httpClient.GetAsync($"/streams/{streamId}/topics", token);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<IReadOnlyList<TopicResponse>>(JsonConverterFactory.TopicResponseOptions, cancellationToken: token)
                   ?? EmptyList<TopicResponse>.Instance;
        }
        await HandleResponseAsync(response);
        return EmptyList<TopicResponse>.Instance;
    }

    public async Task<TopicResponse?> GetTopicByIdAsync(Identifier streamId, Identifier topicId, CancellationToken token = default)
    {
        var response = await _httpClient.GetAsync($"/streams/{streamId}/topics/{topicId}", token);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<TopicResponse>(JsonConverterFactory.TopicResponseOptions, cancellationToken: token);
        }
        await HandleResponseAsync(response);
        return null;
    }

    public async Task SendMessagesAsync(MessageSendRequest request,
        Func<byte[], byte[]>? encryptor = null,
        CancellationToken token = default)
    {
        if (encryptor is not null)
        {
            for (var i = 0; i < request.Messages.Count; i++)
            {
                request.Messages[i] = request.Messages[i] with { Payload = encryptor(request.Messages[i].Payload) };
            }
        }
        await _channel.Writer.WriteAsync(request, token);
    }

    public async Task SendMessagesAsync<TMessage>(Identifier streamId, Identifier topicId, Partitioning partitioning,
        IList<TMessage> messages, Func<TMessage, byte[]> serializer,
        Func<byte[], byte[]>? encryptor = null, Dictionary<HeaderKey, HeaderValue>? headers = null,
        CancellationToken token = default)
    {
        //TODO - maybe get rid of this closure ?
        var request = new MessageSendRequest
        {
            StreamId = streamId,
            TopicId = topicId,
            Partitioning = partitioning,
            Messages = messages.Select(message => new Message
            {
                Id = Guid.NewGuid(),
                Headers = headers,
                Payload = encryptor is not null ? encryptor(serializer(message)) : serializer(message),
            }).ToArray()
        };
        await _channel.Writer.WriteAsync(request, token);
    }

    public async Task<PolledMessages> PollMessagesAsync(MessageFetchRequest request,
        Func<byte[], byte[]>? decryptor = null, CancellationToken token = default)
    {
        var url = CreateUrl($"/streams/{request.StreamId}/topics/{request.TopicId}/messages?consumer_id={request.Consumer.Id}" +
                            $"&partition_id={request.PartitionId}&kind={request.PollingStrategy.Kind}&value={request.PollingStrategy.Value}&count={request.Count}&auto_commit={request.AutoCommit}");

        var response = await _httpClient.GetAsync(url, token);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<PolledMessages>(
                       JsonConverterFactory.MessageResponseOptions(decryptor))
                   ?? PolledMessages.Empty;

        }
        await HandleResponseAsync(response);
        return PolledMessages.Empty;
    }

    public async Task<PolledMessages<TMessage>> PollMessagesAsync<TMessage>(MessageFetchRequest request,
        Func<byte[], TMessage> serializer,
        Func<byte[], byte[]>? decryptor = null, CancellationToken token = default)
    {
        var url = CreateUrl($"/streams/{request.StreamId}/topics/{request.TopicId}/messages?consumer_id={request.Consumer.Id}" +
                            $"&partition_id={request.PartitionId}&kind={request.PollingStrategy.Kind}&value={request.PollingStrategy.Value}&count={request.Count}&auto_commit={request.AutoCommit}");

        var response = await _httpClient.GetAsync(url, token);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<PolledMessages<TMessage>>(
                       JsonConverterFactory.MessageResponseGenericOptions(serializer, decryptor))
                   ?? PolledMessages<TMessage>.Empty;
        }
        await HandleResponseAsync(response);
        return PolledMessages<TMessage>.Empty;
    }

    public async Task StoreOffsetAsync(StoreOffsetRequest request, CancellationToken token = default)
    {
        var json = JsonSerializer.Serialize(request, JsonConverterFactory.SnakeCaseOptions);
        var data = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PutAsync($"/streams/{request.StreamId}/topics/{request.TopicId}/consumer-offsets", data, token);
        if (!response.IsSuccessStatusCode)
        {
            await HandleResponseAsync(response);
        }
    }

    public async Task<OffsetResponse?> GetOffsetAsync(OffsetRequest request, CancellationToken token = default)
    {
        var response = await _httpClient.GetAsync($"/streams/{request.StreamId}/topics/{request.TopicId}/" +
                       $"consumer-offsets?consumer_id={request.Consumer.Id}&partition_id={request.PartitionId}", token);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<OffsetResponse>(JsonConverterFactory.SnakeCaseOptions, cancellationToken: token);
        }
        await HandleResponseAsync(response);
        return null;
    }
    public async Task<IReadOnlyList<ConsumerGroupResponse>> GetConsumerGroupsAsync(Identifier streamId, Identifier topicId, CancellationToken token = default)
    {
        var response = await _httpClient.GetAsync($"/streams/{streamId}/topics/{topicId}/consumer-groups", token);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<IReadOnlyList<ConsumerGroupResponse>>(JsonConverterFactory.SnakeCaseOptions, cancellationToken: token)
                   ?? EmptyList<ConsumerGroupResponse>.Instance;
        }
        await HandleResponseAsync(response);
        return EmptyList<ConsumerGroupResponse>.Instance;
    }
    public async Task<ConsumerGroupResponse?> GetConsumerGroupByIdAsync(Identifier streamId, Identifier topicId,
        int groupId, CancellationToken token = default)
    {
        var response = await _httpClient.GetAsync($"/streams/{streamId}/topics/{topicId}/consumer-groups/{groupId}", token);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<ConsumerGroupResponse>(JsonConverterFactory.SnakeCaseOptions, cancellationToken: token);
        }

        await HandleResponseAsync(response);
        return null;
    }
    public async Task CreateConsumerGroupAsync(CreateConsumerGroupRequest request, CancellationToken token = default)
    {
        var json = JsonSerializer.Serialize(request, JsonConverterFactory.SnakeCaseOptions);
        var data = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"/streams/{request.StreamId}/topics/{request.TopicId}/consumer-groups", data, token);
        if (!response.IsSuccessStatusCode)
        {
            await HandleResponseAsync(response);
        }
    }
    public async Task DeleteConsumerGroupAsync(DeleteConsumerGroupRequest request, CancellationToken token = default)
    {
        var response = await _httpClient.DeleteAsync($"/streams/{request.StreamId}/topics/{request.TopicId}/consumer-groups/{request.ConsumerGroupId}", token);
        await HandleResponseAsync(response);
    }
    public async Task<Stats?> GetStatsAsync(CancellationToken token = default)
    {
        var response = await _httpClient.GetAsync($"/stats", token);
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<StatsResponse>(JsonConverterFactory.SnakeCaseOptions, cancellationToken: token);
            return result?.ToStats();
        }
        await HandleResponseAsync(response);
        return null;
    }

    [Obsolete("This method is only supported in TCP protocol", true)]
    public Task JoinConsumerGroupAsync(JoinConsumerGroupRequest request, CancellationToken token = default)
    {
        throw new FeatureUnavailableException();
    }

    [Obsolete("This method is only supported in TCP protocol", true)]
    public Task LeaveConsumerGroupAsync(LeaveConsumerGroupRequest request, CancellationToken token = default)
    {
        throw new FeatureUnavailableException();
    }

    public async Task DeletePartitionsAsync(DeletePartitionsRequest request,
        CancellationToken token = default)
    {
        var response = await _httpClient.DeleteAsync($"/streams/{request.StreamId}/topics/{request.TopicId}/partitions?partitions_count={request.PartitionsCount}", token);
        if (!response.IsSuccessStatusCode)
        {
            await HandleResponseAsync(response);
        }
    }

    public async Task CreatePartitionsAsync(CreatePartitionsRequest request, CancellationToken token = default)
    {
        var json = JsonSerializer.Serialize(request, JsonConverterFactory.SnakeCaseOptions);
        var data = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"/streams/{request.StreamId}/topics/{request.TopicId}/partitions", data, token);
        if (!response.IsSuccessStatusCode)
        {
            await HandleResponseAsync(response);
        }
    }

    private static async Task HandleResponseAsync(HttpResponseMessage response)
    {
        if ((int)response.StatusCode > 300 && (int)response.StatusCode < 500)
        {
            var err = await response.Content.ReadAsStringAsync();
            throw new InvalidResponseException(err);
        }
        if (response.StatusCode == HttpStatusCode.InternalServerError)
        {
            throw new Exception("Internal server error");
        }
    }
    private static string CreateUrl(ref MessageRequestInterpolationHandler message)
    {
        return message.ToString();
    }
}