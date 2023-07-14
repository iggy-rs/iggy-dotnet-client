using Iggy_SDK_Tests.Utils;
using Iggy_SDK.Contracts;
using Iggy_SDK.Mappers;

namespace Iggy_SDK_Tests.MapperTests;

public sealed class BinaryMapperTests
{
	[Fact]
    public void MapOffsets_ReturnsValidOffsetResponse()
    {
        // Arrange
        int consumerId = 123;
        int offset = 456;
        byte[] payload = BinaryFactory.CreateOffsetPayload(consumerId, offset);

        // Act
        OffsetResponse response = BinaryMapper.MapOffsets(payload);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(consumerId, response.ConsumerId);
        Assert.Equal(offset, response.Offset);
    }

    [Fact]
    public void MapMessages_ReturnsValidMessageResponses()
    {
        // Arrange
        int offset1 = 12;
        int timestamp1 = 69;
        int id1 = 3;
        string payloadString1 = "Test1";
        byte[] payload1 = BinaryFactory.CreateMessagePayload(offset1, timestamp1, id1, payloadString1);
        
        int offset2 = 234;
        int timestamp2 = 987654321;
        int id2 = 567;
        int messageLength2 = 8;
        string payloadString2 = "Test 2";
        byte[] payload2 = BinaryFactory.CreateMessagePayload(offset2, timestamp2, id2, payloadString2);


        byte[] combinedPayload = new byte[4 + payload1.Length + payload2.Length];
        for (int i = 4; i < payload1.Length + 4; i++)
        {
            combinedPayload[i] = payload1[i - 4];
        }

        for (int i = 0; i < payload2.Length; i++)
        {
            combinedPayload[4 + payload1.Length + i] = payload2[i];
        }
        

        // Act
        IEnumerable<MessageResponse> responses = BinaryMapper.MapMessages(combinedPayload).ToList();

        // Assert
        Assert.NotNull(responses);
        Assert.Equal(2, responses.Count());

        MessageResponse response1 = responses.ElementAt(0);
        Assert.Equal(offset1, response1.Offset);
        Assert.Equal((ulong)timestamp1, response1.Timestamp);
        Assert.Equal((ulong)id1, response1.Id);
        Assert.Equal(payloadString1, response1.Payload);

        MessageResponse response2 = responses.ElementAt(1);
        Assert.Equal(offset2, response2.Offset);
        Assert.Equal((ulong)timestamp2, response2.Timestamp);
        Assert.Equal((ulong)id2, response2.Id);
        Assert.Equal(payloadString2, response2.Payload);
    }

    [Fact]
    public void MapStreams_ReturnsValidStreamsResponses()
    {
        // Arrange
        int id1 = 123;
        int topicsCount1 = 3;
        string name1 = "Stream 1";
        byte[] payload1 = BinaryFactory.CreateStreamPayload(id1, topicsCount1, name1);

        int id2 = 456;
        int topicsCount2 = 2;
        string name2 = "Stream 2";
        byte[] payload2 = BinaryFactory.CreateStreamPayload(id2, topicsCount2, name2);

        byte[] combinedPayload = new byte[payload1.Length + payload2.Length];
        payload1.CopyTo(combinedPayload.AsSpan());
        payload2.CopyTo(combinedPayload.AsSpan(payload1.Length));

        // Act
        IEnumerable<StreamsResponse> responses = BinaryMapper.MapStreams(combinedPayload).ToList();

        // Assert
        Assert.NotNull(responses);
        Assert.Equal(2, responses.Count());

        var response1 = responses.ElementAt(0);
        Assert.Equal(id1, response1.Id);
        Assert.Equal(topicsCount1, response1.TopicsCount);
        Assert.Equal(name1, response1.Name);

        var response2 = responses.ElementAt(1);
        Assert.Equal(id2, response2.Id);
        Assert.Equal(topicsCount2, response2.TopicsCount);
        Assert.Equal(name2, response2.Name);
    }

    [Fact]
    public void MapStream_ReturnsValidStreamResponse()
    {
        // Arrange
        int streamId = 123;
        int topicsCount = 2;
        string streamName = "Stream 1";
        byte[] streamPayload = BinaryFactory.CreateStreamPayload(streamId, topicsCount, streamName);

        int topicId1 = 456;
        int partitionsCount1 = 3;
        string topicName1 = "Topic 1";
        byte[] topicPayload1 = BinaryFactory.CreateTopicPayload(topicId1, partitionsCount1, topicName1);

        byte[] topicCombinedPayload = new byte[topicPayload1.Length ];
        topicPayload1.CopyTo(topicCombinedPayload.AsSpan());

        byte[] streamCombinedPayload = new byte[streamPayload.Length + topicCombinedPayload.Length];
        streamPayload.CopyTo(streamCombinedPayload.AsSpan());
        topicCombinedPayload.CopyTo(streamCombinedPayload.AsSpan(streamPayload.Length));

        // Act
        var response = BinaryMapper.MapStream(streamCombinedPayload);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(streamId, response.Id);
        Assert.Equal(topicsCount, response.TopicsCount);
        Assert.Equal(streamName, response.Name);
        Assert.NotNull(response.Topics);
        Assert.Equal(1, response.Topics.ToList().Count);

        var topicResponse = response.Topics.First();
        Assert.Equal(topicId1, topicResponse.Id);
        Assert.Equal(partitionsCount1, topicResponse.PartitionsCount);
        Assert.Equal(topicName1, topicResponse.Name);
    }

    [Fact]
    public void MapTopics_ReturnsValidTopicsResponses()
    {
        // Arrange
        int id1 = 123;
        int partitionsCount1 = 2;
        string name1 = "Topic 1";
        byte[] payload1 = BinaryFactory.CreateTopicPayload(id1, partitionsCount1, name1);

        int id2 = 456;
        int partitionsCount2 = 3;
        string name2 = "Topic 2";
        byte[] payload2 = BinaryFactory.CreateTopicPayload(id2, partitionsCount2, name2);

        byte[] combinedPayload = new byte[payload1.Length + payload2.Length];
        payload1.CopyTo(combinedPayload.AsSpan());
        payload2.CopyTo(combinedPayload.AsSpan(payload1.Length));

        // Act
        var responses = BinaryMapper.MapTopics(combinedPayload);

        // Assert
        Assert.NotNull(responses);
        Assert.Equal(2, responses.Count());

        var response1 = responses.ElementAt(0);
        Assert.Equal(id1, response1.Id);
        Assert.Equal(partitionsCount1, response1.PartitionsCount);
        Assert.Equal(name1, response1.Name);

        var response2 = responses.ElementAt(1);
        Assert.Equal(id2, response2.Id);
        Assert.Equal(partitionsCount2, response2.PartitionsCount);
        Assert.Equal(name2, response2.Name);
    }

    [Fact]
    public void MapTopic_ReturnsValidTopicResponse()
    {
        // Arrange
        int topicId = 123;
        int partitionsCount = 3;
        string topicName = "Topic 1";
        byte[] topicPayload = BinaryFactory.CreateTopicPayload(topicId, partitionsCount, topicName);

        byte[] combinedPayload = new byte[topicPayload.Length];
        topicPayload.CopyTo(combinedPayload.AsSpan());

        // Act
        TopicsResponse response = BinaryMapper.MapTopic(combinedPayload);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(topicId, response.Id);
        Assert.Equal(topicName, response.Name);
    }

    [Fact]
    public void MapConsumerGroups_ReturnsValidConsumerGroupsResponses()
    {
        // Arrange
        int id1 = 123;
        int membersCount1 = 2;
        int partitionsCount1 = 4;
        byte[] payload1 = BinaryFactory.CreateGroupPayload(id1, membersCount1, partitionsCount1);

        int id2 = 456;
        int membersCount2 = 3;
        int partitionsCount2 = 2;
        byte[] payload2 = BinaryFactory.CreateGroupPayload(id2, membersCount2, partitionsCount2);

        byte[] combinedPayload = new byte[payload1.Length + payload2.Length];
        payload1.CopyTo(combinedPayload.AsSpan());
        payload2.CopyTo(combinedPayload.AsSpan(payload1.Length));

        // Act
        List<GroupResponse> responses = BinaryMapper.MapConsumerGroups(combinedPayload);

        // Assert
        Assert.NotNull(responses);
        Assert.Equal(2, responses.Count);

        GroupResponse response1 = responses[0];
        Assert.Equal(id1, response1.Id);
        Assert.Equal(membersCount1, response1.MembersCount);
        Assert.Equal(partitionsCount1, response1.PartitionsCount);

        GroupResponse response2 = responses[1];
        Assert.Equal(id2, response2.Id);
        Assert.Equal(membersCount2, response2.MembersCount);
        Assert.Equal(partitionsCount2, response2.PartitionsCount);
    }

    [Fact]
    public void MapConsumerGroup_ReturnsValidConsumerGroupResponse()
    {
        // Arrange
        int groupId = 123;
        int membersCount = 2;
        int partitionsCount = 4;
        byte[] groupPayload = BinaryFactory.CreateGroupPayload(groupId, membersCount, partitionsCount);

        // Act
        GroupResponse response = BinaryMapper.MapConsumerGroup(groupPayload);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(groupId, response.Id);
        Assert.Equal(membersCount, response.MembersCount);
        Assert.Equal(partitionsCount, response.PartitionsCount);
    }
}