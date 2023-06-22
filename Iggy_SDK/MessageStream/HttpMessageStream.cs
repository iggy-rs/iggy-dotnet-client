using System.Net.Http.Json;
using System.Text;
using Iggy_SDK.Contracts;
using System.Text.Json;
using Iggy_SDK.Enums;
using Iggy_SDK.JsonConfiguration;
using Iggy_SDK.Messages;
using Iggy_SDK.StringHandlers;

namespace Iggy_SDK.MessageStream;

public class HttpMessageStream : IMessageStream
{
    private static HttpClient _httpClient = new();
    private JsonSerializerOptions? _toSnakeCaseOptions = new() { Converters = { new UInt128Conveter() }};
    public HttpMessageStream(string baseAdress)
    {
        _httpClient.BaseAddress = new Uri(baseAdress);
        
        _toSnakeCaseOptions!.PropertyNamingPolicy = new ToSnakeCaseNamingPolicy();
        _toSnakeCaseOptions!.WriteIndented = true;
    }
    public async Task<bool> CreateStreamAsync(CreateStreamRequest request)
    {
        var json = JsonSerializer.Serialize(request, _toSnakeCaseOptions);
        var data = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("/streams", data);
        return response.StatusCode == System.Net.HttpStatusCode.Created ? true : false;
    }
    
    public async Task<StreamResponse?> GetStreamByIdAsync(int streamId)
    {
        var response = await _httpClient.GetAsync($"/streams/{streamId}");
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<StreamResponse>(_toSnakeCaseOptions);
        }
        
        return null;
    }
    
    public async Task<bool> CreateTopicAsync(int streamId, TopicRequest topic)
    {
        var json = JsonSerializer.Serialize(topic, _toSnakeCaseOptions);
        var data = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync($"/streams/{streamId}/topics", data);
        return response.StatusCode == System.Net.HttpStatusCode.Created ? true : false;
    }

    public async Task<IEnumerable<TopicsResponse>> GetTopicsAsync(int streamId)
    {
        var response = await _httpClient.GetAsync($"/streams/{streamId}/topics");
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<IEnumerable<TopicsResponse>>(_toSnakeCaseOptions) 
                   ?? Array.Empty<TopicsResponse>();
        }
        
        return Enumerable.Empty<TopicsResponse>();
    }

    public async Task<TopicsResponse?> GetTopicByIdAsync(int streamId, int topicId)
    {
        var response = await _httpClient.GetAsync($"/streams/{streamId}/topics/{topicId}");
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<TopicsResponse>(_toSnakeCaseOptions);
        }
        return null;
    }

    public async Task<bool> SendMessagesAsync(int streamId, int topicId, Keykind keyKind, int keyValue, IEnumerable<IMessage> messages)
    {
        var keyKindValue = keyKind switch
        {
            Keykind.PartitionId => "partition_id",
            Keykind.Offset => "offset",
            _ => "partition_id"
        };
        var request = new SendMessageRequest
        {
            KeyKind = keyKindValue,
            KeyValue = keyValue,
            Messages = messages.Select(x =>
            {
                return new MessageContract
                {
                    Id = x.Id,
                    Payload = x.Payload
                };
            })
        };
        var json = JsonSerializer.Serialize(request, _toSnakeCaseOptions);
        var data = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync($"/streams/{streamId}/topics/{topicId}/messages", data);
        return response.StatusCode == System.Net.HttpStatusCode.Created ? true : false;
    }

    public async Task<IEnumerable<MessageResponse>> GetMessagesAsync(MessageRequest request)
    {
        var url = CreateUrl($"/streams/{request.StreamId}/topics/{request.TopicId}/messages?consumer_id={request.ConsumerId}" +
            $"&partition_id={request.PartitionId}&kind={request.PollingStrategy}&value={request.Value}&count={request.Count}&auto_commit={request.AutoCommit}");
        
        var response =  await _httpClient.GetAsync(url);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<IEnumerable<MessageResponse>>(_toSnakeCaseOptions) 
                   ?? Enumerable.Empty<MessageResponse>();
        }
        
        return Enumerable.Empty<MessageResponse>();
    }
    private string CreateUrl(ref MessageRequestInterpolationHandler message)
    {
        return message.ToString();
    }
}