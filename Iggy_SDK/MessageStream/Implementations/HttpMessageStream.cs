using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Exceptions;
using Iggy_SDK.JsonConfiguration;
using Iggy_SDK.Kinds;
using Iggy_SDK.Messages;
using Iggy_SDK.StringHandlers;
using Iggy_SDK.Utils;

namespace Iggy_SDK.MessageStream.Implementations;


public class HttpMessageStream : IMessageStream
{
    //TODO - replace the HttpClient with IHttpClientFactory, when implementing support for ASP.NET Core DI
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _toSnakeCaseOptions;
    
    internal HttpMessageStream(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _toSnakeCaseOptions = new();
        
        _toSnakeCaseOptions.PropertyNamingPolicy = new ToSnakeCaseNamingPolicy();
        _toSnakeCaseOptions.WriteIndented = true;
        
        _toSnakeCaseOptions.Converters.Add(new UInt128Converter());
        _toSnakeCaseOptions.Converters.Add(new JsonStringEnumConverter(new ToSnakeCaseNamingPolicy()));
    }
    public async Task CreateStreamAsync(StreamRequest request)
    {
        var json = JsonSerializer.Serialize(request, _toSnakeCaseOptions);
        var data = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync("/streams", data);
        if (!response.IsSuccessStatusCode)
        {
            await HandleResponseAsync(response);
            throw new Exception("Unknown error occurred.");
        }
    }
    public async Task DeleteStreamAsync(Identifier streamId)
    {
        var response = await _httpClient.DeleteAsync($"/streams/{streamId}");
        if (!response.IsSuccessStatusCode)
        {
            await HandleResponseAsync(response);
            throw new Exception("Unknown error occurred.");
        }
    }
    public async Task<StreamResponse?> GetStreamByIdAsync(Identifier streamId)
    {
        var response = await _httpClient.GetAsync($"/streams/{streamId}");
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<StreamResponse>(_toSnakeCaseOptions);
        }
        await HandleResponseAsync(response);
        throw new Exception("Unknown error occurred.");
    }
    public async Task<IEnumerable<StreamResponse>> GetStreamsAsync()
    {
        var response = await _httpClient.GetAsync($"/streams");
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<IEnumerable<StreamResponse>>(_toSnakeCaseOptions)
                   ?? Enumerable.Empty<StreamResponse>();
        }
        await HandleResponseAsync(response);
        throw new Exception("Unknown error occurred.");
    }
    public async Task CreateTopicAsync(Identifier streamId, TopicRequest topic)
    {
        var json = JsonSerializer.Serialize(topic, _toSnakeCaseOptions);
        var data = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync($"/streams/{streamId}/topics", data);
        if (!response.IsSuccessStatusCode)
        {
            await HandleResponseAsync(response);
            throw new Exception("Unknown error occurred.");
        }
    }
    public async Task DeleteTopicAsync(Identifier streamId, Identifier topicId)
    {
        var response = await _httpClient.DeleteAsync($"/streams/{streamId}/topics/{topicId}");
        if (!response.IsSuccessStatusCode)
        {
            await HandleResponseAsync(response);
            throw new Exception("Unknown error occurred.");
        }
    }
    public async Task<IEnumerable<TopicResponse>> GetTopicsAsync(Identifier streamId)
    {
        var response = await _httpClient.GetAsync($"/streams/{streamId}/topics");
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<IEnumerable<TopicResponse>>(_toSnakeCaseOptions) 
                   ?? Enumerable.Empty<TopicResponse>();
        }
        await HandleResponseAsync(response);
        throw new Exception("Unknown error occurred.");
    }

    public async Task<TopicResponse?> GetTopicByIdAsync(Identifier streamId, Identifier topicId)
    {
        var response = await _httpClient.GetAsync($"/streams/{streamId}/topics/{topicId}");
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<TopicResponse>(_toSnakeCaseOptions);
        }
        await HandleResponseAsync(response);
        throw new Exception("Unknown error occurred.");
    }
    public async Task SendMessagesAsync(Identifier streamId, Identifier topicId, MessageSendRequest request)
    {
        var json = JsonSerializer.Serialize(request, _toSnakeCaseOptions);
        var data = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync($"/streams/{streamId}/topics/{topicId}/messages", data);
        if (!response.IsSuccessStatusCode)
        {
            await HandleResponseAsync(response);
            throw new Exception("Unknown error occurred.");
        }
    }

    public async Task SendMessagesAsync<TMessage>(Identifier streamId, Identifier topicId, Partitioning partitioning,
        ICollection<TMessage> messages, Func<TMessage, byte[]> serializer)
    {
        //TODO - maybe get rid of this closure ?
        var request = new MessageSendRequest
        {
            Partitioning = partitioning,
            Messages = messages.Select(message => new Message
            {
                Id = Guid.NewGuid(),
                Payload = serializer(message),
            }).ToArray()
        };
        var json = JsonSerializer.Serialize(request, _toSnakeCaseOptions);
        var data = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync($"/streams/{streamId}/topics/{topicId}/messages", data);
        if (!response.IsSuccessStatusCode)
        {
            await HandleResponseAsync(response);
            throw new Exception("Unknown error occurred.");
        }
    }

    public async Task<IEnumerable<MessageResponse>> PollMessagesAsync(MessageFetchRequest request)
    {
        var url = CreateUrl($"/streams/{request.StreamId}/topics/{request.TopicId}/messages?consumer_id={request.Consumer.Id}" +
                            $"&partition_id={request.PartitionId}&kind={request.PollingStrategy}&value={request.Value}&count={request.Count}&auto_commit={request.AutoCommit}");
        
        var response =  await _httpClient.GetAsync(url);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<IEnumerable<MessageResponse>>(new JsonSerializerOptions
                   {
                       Converters = { new MessageResponseConverter() }
                   })
                   ?? Enumerable.Empty<MessageResponse>();
        }
        await HandleResponseAsync(response);
        throw new Exception("Unknown error occurred.");
    }

    public async Task<IEnumerable<MessageResponse<TMessage>>> PollMessagesAsync<TMessage>(MessageFetchRequest request, Func<byte[], TMessage> serializer)
    {
        var url = CreateUrl($"/streams/{request.StreamId}/topics/{request.TopicId}/messages?consumer_id={request.Consumer.Id}" +
                            $"&partition_id={request.PartitionId}&kind={request.PollingStrategy}&value={request.Value}&count={request.Count}&auto_commit={request.AutoCommit}");
        
        var response =  await _httpClient.GetAsync(url);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<IEnumerable<MessageResponse<TMessage>>>(new JsonSerializerOptions
                   {
                       Converters = { new MessageResponseGenericConverter<TMessage>(serializer) }
                   })
                   ?? Enumerable.Empty<MessageResponse<TMessage>>();
        }
        await HandleResponseAsync(response);
        throw new Exception("Unknown error occurred.");
    }

    public async Task StoreOffsetAsync(Identifier streamId, Identifier topicId, OffsetContract contract)
    {
        var json = JsonSerializer.Serialize(contract, _toSnakeCaseOptions);
        var data = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PutAsync($"/streams/{streamId}/topics/{topicId}/consumer-offsets", data);
        if (!response.IsSuccessStatusCode)
        {
            await HandleResponseAsync(response);
            throw new Exception("Unknown error occurred.");
        }
    }

    public async Task<OffsetResponse?> GetOffsetAsync(OffsetRequest request)
    {
        var response = await _httpClient.GetAsync($"/streams/{request.StreamId}/topics/{request.TopicId}/" +
                       $"consumer-offsets?consumer_id={request.Consumer.Id}&partition_id={request.PartitionId}");
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<OffsetResponse>(_toSnakeCaseOptions);
        }
        await HandleResponseAsync(response);
        throw new Exception("Unknown error occurred.");
    }
    public async Task<IEnumerable<ConsumerGroupResponse>> GetConsumerGroupsAsync(Identifier streamId, Identifier topicId)
    {
        var response = await _httpClient.GetAsync($"/streams/{streamId}/topics/{topicId}/consumer-groups");
        
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<IEnumerable<ConsumerGroupResponse>>(_toSnakeCaseOptions)
                ?? Enumerable.Empty<ConsumerGroupResponse>();
        }
        await HandleResponseAsync(response);
        throw new Exception("Unknown error occurred.");
    }
    public async Task<ConsumerGroupResponse?> GetConsumerGroupByIdAsync(Identifier streamId, Identifier topicId, int groupId)
    {
        var response = await _httpClient.GetAsync($"/streams/{streamId}/topics/{topicId}/consumer-groups/{groupId}");
        
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<ConsumerGroupResponse>(_toSnakeCaseOptions);
        }

        await HandleResponseAsync(response);
        throw new Exception("Unknown error occurred.");
    }
    public async Task CreateConsumerGroupAsync(Identifier streamId, Identifier topicId, CreateConsumerGroupRequest request)
    {
        var json = JsonSerializer.Serialize(request, _toSnakeCaseOptions);
        var data = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync($"/streams/{streamId}/topics/{topicId}/consumer-groups", data);
        if (!response.IsSuccessStatusCode)
        {
            await HandleResponseAsync(response);
            throw new Exception("Unknown error occurred.");
        }
    }
    public async Task DeleteConsumerGroupAsync(Identifier streamId, Identifier topicId, int groupId)
    {
        var response = await _httpClient.DeleteAsync($"/streams/{streamId}/topics/{topicId}/consumer-groups/{groupId}");
        await HandleResponseAsync(response);
    }
    public async Task<Stats?> GetStatsAsync()
    {
        var response = await _httpClient.GetAsync($"/stats");
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<StatsResponse>(_toSnakeCaseOptions);
            return result?.ToStats();
        }
        await HandleResponseAsync(response);
        throw new Exception("Unknown error occurred.");
    }

    [Obsolete("This method is only supported in TCP protocol", true)]
    public Task JoinConsumerGroupAsync(JoinConsumerGroupRequest request)
    {
        throw new FeatureUnavailableException();
    }

    [Obsolete("This method is only supported in TCP protocol", true)]
    public Task LeaveConsumerGroupAsync(LeaveConsumerGroupRequest request)
    {
        throw new FeatureUnavailableException();
    }

    public async Task DeletePartitionsAsync(Identifier streamId, Identifier topicId, DeletePartitionsRequest request)
    {
        var json = JsonSerializer.Serialize(request, _toSnakeCaseOptions);
        var data = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.DeleteAsync(
            $"/streams/{streamId}/topics/{topicId}/partitions?stream_id={streamId}&topic_id={topicId}&partitions_count={request.PartitionsCount}");
        if (!response.IsSuccessStatusCode)
        {
            await HandleResponseAsync(response);
            throw new Exception("Unknown error occurred.");
        }
    }

    public async Task CreatePartitionsAsync(Identifier streamId, Identifier topicId, CreatePartitionsRequest request)
    {
        var json = JsonSerializer.Serialize(request, _toSnakeCaseOptions);
        var data = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync($"/streams/{streamId}/topics/{topicId}/partitions", data);
        if (!response.IsSuccessStatusCode)
        {
            await HandleResponseAsync(response);
            throw new Exception("Unknown error occurred.");
        }
    }

    private async Task HandleResponseAsync(HttpResponseMessage response)
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