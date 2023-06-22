using System.Net;
using System.Net.Http.Json;
using System.Text;
using Iggy_SDK.Contracts;
using System.Text.Json;
using System.Text.Json.Serialization;
using Iggy_SDK.SerializationConfiguration;
using Iggy_SDK.StringHandlers;

namespace Iggy_SDK.MessageStream;

public class HttpMessageStream : IMessageStream
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _toSnakeCaseOptions;
    internal HttpMessageStream(string baseAdress)
    {
        _httpClient = new();
        _toSnakeCaseOptions = new();
        
        _httpClient.BaseAddress = new Uri(baseAdress);
        
        _toSnakeCaseOptions.PropertyNamingPolicy = new ToSnakeCaseNamingPolicy();
        _toSnakeCaseOptions.WriteIndented = true;
        
        _toSnakeCaseOptions.Converters.Add(new UInt128Conveter());
        _toSnakeCaseOptions.Converters.Add(new JsonStringEnumConverter(new ToSnakeCaseNamingPolicy()));
    }
    public async Task<bool> CreateStreamAsync(CreateStreamRequest request)
    {
        var json = JsonSerializer.Serialize(request, _toSnakeCaseOptions);
        var data = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync("/streams", data);
        return response.StatusCode == HttpStatusCode.Created;
    }

    public async Task<bool> DeleteStreamAsync(int streamId)
    {
        var response = await _httpClient.DeleteAsync($"/streams/{streamId}");
        return response.StatusCode == HttpStatusCode.NoContent;
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

    public async Task<IEnumerable<StreamsResponse>> GetStreamsAsync()
    {
        var response = await _httpClient.GetAsync($"/streams");
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<IEnumerable<StreamsResponse>>(_toSnakeCaseOptions);
        }
        return Enumerable.Empty<StreamsResponse>();
    }

    public async Task<bool> CreateTopicAsync(int streamId, TopicRequest topic)
    {
        var json = JsonSerializer.Serialize(topic, _toSnakeCaseOptions);
        var data = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync($"/streams/{streamId}/topics", data);
        return response.StatusCode == HttpStatusCode.Created;
    }

    public async Task<bool> DeleteTopicAsync(int streamId, int topicId)
    {
        var response = await _httpClient.DeleteAsync($"/streams/{streamId}/topics/{topicId}");
        return response.StatusCode == HttpStatusCode.NoContent;
    }

    public async Task<IEnumerable<TopicsResponse>> GetTopicsAsync(int streamId)
    {
        var response = await _httpClient.GetAsync($"/streams/{streamId}/topics");
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<IEnumerable<TopicsResponse>>(_toSnakeCaseOptions) 
                   ?? Enumerable.Empty<TopicsResponse>();
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

    public async Task<bool> SendMessagesAsync(MessageSendRequest request)
    {
        var json = JsonSerializer.Serialize(request, _toSnakeCaseOptions);
        var data = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync($"/streams/{request.StreamId}/topics/{request.TopicId}/messages", data);
        return response.StatusCode == HttpStatusCode.Created;
    }

    public async Task<IEnumerable<MessageResponse>> GetMessagesAsync(MessageFetchRequest request)
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

    public async Task<bool> UpdateOffsetAsync(int streamId, int topicId, OffsetContract contract)
    {
        var json = JsonSerializer.Serialize(contract, _toSnakeCaseOptions);
        var data = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PutAsync($"/streams/{streamId}/topics/{topicId}/messages/offsets", data);
        var xd = await response.Content.ReadAsStringAsync();
        return response.StatusCode == HttpStatusCode.NoContent; 
    }

    public async Task<OffsetResponse?> GetOffsetAsync(OffsetRequest request)
    {
        var response = await _httpClient.GetAsync($"/streams/{request.StreamId}/topics/{request.TopicId}/messages/" +
                       $"offsets?consumer_id={request.ConsumerId}&partition_id={request.PartitionId}");
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<OffsetResponse>(_toSnakeCaseOptions);
        }
        return null;
    }

    private static string CreateUrl(ref MessageRequestInterpolationHandler message)
    {
        return message.ToString();
    }
}