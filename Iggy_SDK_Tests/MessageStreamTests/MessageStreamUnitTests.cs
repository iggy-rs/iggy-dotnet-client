using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Iggy_SDK_Tests.Utils.Messages;
using Iggy_SDK_Tests.Utils.Offset;
using Iggy_SDK_Tests.Utils.Streams;
using Iggy_SDK_Tests.Utils.Topics;
using Iggy_SDK.Contracts;
using Iggy_SDK.MessageStream;
using Iggy_SDK.SerializationConfiguration;
using RichardSzalay.MockHttp;

namespace Iggy_SDK_Tests.MessageStreamTests;

public sealed class MessageStreamUnitTests
{
	private readonly MockHttpMessageHandler _httpHandler;
	private readonly IMessageStream _sut;
	
	private JsonSerializerOptions _toSnakeCaseOptions;
	
	private const string URL = "http://localhost:3000";
	
	public MessageStreamUnitTests()
	{
		_toSnakeCaseOptions = new();
		_toSnakeCaseOptions.PropertyNamingPolicy = new ToSnakeCaseNamingPolicy();
		_toSnakeCaseOptions.WriteIndented = true;
        _toSnakeCaseOptions.Converters.Add(new UInt128Conveter());
        _toSnakeCaseOptions.Converters.Add(new JsonStringEnumConverter(new ToSnakeCaseNamingPolicy()));		_httpHandler = new MockHttpMessageHandler();
        
		var client = _httpHandler.ToHttpClient();
		client.BaseAddress = new Uri(URL);
		_sut = new HttpMessageStream(client);
	}

	
	[Fact]
	public async Task CreateStreamAsync_Returns201Created_WhenSuccessfullyCreated()
	{
		var streamId = 1;
		var content = StreamFactory.CreateStreamRequest();
		var json = JsonSerializer.Serialize(content, _toSnakeCaseOptions);

		_httpHandler.When(HttpMethod.Post, $"/streams")
			.WithContent(json)
			.Respond(HttpStatusCode.Created);
		
		var result = await _sut.CreateStreamAsync(content);
		Assert.True(result);
		_httpHandler.Flush();
	}
	
	[Fact]
	public async Task CreateStreamAsync_Returns400BadRequest_OnFailure()
	{
		var streamId = 1;
		var content = StreamFactory.CreateStreamRequest();
		var json = JsonSerializer.Serialize(content, _toSnakeCaseOptions);

		_httpHandler.When(HttpMethod.Post, $"/streams")
			.WithContent(json)
			.Respond(HttpStatusCode.BadRequest);
		
		var result = await _sut.CreateStreamAsync(content);
		Assert.False(result);
		_httpHandler.Flush();
	}
	
	[Fact]
	public async Task DeleteStreamAsync_Returns204NoContent_WhenSuccessfullyCreated()
	{
		var streamId = 1;

		_httpHandler.When(HttpMethod.Delete, $"/streams/{streamId}")
			.Respond(HttpStatusCode.NoContent);
		
		var result = await _sut.DeleteStreamAsync(streamId);
		Assert.True(result);
		_httpHandler.Flush();
	}

	[Fact]
	public async Task GetStreamByIdAsync_Returns200Ok_WhenFound()
	{
		var streamId = 1;
		var streamResponse = StreamFactory.CreateStreamResponse();
		var content = JsonSerializer.Serialize(streamResponse, _toSnakeCaseOptions);

		_httpHandler.When(HttpMethod.Get, $"/streams/{streamId}")
			.Respond(HttpStatusCode.OK, "application/json", content);
		
		var result = await _sut.GetStreamByIdAsync(streamId);
		Assert.NotNull(result);
		_httpHandler.Flush();
	}	
	
	[Fact]
	public async Task GetStreamByIdAsync_Returns404NotFound_OnFailure()
	{
		var streamId = 1;
		
		_httpHandler.When(HttpMethod.Get, $"/streams/{streamId}")
			.Respond(HttpStatusCode.NotFound);
		
		var result = await _sut.GetStreamByIdAsync(streamId);
		Assert.Null(result);
		_httpHandler.Flush();
	}

	[Fact]
	public async Task GetStreamsAsync_Returns200Ok_WhenFound()
	{
		var response = new List<StreamsResponse>();
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
	public async Task GetStreamsAsync_Returns404NotFound_OnFailure()
	{
		var response = new List<StreamsResponse>();
		var content = JsonSerializer.Serialize(response, _toSnakeCaseOptions);

		_httpHandler.When(HttpMethod.Get, $"/streams")
			.Respond(HttpStatusCode.NotFound, "application/json", content);


		var result = await _sut.GetStreamsAsync();
		Assert.Empty(result);
		_httpHandler.Flush();
	}

	[Fact]
	public async Task CreateTopicAsync_Returns201Created_WhenSuccessfullyCreated()
	{
		var streamId = 1;
		var content = TopicFactory.CreateTopicRequest();
		var json = JsonSerializer.Serialize(content, _toSnakeCaseOptions);

		_httpHandler.When(HttpMethod.Post, $"/streams/{streamId}/topics")
			.WithContent(json)
			.Respond(HttpStatusCode.Created);

		var result = await _sut.CreateTopicAsync(streamId, content);
		Assert.True(result);
		_httpHandler.Flush();
	}
	[Fact]
	public async Task CreateTopicAsync_Returns400BadRequest_OnFailure()
	{
		var streamId = 1;
		var content = TopicFactory.CreateTopicRequest();
		var json = JsonSerializer.Serialize(content, _toSnakeCaseOptions);

		_httpHandler.When(HttpMethod.Post, $"/streams/{streamId}/topics")
			.WithContent(json)
			.Respond(HttpStatusCode.BadRequest);

		var result = await _sut.CreateTopicAsync(streamId, content);
		Assert.False(false);
		_httpHandler.Flush();
	}
	
	[Fact]	
	public async Task DeleteTopicAsync_Returns204NoContent_WhenSuccessfullyCreated()
	{
		var streamId = 1;
		var topicId = 1;

		_httpHandler.When(HttpMethod.Delete, $"/streams/{streamId}/topics/{topicId}")
			.Respond(HttpStatusCode.NoContent);

		var result = await _sut.DeleteTopicAsync(streamId, topicId);
		Assert.True(result);
		_httpHandler.Flush();
	}

	[Fact]
	public async Task DeleteTopicAsync_Returns400BadRequest_OnFailure()
	{
		var streamId = 1;
		var topicId = 1;

		_httpHandler.When(HttpMethod.Delete, $"/streams/{streamId}/topics/{topicId}")
			.Respond(HttpStatusCode.BadRequest);

		var result = await _sut.DeleteTopicAsync(streamId, topicId);
		Assert.False(result);
		_httpHandler.Flush();
	}
	
	[Fact]
	public async Task GetTopicsAsync_Returns200Ok_WhenFound()
	{
		int streamId = 1;
		
		var response = new List<TopicsResponse>();
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
	public async Task GetTopicsAsync_Returns404NotFound_OnFailure()
	{
		int streamId = 1;
		
		_httpHandler.When(HttpMethod.Get, $"/streams/{streamId}/topics")
			.Respond(HttpStatusCode.NotFound);
		
		var result = await _sut.GetTopicsAsync(streamId);
		Assert.Empty(result);
		_httpHandler.Flush();
		
	}
	
	[Fact]
	public async Task GetTopicByIdAsync_Returns200Ok_WhenFound()
	{
		int streamId = 1;
		int topicId = 1;
		TopicsResponse? topic = TopicFactory.CreateTopicsResponse();
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
	public async Task GetTopicByIdAsync_Returns404NotFound_OnFailure()
	{
		int streamId = 1;
		int topicId = 1;
		
		_httpHandler.When(HttpMethod.Get, $"/streams/{streamId}/topics/{topicId}")
			.Respond(HttpStatusCode.NotFound);
		
		var result = await _sut.GetTopicByIdAsync(streamId, topicId);
		Assert.Null(result);
		_httpHandler.Flush();
	}

	[Fact]
	public async Task SendMessageAsync_Returns201Created_WhenSuccessfullyCreated()
	{
		var request = MessageFactory.CreateMessageSendRequest();
		
		_httpHandler.When(HttpMethod.Post, $"/streams/{request.StreamId}/topics/{request.TopicId}/messages")
			.WithContent(JsonSerializer.Serialize(request, _toSnakeCaseOptions))
			.Respond(HttpStatusCode.Created);
		
		var result = await _sut.SendMessagesAsync(request);
		Assert.True(result);
		_httpHandler.Flush();
	}

	[Fact]
	public async Task SendMessageAsync_Returns400BadRequest_OnFailure()
	{
		var request = MessageFactory.CreateMessageSendRequest();		
		
		_httpHandler.When(HttpMethod.Post, $"/streams/{request.StreamId}/topics/{request.TopicId}/messages")
			.WithContent(JsonSerializer.Serialize(request, _toSnakeCaseOptions))
			.Respond(HttpStatusCode.BadRequest);
		
		var result = await _sut.SendMessagesAsync(request);
		Assert.False(result);
		_httpHandler.Flush();
	}

	[Fact]
	public async Task GetMessagesAsync_Returns200Ok_WhenFound()
	{
		var request = MessageFactory.CreateMessageFetchRequest();
		var response = new List<MessageResponse>();
		response.Add(MessageFactory.CreateMessageResponse());
		var content = JsonSerializer.Serialize(response, _toSnakeCaseOptions); 
		
		_httpHandler.When(HttpMethod.Get, 
        $"/streams/{request.StreamId}/topics/{request.TopicId}/messages?consumer_id={request.ConsumerId}" +
                            $"&partition_id={request.PartitionId}&kind=offset&value={request.Value}&count={request.Count}" +
							$"&auto_commit={request.AutoCommit.ToString().ToLower()}")
					.Respond(HttpStatusCode.OK, "application/json", content);
        
		var result = await _sut.GetMessagesAsync(request);
		Assert.NotEmpty(result);
		_httpHandler.Flush();
	}

	[Fact]
	public async Task GetMessagesAsync_ReturnsEmpty_OnFailure()
	{
		var request = MessageFactory.CreateMessageFetchRequest();
		var response = new List<MessageResponse>();
		var content = JsonSerializer.Serialize(response, _toSnakeCaseOptions);
		
		_httpHandler.When(HttpMethod.Get, 
        $"/streams/{request.StreamId}/topics/{request.TopicId}/messages?consumer_id={request.ConsumerId}" +
                            $"&partition_id={request.PartitionId}&kind=offset&value={request.Value}&count={request.Count}" +
							$"&auto_commit={request.AutoCommit.ToString().ToLower()}")
					.Respond(HttpStatusCode.BadRequest, "application/json", content);
		
		var result = await _sut.GetMessagesAsync(request);
		Assert.Empty(result);
		_httpHandler.Flush();
	}

	[Fact]
	public async Task UpdateOffsetAsync_Returns204NoContent_WhenSuccessfullyUpdated()
	{
		int streamId = 1;
		int topicId = 1;
		var contract = OffsetFactory.CreateOffsetContract();

		_httpHandler.When(HttpMethod.Put, $"/streams/{streamId}/topics/{topicId}/messages/offsets")
			.Respond(HttpStatusCode.NoContent);
		
		var result = await _sut.UpdateOffsetAsync(streamId, topicId, contract);
		Assert.True(result);
		_httpHandler.Flush();
	}
	
	[Fact]
	public async Task UpdateOffsetAsync_Returns400BadRequest_OnFailure()
	{
		int streamId = 1;
		int topicId = 1;
		var contract = OffsetFactory.CreateOffsetContract();

		_httpHandler.When(HttpMethod.Put, $"/streams/{streamId}/topics/{topicId}/messages/offsets")
			.Respond(HttpStatusCode.BadRequest);
		
		var result = await _sut.UpdateOffsetAsync(streamId, topicId, contract);
		Assert.False(result);
		_httpHandler.Flush();
	}

	[Fact]
	public async Task GetOffsetAsync_Returns200Ok_WhenFound()
	{
		var request = OffsetFactory.CreateOffsetRequest();
		var response = OffsetFactory.CreateOffsetResponse();
		
        _httpHandler.When(HttpMethod.Get, $"/streams/{request.StreamId}/topics/{request.TopicId}/messages/" +
                       $"offsets?consumer_id={request.ConsumerId}&partition_id={request.PartitionId}")
					.Respond(HttpStatusCode.OK, "application/json", JsonSerializer.Serialize(response, _toSnakeCaseOptions));
        
		var result = await _sut.GetOffsetAsync(request);
		Assert.NotNull(result);
		Assert.Equal(response.ConsumerId, result.ConsumerId);
		Assert.Equal(response.Offset, result.Offset);
	}
	
	[Fact]
	public async Task GetOffsetAsync_Returns404NotFound_OnFailure()
	{
		var request = OffsetFactory.CreateOffsetRequest();
		var response = OffsetFactory.CreateOffsetResponse();
		
        _httpHandler.When(HttpMethod.Get, $"/streams/{request.StreamId}/topics/{request.TopicId}/messages/" +
                       $"offsets?consumer_id={request.ConsumerId}&partition_id={request.PartitionId}")
					.Respond(HttpStatusCode.NotFound, "application/json", JsonSerializer.Serialize(response, _toSnakeCaseOptions));
        
		var result = await _sut.GetOffsetAsync(request);
		Assert.Null(result);
	}
}
