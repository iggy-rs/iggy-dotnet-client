﻿using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Enums;
using Iggy_SDK.Exceptions;
using Iggy_SDK.Extensions;
using Iggy_SDK.Headers;
using Iggy_SDK.JsonConfiguration;
using Iggy_SDK.Kinds;
using Iggy_SDK.Messages;
using Iggy_SDK.MessagesDispatcher;
using Iggy_SDK.StringHandlers;
using Iggy_SDK.Utils;
using Microsoft.Extensions.Logging;
using System.Buffers.Binary;
using System.Net;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;

namespace Iggy_SDK.MessageStream.Implementations;


public class HttpMessageStream : IIggyClient
{
    //TODO - replace the HttpClient with IHttpClientFactory, when implementing support for ASP.NET Core DI
    //TODO - the error handling pattern is pretty ugly, look into moving it into an extension method
    //TODO - I can create a good extension method for reading the response and handling the errors
    //but first I need to move the JsonConverters to attributes in contracts.
    private readonly HttpClient _httpClient;
    private readonly Channel<MessageSendRequest>? _channel;
    private readonly ILogger<HttpMessageStream> _logger;
    private readonly IMessageInvoker? _messageInvoker;

    internal HttpMessageStream(HttpClient httpClient, Channel<MessageSendRequest>? channel, ILoggerFactory loggerFactory, IMessageInvoker? messageInvoker = null)
    {
        _httpClient = httpClient;
        _channel = channel;
        _messageInvoker = messageInvoker;
        _logger = loggerFactory.CreateLogger<HttpMessageStream>();
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
        
        if (_messageInvoker is not null)
        {
            try
            {
                await _messageInvoker.SendMessagesAsync(request, token);
            }
            catch
            {
                var partId = BinaryPrimitives.ReadInt32LittleEndian(request.Partitioning.Value);
                _logger.LogError("Error encountered while sending messages - Stream ID:{streamId}, Topic ID:{topicId}, Partition ID: {partitionId}",
                    request.StreamId, request.TopicId, partId);
            }
            return;
        }
        await _channel!.Writer.WriteAsync(request, token);
    }

    public async Task SendMessagesAsync<TMessage>(MessageSendRequest<TMessage> request,
        Func<TMessage, byte[]> serializer,
        Func<byte[], byte[]>? encryptor = null, Dictionary<HeaderKey, HeaderValue>? headers = null,
        CancellationToken token = default)
    {
        var messages = request.Messages;
        //TODO - maybe get rid of this closure ?
        var sendRequest = new MessageSendRequest
        {
            StreamId = request.StreamId,
            TopicId = request.TopicId,
            Partitioning = request.Partitioning,
            Messages = messages.Select(message => new Message
            {
                Id = Guid.NewGuid(),
                Headers = headers,
                Payload = encryptor is not null ? encryptor(serializer(message)) : serializer(message),
            }).ToArray()
        };
        
        if (_messageInvoker is not null)
        {
            try
            {
                await _messageInvoker.SendMessagesAsync(sendRequest, token);
            }
            catch
            {
                var partId = BinaryPrimitives.ReadInt32LittleEndian(sendRequest.Partitioning.Value);
                _logger.LogError("Error encountered while sending messages - Stream ID:{streamId}, Topic ID:{topicId}, Partition ID: {partitionId}",
                    sendRequest.StreamId, sendRequest.TopicId, partId);
            }
            return;
        }
        await _channel!.Writer.WriteAsync(sendRequest, token);
    }

    public async Task<PolledMessages> FetchMessagesAsync(MessageFetchRequest request,
        Func<byte[], byte[]>? decryptor = null, CancellationToken token = default)
    {
        var url = CreateUrl($"/streams/{request.StreamId}/topics/{request.TopicId}/messages?consumer_id={request.Consumer.Id}" +
                            $"&partition_id={request.PartitionId}&kind={request.PollingStrategy.Kind}&value={request.PollingStrategy.Value}&count={request.Count}&auto_commit={request.AutoCommit}");

        var response = await _httpClient.GetAsync(url, token);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<PolledMessages>(JsonConverterFactory.MessageResponseOptions(decryptor), cancellationToken: token)
                   ?? PolledMessages.Empty;

        }
        await HandleResponseAsync(response);
        return PolledMessages.Empty;
    }

    public async Task<PolledMessages<TMessage>> FetchMessagesAsync<TMessage>(MessageFetchRequest request,
        Func<byte[], TMessage> serializer, Func<byte[], byte[]>? decryptor = null,
         CancellationToken token = default)
    {
        var url = CreateUrl($"/streams/{request.StreamId}/topics/{request.TopicId}/messages?consumer_id={request.Consumer.Id}" +
                            $"&partition_id={request.PartitionId}&kind={request.PollingStrategy.Kind}&value={request.PollingStrategy.Value}&count={request.Count}&auto_commit={request.AutoCommit}");

        var response = await _httpClient.GetAsync(url, token);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<PolledMessages<TMessage>>(JsonConverterFactory.MessageResponseGenericOptions(serializer, decryptor), cancellationToken: token)
                   ?? PolledMessages<TMessage>.Empty;
        }
        await HandleResponseAsync(response);
        return PolledMessages<TMessage>.Empty;
    }
    public async IAsyncEnumerable<MessageResponse<TMessage>> PollMessagesAsync<TMessage>(PollMessagesRequest request, 
        Func<byte[], TMessage> deserializer, Func<byte[], byte[]>? decryptor = null, 
        [EnumeratorCancellation] CancellationToken token = default)
    {
        var channel = Channel.CreateUnbounded<MessageResponse<TMessage>>();
        var autoCommit = request.StoreOffsetStrategy switch
        {
            StoreOffset.Never => false,
            StoreOffset.WhenMessagesAreReceived => true,
            StoreOffset.AfterProcessingEachMessage => false
        };
        var fetchRequest = new MessageFetchRequest
        {
            Consumer = request.Consumer,
            StreamId = request.StreamId,
            TopicId = request.TopicId,
            AutoCommit = autoCommit,
            Count = request.Count,
            PartitionId = request.PartitionId,
            PollingStrategy = request.PollingStrategy
        };
        

        _ = StartPollingMessagesAsync(fetchRequest, deserializer, request.Interval, channel.Writer, decryptor, token);
        await foreach(var messageResponse in channel.Reader.ReadAllAsync(token))
        {
            yield return messageResponse;
            
            var currentOffset = messageResponse.Offset;
            if (request.StoreOffsetStrategy is StoreOffset.AfterProcessingEachMessage)
            {
                var storeOffsetRequest = new StoreOffsetRequest
                {
                    Consumer = request.Consumer,
                    Offset = currentOffset,
                    PartitionId = request.PartitionId,
                    StreamId = request.StreamId,
                    TopicId = request.TopicId
                };
                try
                {
                    await StoreOffsetAsync(storeOffsetRequest, token);
                }
                catch
                {
                    _logger.LogError("Error encountered while saving offset information - Offset: {offset}, Stream ID: {streamId}, Topic ID: {topicId}, Partition ID: {partitionId}",
                        currentOffset, request.StreamId, request.TopicId, request.PartitionId);
                }
            }
            if (request.PollingStrategy.Kind is MessagePolling.Offset)
            {
                //TODO - check with profiler whether this doesn't cause a lot of allocations
                request.PollingStrategy = PollingStrategy.Offset(currentOffset + 1);
            }
        }
    }
    private async Task StartPollingMessagesAsync<TMessage>(MessageFetchRequest request,
        Func<byte[], TMessage> deserializer, TimeSpan interval, ChannelWriter<MessageResponse<TMessage>> writer,
        Func<byte[], byte[]>? decryptor = null,
        CancellationToken token = default)
    {
        var timer = new PeriodicTimer(interval);
        while (await timer.WaitForNextTickAsync(token) || token.IsCancellationRequested)
        {
            try
            {
                var fetchResponse = await FetchMessagesAsync(request, deserializer, decryptor, token);
                if (fetchResponse.Messages.Count == 0)
                {
                    continue;
                }
                foreach (var messageResponse in fetchResponse.Messages)
                {
                    await writer.WriteAsync(messageResponse, token);
                }
            }
            catch(Exception e)
            {
                _logger.LogError("Error encountered while polling messages - Stream ID: {streamId}, Topic ID: {topicId}, Partition ID: {partitionId}",
                    request.StreamId, request.TopicId, request.PartitionId);
            }
            
        }
        writer.Complete();
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
    public async Task<IReadOnlyList<ClientResponse>> GetClientsAsync(CancellationToken token = default)
    {
        var response = await _httpClient.GetAsync($"/clients", token);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<IReadOnlyList<ClientResponse>>(JsonConverterFactory.SnakeCaseOptions, token)
                ?? EmptyList<ClientResponse>.Instance;
        }
        await HandleResponseAsync(response);
        return EmptyList<ClientResponse>.Instance;
    }
    public async Task<ClientResponse?> GetClientByIdAsync(uint clientId, CancellationToken token = default)
    {
        var response = await _httpClient.GetAsync($"/clients/{clientId}", token);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<ClientResponse>(JsonConverterFactory.SnakeCaseOptions, token);
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
    public async Task<UserResponse?> GetUser(Identifier userId, CancellationToken token = default)
    {
        //TODO - this doesn't work prob needs a custom json serializer
        var response = await _httpClient.GetAsync($"/users/{userId}", token);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<UserResponse>(JsonConverterFactory.SnakeCaseOptions, token);
        }
        await HandleResponseAsync(response);
        return null;
    }
    public async Task<IReadOnlyList<UserResponse>> GetUsers(CancellationToken token = default)
    {
        var response = await _httpClient.GetAsync("/users", token);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<IReadOnlyList<UserResponse>>(JsonConverterFactory.SnakeCaseOptions, token)
                ?? EmptyList<UserResponse>.Instance;
        }
        await HandleResponseAsync(response);
        return EmptyList<UserResponse>.Instance;
    }
    public async Task CreateUser(CreateUserRequest request, CancellationToken token = default)
    {
        var json = JsonSerializer.Serialize(request, JsonConverterFactory.SnakeCaseOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json"); 
        var response = await _httpClient.PostAsync("/users", content, token);
        if (!response.IsSuccessStatusCode)
        {
            await HandleResponseAsync(response);
        }
    }
    public async Task DeleteUser(Identifier userId, CancellationToken token = default)
    {
        var response = await _httpClient.DeleteAsync($"/users/{userId}", token);
        if (!response.IsSuccessStatusCode)
        {
            await HandleResponseAsync(response);
        }
    }
    public async Task UpdateUser(UpdateUserRequest request, CancellationToken token = default)
    {
        var json = JsonSerializer.Serialize(request, JsonConverterFactory.SnakeCaseOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json"); 
        var response = await _httpClient.PutAsync($"/users/{request.UserId}", content, token);
        if (!response.IsSuccessStatusCode)
        {
            await HandleResponseAsync(response);
        }
    }
    public async Task UpdatePermissions(UpdateUserPermissionsRequest request, CancellationToken token = default)
    {
        var json = JsonSerializer.Serialize(request, JsonConverterFactory.SnakeCaseOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PutAsync($"/users/{request.UserId}/permissions", content, token);
        if (!response.IsSuccessStatusCode)
        {
            await HandleResponseAsync(response);
        }
    }
    public async Task ChangePassword(ChangePasswordRequest request, CancellationToken token = default)
    {
        var json = JsonSerializer.Serialize(request, JsonConverterFactory.SnakeCaseOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PutAsync($"/users/{request.UserId}/password", content, token);
        if (!response.IsSuccessStatusCode)
        {
            await HandleResponseAsync(response);
        }
    }
    public async Task LoginUser(LoginUserRequest request, CancellationToken token = default)
    {
        var json = JsonSerializer.Serialize(request, JsonConverterFactory.SnakeCaseOptions);
        var data = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("users/login", data, token);
        if (!response.IsSuccessStatusCode)
        {
            await HandleResponseAsync(response);
        }
    }
    public async Task LogoutUser(CancellationToken token = default)
    {
        var json = JsonSerializer.Serialize(new
        {
        }, JsonConverterFactory.SnakeCaseOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json"); 
        var response = await _httpClient.PostAsync("users/logout", content, token);
        if (!response.IsSuccessStatusCode)
        {
            await HandleResponseAsync(response);
        }
    }
}