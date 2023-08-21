using System.Buffers.Binary;
using System.Text;
using Iggy_SDK_Tests.Utils;
using Iggy_SDK_Tests.Utils.DummyObj;
using Iggy_SDK_Tests.Utils.Groups;
using Iggy_SDK_Tests.Utils.Messages;
using Iggy_SDK_Tests.Utils.Stats;
using Iggy_SDK_Tests.Utils.Topics;
using Iggy_SDK.Contracts.Http;
using StreamFactory = Iggy_SDK_Tests.Utils.Streams.StreamFactory;

namespace Iggy_SDK_Tests.MapperTests;

public sealed class BinaryMapper
{
	[Fact]
    public void MapOffsets_ReturnsValidOffsetResponse()
    {
        // Arrange
        int consumerId = 123;
        int offset = 456;
        byte[] payload = BinaryFactory.CreateOffsetPayload(consumerId, offset);

        // Act
        OffsetResponse response = Iggy_SDK.Mappers.BinaryMapper.MapOffsets(payload);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(consumerId, response.ConsumerId);
        Assert.Equal(offset, response.Offset);
    }

    [Fact]
    public void MapMessagesTMessage_NoHeaders_ReturnsValidMessageResponse()
    {
        //Arrange
        Func<byte[], DummyMessage> deserializer = bytes =>
        {
            int id = BinaryPrimitives.ReadInt32LittleEndian(bytes);
            int textLength = BinaryPrimitives.ReadInt32LittleEndian(bytes[4..8]);
            string text = Encoding.UTF8.GetString(bytes[8..(8 + textLength)]);
            return new DummyMessage { Id = id, Text = text };
        };

        var (offset, timestamp, guid, headersLength, payload) = MessageFactory.CreateMessageResponseFieldsTMessage();
        byte[] msgOnePayload = BinaryFactory.CreateMessagePayload(offset, timestamp, 0,
            guid, payload);
        
        var (offset1, timestamp1, guid1, headersLength1, payload1) = MessageFactory.CreateMessageResponseFieldsTMessage();
        byte[] msgTwoPayload = BinaryFactory.CreateMessagePayload(offset1, timestamp1, 0,
            guid1, payload1);

        byte[] combinedPayload = new byte[4 + msgOnePayload.Length + msgTwoPayload.Length];
        for (int i = 4; i < msgOnePayload.Length + 4; i++)
        {
            combinedPayload[i] = msgOnePayload[i - 4];
        }
        for (int i = 0; i < msgTwoPayload.Length; i++)
        {
            combinedPayload[4 + msgOnePayload.Length + i] = msgTwoPayload[i];
        }
        //Act
        var response = Iggy_SDK.Mappers.BinaryMapper.MapMessages<DummyMessage>(combinedPayload, bytes =>
        {
            var id = BitConverter.ToInt32(bytes[..4]);
            var txtLength = BitConverter.ToInt32(bytes[4..8]);
            var text = Encoding.UTF8.GetString(bytes[txtLength..]);
            return new DummyMessage
            {
                Id = id,
                Text = text
            };
        }).ToList();
        //Assert 
        Assert.NotEmpty(response);
        Assert.Equal(2, response.Count);
        Assert.Equal(response[0].Id , guid);
        Assert.Equal(response[0].Offset, offset);
        Assert.Equal(response[0].Timestamp, timestamp);
        Assert.Equal(response[1].Id, guid1);
        Assert.Equal(response[1].Offset, offset1);
        Assert.Equal(response[1].Timestamp, timestamp1);
        Assert.Equal(response[0].Message.Id, deserializer(payload).Id);
        
    }
    [Fact]
    public void MapMessages_NoHeaders_ReturnsValidMessageResponses()
    {
        // Arrange
        var (offset, timestamp, guid, headersLength, payload) = MessageFactory.CreateMessageResponseFields();
        byte[] msgOnePayload = BinaryFactory.CreateMessagePayload(offset, timestamp, 0,
            guid, payload);
        var (offset1, timestamp1, guid1, headersLength2, payload1) = MessageFactory.CreateMessageResponseFields();
        byte[] msgTwoPayload = BinaryFactory.CreateMessagePayload(offset1, timestamp1, 0,
            guid1, payload1);

        byte[] combinedPayload = new byte[4 + msgOnePayload.Length + msgTwoPayload.Length];
        for (int i = 4; i < msgOnePayload.Length + 4; i++)
        {
            combinedPayload[i] = msgOnePayload[i - 4];
        }
        for (int i = 0; i < msgTwoPayload.Length; i++)
        {
            combinedPayload[4 + msgOnePayload.Length + i] = msgTwoPayload[i];
        }
        
        // Act
        IEnumerable<MessageResponse> responses = Iggy_SDK.Mappers.BinaryMapper.MapMessages(combinedPayload).ToList();

        // Assert
        Assert.NotNull(responses);
        Assert.Equal(2, responses.Count());

        MessageResponse response1 = responses.ElementAt(0);
        Assert.Equal(offset, response1.Offset);
        Assert.Equal(timestamp, response1.Timestamp);
        Assert.Equal(guid, response1.Id);
        Assert.Equal(payload, response1.Payload);

        MessageResponse response2 = responses.ElementAt(1);
        Assert.Equal(offset1, response2.Offset);
        Assert.Equal(timestamp1, response2.Timestamp);
        Assert.Equal(guid1, response2.Id);
        Assert.Equal(payload1, response2.Payload);
    }
    [Fact]
    public void MapStreams_ReturnsValidStreamsResponses()
    {
        // Arrange
        var (id1, topicsCount1, sizeBytes, messagesCount, name1) = StreamFactory.CreateStreamsResponseFields();
            byte[] payload1 = BinaryFactory.CreateStreamPayload(id1, topicsCount1, name1, sizeBytes, messagesCount);
        var (id2, topicsCount2, sizeBytes2, messagesCount2, name2) = StreamFactory.CreateStreamsResponseFields();
        byte[] payload2 = BinaryFactory.CreateStreamPayload(id2, topicsCount2, name2, sizeBytes2, messagesCount2);

        byte[] combinedPayload = new byte[payload1.Length + payload2.Length];
        payload1.CopyTo(combinedPayload.AsSpan());
        payload2.CopyTo(combinedPayload.AsSpan(payload1.Length));

        // Act
        IEnumerable<StreamResponse> responses = Iggy_SDK.Mappers.BinaryMapper.MapStreams(combinedPayload).ToList();

        // Assert
        Assert.NotNull(responses);
        Assert.Equal(2, responses.Count());

        var response1 = responses.ElementAt(0);
        Assert.Equal(id1, response1.Id);
        Assert.Equal(topicsCount1, response1.TopicsCount);
        Assert.Equal(sizeBytes , response1.SizeBytes);
        Assert.Equal(messagesCount, response1.MessagesCount);
        Assert.Equal(name1, response1.Name);

        var response2 = responses.ElementAt(1);
        Assert.Equal(id2, response2.Id);
        Assert.Equal(topicsCount2, response2.TopicsCount);
        Assert.Equal(sizeBytes2, response2.SizeBytes);
        Assert.Equal(messagesCount2, response2.MessagesCount);
        Assert.Equal(name2, response2.Name);
    }

    [Fact]
    public void MapStream_ReturnsValidStreamResponse()
    {
        // Arrange
        var ( id, topicsCount, sizeBytes, messagesCount, name) = StreamFactory.CreateStreamsResponseFields();
        byte[] streamPayload = BinaryFactory.CreateStreamPayload(id, topicsCount, name, sizeBytes, messagesCount);
        var (topicId1, partitionsCount1, topicName1, topicSizeBytes1 ,messagesCountTopic1) =
            TopicFactory.CreateTopicResponseFields();
        byte[] topicPayload1 = BinaryFactory.CreateTopicPayload(topicId1,
            partitionsCount1,
            topicName1,
            topicSizeBytes1,
            messagesCountTopic1);

        byte[] topicCombinedPayload = new byte[topicPayload1.Length ];
        topicPayload1.CopyTo(topicCombinedPayload.AsSpan());

        byte[] streamCombinedPayload = new byte[streamPayload.Length + topicCombinedPayload.Length];
        streamPayload.CopyTo(streamCombinedPayload.AsSpan());
        topicCombinedPayload.CopyTo(streamCombinedPayload.AsSpan(streamPayload.Length));

        // Act
        var response = Iggy_SDK.Mappers.BinaryMapper.MapStream(streamCombinedPayload);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(id, response.Id);
        Assert.Equal(topicsCount, response.TopicsCount);
        Assert.Equal(name, response.Name);
        Assert.Equal(sizeBytes, response.SizeBytes);
        Assert.Equal(messagesCount, response.MessagesCount);
        Assert.NotNull(response.Topics);
        Assert.Single(response.Topics.ToList());

        var topicResponse = response.Topics.First();
        Assert.Equal(topicId1, topicResponse.Id);
        Assert.Equal(partitionsCount1, topicResponse.PartitionsCount);
        Assert.Equal(messagesCountTopic1, topicResponse.MessagesCount);
        Assert.Equal(topicName1, topicResponse.Name);
    }

    [Fact]
    public void MapTopics_ReturnsValidTopicsResponses()
    {
        // Arrange
        var (id1, partitionsCount1, name1, sizeBytesTopic1, messagesCountTopic1) =
            TopicFactory.CreateTopicResponseFields();
        byte[] payload1 = BinaryFactory.CreateTopicPayload(id1, partitionsCount1, name1, sizeBytesTopic1, messagesCountTopic1);
        var (id2, partitionsCount2, name2, sizeBytesTopic2, messagesCountTopic2) =
            TopicFactory.CreateTopicResponseFields();
        byte[] payload2 = BinaryFactory.CreateTopicPayload(id2, partitionsCount2, name2, sizeBytesTopic2, messagesCountTopic2 );

        byte[] combinedPayload = new byte[payload1.Length + payload2.Length];
        payload1.CopyTo(combinedPayload.AsSpan());
        payload2.CopyTo(combinedPayload.AsSpan(payload1.Length));

        // Act
        var responses = Iggy_SDK.Mappers.BinaryMapper.MapTopics(combinedPayload);

        // Assert
        Assert.NotNull(responses);
        Assert.Equal(2, responses.Count());

        var response1 = responses.ElementAt(0);
        Assert.Equal(id1, response1.Id);
        Assert.Equal(partitionsCount1, response1.PartitionsCount);
        Assert.Equal(sizeBytesTopic1, response1.SizeBytes);
        Assert.Equal(messagesCountTopic1, response1.MessagesCount);
        Assert.Equal(name1, response1.Name);

        var response2 = responses.ElementAt(1);
        Assert.Equal(id2, response2.Id);
        Assert.Equal(sizeBytesTopic2, response2.SizeBytes);
        Assert.Equal(messagesCountTopic2, response2.MessagesCount);
        Assert.Equal(partitionsCount2, response2.PartitionsCount);
        Assert.Equal(name2, response2.Name);
    }

    [Fact]
    public void MapTopic_ReturnsValidTopicResponse()
    {
        // Arrange
        var (topicId, partitionsCount, topicName, sizeBytes, messagesCount) = TopicFactory.CreateTopicResponseFields();
        byte[] topicPayload = BinaryFactory.CreateTopicPayload(topicId, partitionsCount, topicName, sizeBytes, messagesCount);

        byte[] combinedPayload = new byte[topicPayload.Length];
        topicPayload.CopyTo(combinedPayload.AsSpan());

        // Act
        TopicResponse response = Iggy_SDK.Mappers.BinaryMapper.MapTopic(combinedPayload);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(messagesCount, response.MessagesCount);
        Assert.Equal(partitionsCount, response.PartitionsCount);
        Assert.Equal(sizeBytes, response.SizeBytes);
        Assert.Equal(topicId, response.Id);
        Assert.Equal(topicName, response.Name);
    }

    [Fact]
    public void MapConsumerGroups_ReturnsValidConsumerGroupsResponses()
    {
        // Arrange
        var(id1, membersCount1, partitionsCount1) = GroupFactory.CreateConsumerGroupResponseFields();
        byte[] payload1 = BinaryFactory.CreateGroupPayload(id1, membersCount1, partitionsCount1);
        var(id2, membersCount2, partitionsCount2) = GroupFactory.CreateConsumerGroupResponseFields();
        byte[] payload2 = BinaryFactory.CreateGroupPayload(id2, membersCount2, partitionsCount2);

        byte[] combinedPayload = new byte[payload1.Length + payload2.Length];
        payload1.CopyTo(combinedPayload.AsSpan());
        payload2.CopyTo(combinedPayload.AsSpan(payload1.Length));

        // Act
        List<ConsumerGroupResponse> responses = Iggy_SDK.Mappers.BinaryMapper.MapConsumerGroups(combinedPayload);

        // Assert
        Assert.NotNull(responses);
        Assert.Equal(2, responses.Count);

        ConsumerGroupResponse response1 = responses[0];
        Assert.Equal(id1, response1.Id);
        Assert.Equal(membersCount1, response1.MembersCount);
        Assert.Equal(partitionsCount1, response1.PartitionsCount);

        ConsumerGroupResponse response2 = responses[1];
        Assert.Equal(id2, response2.Id);
        Assert.Equal(membersCount2, response2.MembersCount);
        Assert.Equal(partitionsCount2, response2.PartitionsCount);
    }

    [Fact]
    public void MapConsumerGroup_ReturnsValidConsumerGroupResponse()
    {
        // Arrange
        var (groupId, membersCount, partitionsCount) = GroupFactory.CreateConsumerGroupResponseFields();
        byte[] groupPayload = BinaryFactory.CreateGroupPayload(groupId, membersCount, partitionsCount);

        // Act
        ConsumerGroupResponse response = Iggy_SDK.Mappers.BinaryMapper.MapConsumerGroup(groupPayload);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(groupId, response.Id);
        Assert.Equal(membersCount, response.MembersCount);
        Assert.Equal(partitionsCount, response.PartitionsCount);
    }

    [Fact]
    public void MapStats_ReturnsValidStatsResponse()
    {
        //Arrange
        var stats = StatsFactory.CreateFakeStatsObject();
        var payload = BinaryFactory.CreateStatsPayload(stats);
        
        //Act
        var response = Iggy_SDK.Mappers.BinaryMapper.MapStats(payload);
        
        //Assert
        Assert.Equal(stats.ProcessId, response.ProcessId);
        Assert.Equal(stats.MessagesCount, response.MessagesCount);
        Assert.Equal(stats.ConsumerGroupsCount, response.ConsumerGroupsCount);
        Assert.Equal(stats.TopicsCount, response.TopicsCount);
        Assert.Equal(stats.StreamsCount, response.StreamsCount);
        Assert.Equal(stats.PartitionsCount, response.PartitionsCount);
        Assert.Equal(stats.SegmentsCount, response.SegmentsCount);
        Assert.Equal(stats.MessagesSizeBytes, response.MessagesSizeBytes);
        Assert.Equal(stats.CpuUsage, response.CpuUsage);
        Assert.Equal(stats.TotalMemory, response.TotalMemory);
        Assert.Equal(stats.AvailableMemory, response.AvailableMemory);
        Assert.Equal(stats.MemoryUsage, response.MemoryUsage);
        Assert.Equal(stats.RunTime, response.RunTime);
        Assert.Equal(stats.StartTime, response.StartTime);
        Assert.Equal(stats.ReadBytes, response.ReadBytes);
        Assert.Equal(stats.WrittenBytes, stats.WrittenBytes);
        Assert.Equal(stats.ClientsCount, response.ClientsCount);
        Assert.Equal(stats.ConsumerGroupsCount, response.ConsumerGroupsCount);
        Assert.Equal(stats.Hostname, response.Hostname);
        Assert.Equal(stats.OsName, response.OsName);
        Assert.Equal(stats.OsVersion, stats.OsVersion);
        Assert.Equal(stats.KernelVersion, response.KernelVersion);
    
    }
}