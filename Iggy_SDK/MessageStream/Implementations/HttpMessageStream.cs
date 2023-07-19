using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Exceptions;
using Iggy_SDK.SerializationConfiguration;
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
        
        _toSnakeCaseOptions.Converters.Add(new UInt128Conveter());
        _toSnakeCaseOptions.Converters.Add(new JsonStringEnumConverter(new ToSnakeCaseNamingPolicy()));
    }
    public async Task CreateStreamAsync(StreamRequest request)
    {
        var json = JsonSerializer.Serialize(request, _toSnakeCaseOptions);
        var data = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync("/streams", data);
        await HandleResponseAsync(response);
    }

    public async Task DeleteStreamAsync(int streamId)
    {
        var response = await _httpClient.DeleteAsync($"/streams/{streamId}");
        await HandleResponseAsync(response);
    }

    public async Task<StreamResponse> GetStreamByIdAsync(int streamId)
    {
        var response = await _httpClient.GetAsync($"/streams/{streamId}");
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<StreamResponse>(_toSnakeCaseOptions);
        }
        var errResponse = await response.Content.ReadFromJsonAsync<ErrorModel>(_toSnakeCaseOptions);
        throw new InvalidResponseException(errResponse.Reason);
    }

    public async Task<IEnumerable<StreamResponse>> GetStreamsAsync()
    {
        var response = await _httpClient.GetAsync($"/streams");
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<IEnumerable<StreamResponse>>(_toSnakeCaseOptions)
                   ?? Enumerable.Empty<StreamResponse>();
        }
        var errResponse = await response.Content.ReadFromJsonAsync<ErrorModel>(_toSnakeCaseOptions);
        throw new InvalidResponseException(errResponse.Reason);
    }

    public async Task CreateTopicAsync(int streamId, TopicRequest topic)
    {
        var json = JsonSerializer.Serialize(topic, _toSnakeCaseOptions);
        var data = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync($"/streams/{streamId}/topics", data);
        await HandleResponseAsync(response);
    }

    public async Task DeleteTopicAsync(int streamId, int topicId)
    {
        var response = await _httpClient.DeleteAsync($"/streams/{streamId}/topics/{topicId}");
        await HandleResponseAsync(response);
    }

    public async Task<IEnumerable<TopicResponse>> GetTopicsAsync(int streamId)
    {
        var response = await _httpClient.GetAsync($"/streams/{streamId}/topics");
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<IEnumerable<TopicResponse>>(_toSnakeCaseOptions) 
                   ?? Enumerable.Empty<TopicResponse>();
        }
        var errResponse = await response.Content.ReadFromJsonAsync<ErrorModel>(_toSnakeCaseOptions);
        throw new InvalidResponseException(errResponse.Reason);
    }

    public async Task<TopicResponse> GetTopicByIdAsync(int streamId, int topicId)
    {
        var response = await _httpClient.GetAsync($"/streams/{streamId}/topics/{topicId}");
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<TopicResponse>(_toSnakeCaseOptions);
        }
        var errResponse = await response.Content.ReadFromJsonAsync<ErrorModel>(_toSnakeCaseOptions);
        throw new InvalidResponseException(errResponse.Reason);
    }

    public async Task SendMessagesAsync(MessageSendRequest request)
    {
        foreach (var message in request.Messages)
        {
            var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(message.Payload));
            message.Payload = base64;
        }
        
        var json = JsonSerializer.Serialize(request, _toSnakeCaseOptions);
        var data = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync($"/streams/{request.StreamId}/topics/{request.TopicId}/messages", data);
        await HandleResponseAsync(response);
    }

    public async Task<IEnumerable<MessageResponse>> PollMessagesAsync(MessageFetchRequest request)
    {
        var url = CreateUrl($"/streams/{request.StreamId}/topics/{request.TopicId}/messages?consumer_id={request.ConsumerId}" +
                            $"&partition_id={request.PartitionId}&kind={request.PollingStrategy}&value={request.Value}&count={request.Count}&auto_commit={request.AutoCommit}");
        
        var response =  await _httpClient.GetAsync(url);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<IEnumerable<MessageResponse>>(_toSnakeCaseOptions) 
                   ?? Enumerable.Empty<MessageResponse>();
        }
        var errResponse = await response.Content.ReadFromJsonAsync<ErrorModel>(_toSnakeCaseOptions);
        throw new InvalidResponseException(errResponse.Reason);
    }

    public async Task StoreOffsetAsync(int streamId, int topicId, OffsetContract contract)
    {
        var json = JsonSerializer.Serialize(contract, _toSnakeCaseOptions);
        var data = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PutAsync($"/streams/{streamId}/topics/{topicId}/messages/offsets", data);
        await HandleResponseAsync(response);
    }

    public async Task<OffsetResponse> GetOffsetAsync(OffsetRequest request)
    {
        var response = await _httpClient.GetAsync($"/streams/{request.StreamId}/topics/{request.TopicId}/messages/" +
                       $"offsets?consumer_id={request.ConsumerId}&partition_id={request.PartitionId}");
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<OffsetResponse>(_toSnakeCaseOptions);
        }
        var errResponse = await response.Content.ReadFromJsonAsync<ErrorModel>(_toSnakeCaseOptions);
        throw new InvalidResponseException(errResponse.Reason);
    }
    public async Task<IEnumerable<GroupResponse>> GetGroupsAsync(int streamId, int topicId)
    {
        var response = await _httpClient.GetAsync($"/streams/{streamId}/topics/{topicId}/consumer_groups");
        
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<IEnumerable<GroupResponse>>(_toSnakeCaseOptions)
                ?? Enumerable.Empty<GroupResponse>();
        }
        var errResponse = await response.Content.ReadFromJsonAsync<ErrorModel>(_toSnakeCaseOptions);
        throw new InvalidResponseException(errResponse.Reason);
    }
    
    public async Task<GroupResponse> GetGroupByIdAsync(int streamId, int topicId, int groupId)
    {
        var response = await _httpClient.GetAsync($"/streams/{streamId}/topics/{topicId}/consumer_groups/{groupId}");
        
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<GroupResponse>(_toSnakeCaseOptions);
        }
        var errResponse = await response.Content.ReadFromJsonAsync<ErrorModel>(_toSnakeCaseOptions);
        throw new InvalidResponseException(errResponse.Reason);
    }
    
    public async Task CreateGroupAsync(int streamId, int topicId, CreateGroupRequest request)
    {
        var json = JsonSerializer.Serialize(request, _toSnakeCaseOptions);
        var data = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync($"/streams/{streamId}/topics/{topicId}/consumer_groups", data);
        await HandleResponseAsync(response);
    }
    public async Task DeleteGroupAsync(int streamId, int topicId, int groupId)
    {
        var response = await _httpClient.DeleteAsync($"/streams/{streamId}/topics/{topicId}/consumer_groups/{groupId}");
        await HandleResponseAsync(response);
    }
    
    private async Task HandleResponseAsync(HttpResponseMessage response)
    {
        if (response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.NotFound)
        {
            var err = await response.Content.ReadFromJsonAsync<ErrorModel>(_toSnakeCaseOptions);
            throw new InvalidResponseException(err.Reason);
        }
        if (response.StatusCode == HttpStatusCode.InternalServerError)
        {
            throw new Exception("Internal server error");
        }
        else
        {
            throw new Exception("Unknown error occurred.");
        }
    }
    private static string CreateUrl(ref MessageRequestInterpolationHandler message)
    {
        return message.ToString();
    }
}