using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Channels;
using Iggy_SDK;
using Iggy_SDK_Tests.Utils.Errors;
using Iggy_SDK_Tests.Utils.Groups;
using Iggy_SDK_Tests.Utils.Messages;
using Iggy_SDK_Tests.Utils.Offset;
using Iggy_SDK_Tests.Utils.Partitions;
using Iggy_SDK_Tests.Utils.Streams;
using Iggy_SDK_Tests.Utils.Topics;
using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Errors;
using Iggy_SDK.Exceptions;
using Iggy_SDK.JsonConfiguration;
using Iggy_SDK.Kinds;
using Iggy_SDK.MessageStream;
using RichardSzalay.MockHttp;

namespace Iggy_SDK_Tests.MessageStreamTests;

public sealed class HttpMessageStream
{
	private readonly MockHttpMessageHandler _httpHandler;
	private readonly IMessageStream _sut;
	private readonly Channel<MessageSendRequest> _channel;
	
	private JsonSerializerOptions _toSnakeCaseOptions;
	
	private const string URL = "http://localhost:3000";
	public HttpMessageStream()
	{
		_channel = Channel.CreateUnbounded<MessageSendRequest>();
		_toSnakeCaseOptions = new();
		_toSnakeCaseOptions.PropertyNamingPolicy = new ToSnakeCaseNamingPolicy();
		_toSnakeCaseOptions.WriteIndented = true;
        //This code makes the source generated JsonSerializer work with JsonIgnore attribute for required properties
        _toSnakeCaseOptions.TypeInfoResolver = new DefaultJsonTypeInfoResolver
        {
            Modifiers =
            {
                ti =>
                {
                    if (ti.Kind == JsonTypeInfoKind.Object)
                    {
                        JsonPropertyInfo[] props = ti.Properties
                            .Where(prop => prop.AttributeProvider == null || prop.AttributeProvider
                                .GetCustomAttributes(typeof(JsonIgnoreAttribute), false).Length == 0)
                            .ToArray();

                        if (props.Length != ti.Properties.Count)
                        {
                            ti.Properties.Clear();
                            foreach (var prop in props)
                            {
                                ti.Properties.Add(prop);
                            }
                        }
                    }
                }
            }
        };
        _toSnakeCaseOptions.Converters.Add(new UInt128Converter());
        _toSnakeCaseOptions.Converters.Add(new JsonStringEnumConverter(new ToSnakeCaseNamingPolicy()));
        _httpHandler = new MockHttpMessageHandler();
        
        
		var client = _httpHandler.ToHttpClient();
		client.BaseAddress = new Uri(URL);
		_sut = new Iggy_SDK.MessageStream.Implementations.HttpMessageStream(client, _channel);
	}
	
	[Fact]
	public async Task CreateStreamAsync_ThrowsErrorResponseException_OnFailure()
	{
		var content = StreamFactory.CreateStreamRequest();
		var json = JsonSerializer.Serialize(content, _toSnakeCaseOptions);
		var error = JsonSerializer.Serialize(ErrorModelFactory.CreateErrorModelBadRequest(), _toSnakeCaseOptions);

		_httpHandler.When(HttpMethod.Post, $"/streams")
			.WithContent(json)
			.Respond(HttpStatusCode.BadRequest, "application/json", error);
		
		await Assert.ThrowsAsync<InvalidResponseException>(async () => await _sut.CreateStreamAsync(content));
		_httpHandler.Flush();
	}

	[Fact]
	public async Task GetStreamByIdAsync_ReturnsStreamResponse_WhenFound()
	{
		var streamId = Identifier.Numeric(1);
		var streamResponse = StreamFactory.CreateStreamResponse();
		var content = JsonSerializer.Serialize(streamResponse, _toSnakeCaseOptions);

		_httpHandler.When(HttpMethod.Get, $"/streams/{streamId}")
			.Respond(HttpStatusCode.OK, "application/json", content);
		
		var result = await _sut.GetStreamByIdAsync(streamId);
		Assert.NotNull(result);
		_httpHandler.Flush();
	}	
	
	[Fact]
	public async Task GetStreamByIdAsync_ThrowsErrorResponseException_OnFailure()
	{
		var streamId = Identifier.Numeric(1);
		var error = JsonSerializer.Serialize(ErrorModelFactory.CreateErrorModelNotFound(), _toSnakeCaseOptions); 
		
		_httpHandler.When(HttpMethod.Get, $"/streams/{streamId}")
			.Respond(HttpStatusCode.NotFound, "application/json", error);
		
		await Assert.ThrowsAsync<InvalidResponseException>( async ()=> await _sut.GetStreamByIdAsync(streamId));
		_httpHandler.Flush();
	}

	[Fact]
	public async Task GetStreamsAsync_ReturnsListOfStreamResponse_WhenFound()
	{
		var response = new List<StreamResponse>();
		response.Add(StreamFactory.CreateStreamsResponse());
		var content = JsonSerializer.Serialize(response, _toSnakeCaseOptions);

		_httpHandler.When(HttpMethod.Get, $"/streams")
			.Respond(HttpStatusCode.OK, "application/json", content);
				
		
		var result = await _sut.GetStreamsAsync();
		Assert.NotNull(result);
		Assert.NotEmpty(result);
		_httpHandler.Flush();
	}

	[Fact]
	public async Task GetStreamsAsync_ThrowsErrorResponseException_OnFailure()
	{
		var content = JsonSerializer.Serialize(ErrorModelFactory.CreateErrorModelNotFound(), _toSnakeCaseOptions);

		_httpHandler.When(HttpMethod.Get, $"/streams")
			.Respond(HttpStatusCode.NotFound, "application/json", content);


		await Assert.ThrowsAsync<InvalidResponseException>( async () => await _sut.GetStreamsAsync());
		_httpHandler.Flush();
	}

	[Fact]
	public async Task CreateTopicAsync_ThrowsErrorResponseException_OnFailure()
	{
		var streamId = Identifier.Numeric(1);
		var content = TopicFactory.CreateTopicRequest();
		var json = JsonSerializer.Serialize(content, _toSnakeCaseOptions);
		var error = JsonSerializer.Serialize(ErrorModelFactory.CreateErrorModelBadRequest(), _toSnakeCaseOptions);


		_httpHandler.When(HttpMethod.Post, $"/streams/{streamId}/topics")
			.WithContent(json)
			.Respond(HttpStatusCode.BadRequest, "application/json", error);

		await Assert.ThrowsAsync<InvalidResponseException>( async () => await _sut.CreateTopicAsync(streamId, content));
		_httpHandler.Flush();
	}

	[Fact]
	public async Task DeleteTopicAsync_ThrowsErrorResponseException_OnFailure()
	{
		var streamId = Identifier.Numeric(1);
		var topicId = Identifier.Numeric(1);
		var error = JsonSerializer.Serialize(ErrorModelFactory.CreateErrorModelBadRequest(), _toSnakeCaseOptions);


		_httpHandler.When(HttpMethod.Delete, $"/streams/{streamId}/topics/{topicId}")
			.Respond(HttpStatusCode.BadRequest, "application/json", error);

		await Assert.ThrowsAsync<InvalidResponseException>(async () => await _sut.DeleteTopicAsync(streamId, topicId));
		_httpHandler.Flush();
	}
	
	[Fact]
	public async Task GetTopicsAsync_ReturnsListOfTopicResponse_WhenFound()
	{
		var streamId = Identifier.Numeric(1);
		
		var response = new List<TopicResponse>();
		response.Add(TopicFactory.CreateTopicsResponse());
		var content = JsonSerializer.Serialize(response, _toSnakeCaseOptions);
		
		_httpHandler.When(HttpMethod.Get, $"/streams/1/topics")
			.Respond(HttpStatusCode.OK, "application/json", content);
		
		var result = await _sut.GetTopicsAsync(streamId);
		Assert.NotNull(result);
		Assert.NotEmpty(result);
		_httpHandler.Flush();
	}

	[Fact]
	public async Task GetTopicsAsync_ThrowsErrorResponseException_OnFailure()
	{
		var streamId = Identifier.Numeric(1);
		var error = JsonSerializer.Serialize(ErrorModelFactory.CreateErrorModelNotFound(), _toSnakeCaseOptions); 
		
		_httpHandler.When(HttpMethod.Get, $"/streams/{streamId}/topics")
			.Respond(HttpStatusCode.NotFound, "application/json", error);
		
		await Assert.ThrowsAsync<InvalidResponseException>(async () => await _sut.GetTopicsAsync(streamId));
		_httpHandler.Flush();
		
	}
	
	[Fact]
	public async Task GetTopicByIdAsync_ReturnsTopicResponse_WhenFound()
	{
		var streamId = Identifier.Numeric(1);
		var topicId = Identifier.Numeric(1);
		TopicResponse topic = TopicFactory.CreateTopicsResponse();
		var content = JsonSerializer.Serialize(topic, _toSnakeCaseOptions);

		_httpHandler.When(HttpMethod.Get, $"/streams/{streamId}/topics/{topicId}")
			.Respond(HttpStatusCode.OK, "application/json", content);
		
		var result = await _sut.GetTopicByIdAsync(streamId, topicId);
		Assert.NotNull(result);
		Assert.Equal(topic.Id, result.Id);
		Assert.Equal(topic.Name, result.Name);
		Assert.Equal(topic.PartitionsCount, result.PartitionsCount);
		_httpHandler.Flush();
	}
	
	[Fact]
	public async Task GetTopicByIdAsync_ThrowsErrorResponseException_OnFailure()
	{
		var streamId = Identifier.Numeric(1);
		var topicId = Identifier.Numeric(1);
		var error = JsonSerializer.Serialize(ErrorModelFactory.CreateErrorModelNotFound(), _toSnakeCaseOptions); 
		
		_httpHandler.When(HttpMethod.Get, $"/streams/{streamId}/topics/{topicId}")
			.Respond(HttpStatusCode.NotFound, "application/json", error);
		
		await Assert.ThrowsAsync<InvalidResponseException>( async () => await _sut.GetTopicByIdAsync(streamId, topicId));
		_httpHandler.Flush();
	}

	
	[Fact]
	public async Task GetMessagesAsync_ReturnsMessages_WhenFound()
	{
		var request = MessageFactory.CreateMessageFetchRequest();
		var response = new List<MessageResponseHttp>();
		response.Add(MessageFactory.CreateMessageResponse());
		var content = JsonSerializer.Serialize(response, _toSnakeCaseOptions); 
		
		_httpHandler.When(HttpMethod.Get, 
        $"/streams/{request.StreamId}/topics/{request.TopicId}/messages?consumer_id={request.Consumer.Id}" +
                            $"&partition_id={request.PartitionId}&kind=offset&value={request.PollingStrategy.Value}&count={request.Count}" +
							$"&auto_commit={request.AutoCommit.ToString().ToLower()}")
					.Respond(HttpStatusCode.OK, "application/json", content);
        
		var result = await _sut.PollMessagesAsync(request);
		Assert.NotEmpty(result);
		_httpHandler.Flush();
	}

	[Fact]
	public async Task GetMessagesAsync_ThrowsErrorResponseException_OnFailure()
	{
		var request = MessageFactory.CreateMessageFetchRequest();
		var content = JsonSerializer.Serialize(ErrorModelFactory.CreateErrorModelBadRequest(), _toSnakeCaseOptions);
		
		_httpHandler.When(HttpMethod.Get, 
        $"/streams/{request.StreamId}/topics/{request.TopicId}/messages?consumer_id={request.Consumer.Id}" +
                            $"&partition_id={request.PartitionId}&kind=offset&value={request.PollingStrategy.Value}&count={request.Count}" +
							$"&auto_commit={request.AutoCommit.ToString().ToLower()}")
					.Respond(HttpStatusCode.BadRequest, "application/json", content);
		
		await Assert.ThrowsAsync<InvalidResponseException>(async () => await _sut.PollMessagesAsync(request));
		_httpHandler.Flush();
	}

	
	[Fact]
	public async Task UpdateOffsetAsync_ThrowsErrorResponseException_OnFailure()
	{
		var contract = OffsetFactory.CreateOffsetContract();
		var error = JsonSerializer.Serialize(ErrorModelFactory.CreateErrorModelBadRequest(), _toSnakeCaseOptions);


		_httpHandler.When(HttpMethod.Put, $"/streams/{contract.StreamId}/topics/{contract.TopicId}/consumer-offsets")
			.Respond(HttpStatusCode.BadRequest, "application/json", error);
		
		await Assert.ThrowsAsync<InvalidResponseException>( async () => await _sut.StoreOffsetAsync(contract));
		_httpHandler.Flush();
	}

	[Fact]
	public async Task GetOffsetAsync_ReturnsOffsetResponse_WhenFound()
	{
		var request = OffsetFactory.CreateOffsetRequest();
		var response = OffsetFactory.CreateOffsetResponse();
		
        _httpHandler.When(HttpMethod.Get, $"/streams/{request.StreamId}/topics/{request.TopicId}/" +
                       $"consumer-offsets?consumer_id={request.Consumer.Id}&partition_id={request.PartitionId}")
					.Respond(HttpStatusCode.OK, "application/json", JsonSerializer.Serialize(response, _toSnakeCaseOptions));
        
		var result = await _sut.GetOffsetAsync(request);
		Assert.NotNull(result);
		Assert.Equal(response.ConsumerId, result.ConsumerId);
		Assert.Equal(response.Offset, result.Offset);
	}
	
	[Fact]
	public async Task GetOffsetAsync_ThrowsErrorResponseException_OnFailure()
	{
		var request = OffsetFactory.CreateOffsetRequest();
		var response = ErrorModelFactory.CreateErrorModelNotFound();
		
        _httpHandler.When(HttpMethod.Get, $"/streams/{request.StreamId}/topics/{request.TopicId}/" +
                       $"consumer-offsets?consumer_id={request.Consumer.Id}&partition_id={request.PartitionId}")
					.Respond(HttpStatusCode.NotFound, "application/json", JsonSerializer.Serialize(response, _toSnakeCaseOptions));
        
		await Assert.ThrowsAsync<InvalidResponseException>( async () => await _sut.GetOffsetAsync(request));
	}

	[Fact]
	public async Task GetGroupsAsync_ReturnsGroupResponse_WhenFound()
	{
		var streamId = Identifier.Numeric(1);
		var topicId = Identifier.Numeric(1);
		var response = GroupFactory.CreateGroupsResponse(3);

		_httpHandler.When($"/streams/{streamId}/topics/{topicId}/consumer-groups")
			.Respond(HttpStatusCode.OK, "application/json", JsonSerializer.Serialize(response, _toSnakeCaseOptions));
		
		var result = await _sut.GetConsumerGroupsAsync(streamId , topicId);
		Assert.NotNull(result);
		Assert.NotEmpty(result);
	}

	[Fact]
	public async Task GetGroupsAsync_ThrowsErrorResponseException_WhenNotFound()
	{
		var streamId = Identifier.Numeric(1);
		var topicId = Identifier.Numeric(1);
		var error = ErrorModelFactory.CreateErrorModelNotFound();

		_httpHandler.When($"/streams/{streamId}/topics/{topicId}/consumer-groups")
			.Respond(HttpStatusCode.NotFound, "application/json", JsonSerializer.Serialize(error, _toSnakeCaseOptions));

		await Assert.ThrowsAsync<InvalidResponseException>( async () => await _sut.GetConsumerGroupsAsync(streamId, topicId));
		_httpHandler.Flush();
		
	}

	[Fact]
	public async Task GetStats_ThrowsErrorResponseException_OnFailure()
	{
		var error = ErrorModelFactory.CreateErrorModelBadRequest();

		_httpHandler.When($"/stats")
			.Respond(HttpStatusCode.BadRequest, "application/json", JsonSerializer.Serialize(error, _toSnakeCaseOptions));
		
		await Assert.ThrowsAsync<InvalidResponseException>( async () => await _sut.GetStatsAsync());
	}

	[Fact]
	public async Task CreatePartitions_ThrowsErrorResponseException_OnFailure()
	{
		var request = PartitionFactory.CreatePartitionsRequest();
		var json = JsonSerializer.Serialize(request, _toSnakeCaseOptions); 
		
		var error = ErrorModelFactory.CreateErrorModelBadRequest();
		
		_httpHandler.When(HttpMethod.Post, $"/streams/{request.StreamId}/topics/{request.TopicId}/partitions")
			.With(message =>
			{
				message.Content = new StringContent(json, Encoding.UTF8, "application/json");
				return true;
			})
			.Respond(HttpStatusCode.BadRequest, "application/json", JsonSerializer.Serialize(error, _toSnakeCaseOptions));
		
		await Assert.ThrowsAsync<InvalidResponseException>( async () => await _sut.CreatePartitionsAsync(request));
	}

	[Fact]
	public async Task DeletePartitions_ThrowsErrorResponseException_OnFailure()
	{
		var request = PartitionFactory.CreateDeletePartitionsRequest();
		
		var error = ErrorModelFactory.CreateErrorModelBadRequest();
		
		_httpHandler.When(HttpMethod.Delete,
				$"/streams/{request.StreamId}/topics/{request.TopicId}/partitions?partitions_count={request.PartitionsCount}")
			.Respond(HttpStatusCode.BadRequest, "application/json", JsonSerializer.Serialize(error, _toSnakeCaseOptions));
		await Assert.ThrowsAsync<InvalidResponseException>( async () => await _sut.DeletePartitionsAsync(request));
	}
	
}
