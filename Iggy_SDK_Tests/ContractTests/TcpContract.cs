using System.Text;
using Iggy_SDK_Tests.Utils.Groups;
using Iggy_SDK_Tests.Utils.Messages;
using Iggy_SDK_Tests.Utils.Offset;
using Iggy_SDK_Tests.Utils.Streams;
using Iggy_SDK_Tests.Utils.Topics;
using Iggy_SDK.Contracts.Tcp;
using Iggy_SDK.Enums;

namespace Iggy_SDK_Tests.ContractTests;

public sealed class TcpContract
{
	public TcpContract()
	{
		
	}

    [Fact]
    public void TcpContracts_MessageFetchRequest_HasCorrectBytes()
    {
		const int messageBufferSize = 31;
        // Arrange
        var request = MessageFactory.CreateMessageFetchRequest();
        var result = new byte[messageBufferSize];
        
        // Act
        TcpContracts.GetMessages(result, request);
        
        // Assert
        Assert.Equal(result[0] switch { 0 => ConsumerType.Consumer , 1 => ConsumerType.ConsumerGroup,
            _ => throw new ArgumentOutOfRangeException()
        } , request.ConsumerType);
        Assert.Equal(request.ConsumerId, BitConverter.ToInt32(result[1..5]));
        Assert.Equal(request.StreamId, BitConverter.ToInt32(result[5..9]));
        Assert.Equal(request.TopicId, BitConverter.ToInt32(result[9..13]));
        Assert.Equal(request.PartitionId, BitConverter.ToInt32(result[13..17]));
        Assert.Equal(
            result[17] switch
            {
                0 => MessagePolling.Offset, 1 => MessagePolling.Timestamp, 2 => MessagePolling.First,
                3 => MessagePolling.Last, 4 => MessagePolling.Next,
                _ => throw new ArgumentOutOfRangeException()
            }, request.PollingStrategy);
        Assert.Equal(request.Value, BitConverter.ToUInt64(result[18..26]));
        Assert.Equal(request.Count, BitConverter.ToInt32(result[26..30]));
        Assert.Equal(request.AutoCommit, result[30] switch { 0 => false, 1 => true,
            _ => throw new ArgumentOutOfRangeException()
        });
    }

    [Fact]
    public void TcpContracts_MessageSendRequest_HasCorrectBytes()
    {
        // Arrange
        int streamId = 1;
        int topicId = 1;
        var request = MessageFactory.CreateMessageSendRequest();
        var messageBufferSize = request.Messages.Sum(message => 16 + 4 + message.Payload.Length)
	        + request.Key.Length + 14;
        var result = new byte[messageBufferSize];
        

        // Act
        TcpContracts.CreateMessage(result, streamId, topicId, request);
        
        //Assert
        Assert.Equal(streamId, BitConverter.ToInt32(result[0..4]));
        Assert.Equal(topicId, BitConverter.ToInt32(result[4..8]));
        Assert.Equal(request.Key.Kind, result[8] switch { 0 => KeyKind.None, 1 => KeyKind.PartitionId, 2 => KeyKind.EntityId,
            _ => throw new ArgumentOutOfRangeException()
        });
        Assert.Equal(request.Key.Length, result[9]);
        Assert.Equal(request.Key.Value.Length, result[10..(10 + request.Key.Length)].Length);
        Assert.Equal(request.Messages.Count(), BitConverter.ToInt32(result[(10 + request.Key.Length)..(10 + request.Key.Length + 4)]));
        
        int currentIndex = 10 + request.Key.Length + 4;
        foreach (var message in request.Messages)
        {
            // Assert
            Assert.Equal(message.Id, new Guid(result[currentIndex..(currentIndex + 16)]));
            currentIndex += 16;

            int payloadLength = BitConverter.ToInt32(result[currentIndex..(currentIndex + 4)]);
            currentIndex += 4;

            byte[] payload = result[currentIndex..(currentIndex + payloadLength)].ToArray();
            currentIndex += payloadLength;

            Assert.Equal(message.Payload.Length, payload.Length);
            Assert.Equal(message.Payload, payload);
        }
    }

    [Fact]
    public void TcpContracts_CreateStream_HasCorrectBytes()
    {
        // Arrange
        var request = StreamFactory.CreateStreamRequest();

        // Act
        var result = TcpContracts.CreateStream(request).AsSpan();

        // Assert
        int expectedBytesLength = sizeof(int) + request.Name.Length;

        Assert.Equal(expectedBytesLength, result.Length);
        Assert.Equal(request.StreamId, BitConverter.ToInt32(result[..4]));
        Assert.Equal(request.Name, Encoding.UTF8.GetString(result[4..]));
    }

    [Fact]
    public void TcpContracts_CreateGroup_HasCorrectBytes()
    {
        // Arrange
        int streamId = 1;
        int topicId = 1;
        var request = GroupFactory.CreateGroupRequest();

        // Act
        var result = TcpContracts.CreateGroup(streamId, topicId, request).AsSpan();

        // Assert
        int expectedBytesLength = sizeof(int) * 3;

        Assert.Equal(expectedBytesLength, result.Length);
        Assert.Equal(streamId, BitConverter.ToInt32(result[..4]));
        Assert.Equal(topicId, BitConverter.ToInt32(result[4..8]));
        Assert.Equal(request.ConsumerGroupId, BitConverter.ToInt32(result[8..12]));
    }

    [Fact]
    public void TcpContracts_DeleteGroup_HasCorrectBytes()
    {
        // Arrange
        int streamId = 1;
        int topicId = 1;
        int groupId = 1;

        // Act
        var result = TcpContracts.DeleteGroup(streamId, topicId, groupId).AsSpan();

        // Assert
        int expectedBytesLength = sizeof(int) * 3;

        Assert.Equal(expectedBytesLength, result.Length);
        Assert.Equal(streamId, BitConverter.ToInt32(result[..4]));
        Assert.Equal(topicId, BitConverter.ToInt32(result[4..8]));
        Assert.Equal(groupId, BitConverter.ToInt32(result[8..12]));
    }

    [Fact]
    public void TcpContracts_GetGroups_HasCorrectBytes()
    {
        // Arrange
        int streamId = 1;
        int topicId = 1;

        // Act
        var result = TcpContracts.GetGroups(streamId, topicId).AsSpan();

        // Assert
        int expectedBytesLength = sizeof(int) * 2;

        Assert.Equal(expectedBytesLength, result.Length);
        Assert.Equal(streamId, BitConverter.ToInt32(result[..4]));
        Assert.Equal(topicId, BitConverter.ToInt32(result[4..8]));
    }
    [Fact]
    public void TcpContracts_JoinGroup_HasCorrectBytes()
    {
        // Arrange
        var request = GroupFactory.CreateJoinGroupRequest();

        // Act
        var result = TcpContracts.JoinGroup(request).AsSpan();

        // Assert
        int expectedBytesLength = sizeof(int) * 3;

        Assert.Equal(expectedBytesLength, result.Length);
        Assert.Equal(request.StreamId, BitConverter.ToInt32(result[..4]));
        Assert.Equal(request.TopicId, BitConverter.ToInt32(result[4..8]));
        Assert.Equal(request.ConsumerGroupId, BitConverter.ToInt32(result[8..12]));
    }

    [Fact]
    public void TcpContracts_LeaveGroup_HasCorrectBytes()
    {
        // Arrange
        var request = GroupFactory.CreateLeaveGroupRequest();

        // Act
        var result = TcpContracts.LeaveGroup(request).AsSpan();

        // Assert
        int expectedBytesLength = sizeof(int) * 3;

        Assert.Equal(expectedBytesLength, result.Length);
        Assert.Equal(request.StreamId, BitConverter.ToInt32(result[..4]));
        Assert.Equal(request.TopicId, BitConverter.ToInt32(result[4..8]));
        Assert.Equal(request.ConsumerGroupId, BitConverter.ToInt32(result[8..12]));
    }

    [Fact]
    public void TcpContracts_GetGroup_HasCorrectBytes()
    {
        // Arrange
        int streamId = 1;
        int topicId = 1;
        int groupId = 1;

        // Act
        var result = TcpContracts.GetGroup(streamId, topicId, groupId).AsSpan();

        // Assert
        int expectedBytesLength = sizeof(int) * 3;

        Assert.Equal(expectedBytesLength, result.Length);
        Assert.Equal(streamId, BitConverter.ToInt32(result[..4]));
        Assert.Equal(topicId, BitConverter.ToInt32(result[4..8]));
        Assert.Equal(groupId, BitConverter.ToInt32(result[8..12]));
    }

    [Fact]
    public void TcpContracts_CreateTopic_HasCorrectBytes()
    {
        // Arrange
        int streamId = 1;
        var request = TopicFactory.CreateTopicRequest();

        // Act
        var result = TcpContracts.CreateTopic(streamId, request).AsSpan();

        // Assert
        int expectedBytesLength = sizeof(int) * 3 + request.Name.Length;

        Assert.Equal(expectedBytesLength, result.Length);
        Assert.Equal(streamId, BitConverter.ToInt32(result[..4]));
        Assert.Equal(request.TopicId, BitConverter.ToInt32(result[4..8]));
        Assert.Equal(request.PartitionsCount, BitConverter.ToInt32(result[8..12]));
        Assert.Equal(request.Name, Encoding.UTF8.GetString(result[12..]));
    }

    [Fact]
    public void TcpContracts_GetTopicById_HasCorrectBytes()
    {
        // Arrange
        int streamId = 1;
        int topicId = 1;

        // Act
        var result = TcpContracts.GetTopicById(streamId, topicId).AsSpan();

        // Assert
        int expectedBytesLength = sizeof(int) * 2;

        Assert.Equal(expectedBytesLength, result.Length);
        Assert.Equal(streamId, BitConverter.ToInt32(result[..4]));
        Assert.Equal(topicId, BitConverter.ToInt32(result[4..8]));
    }

    [Fact]
    public void TcpContracts_DeleteTopic_HasCorrectBytes()
    {
        // Arrange
        int streamId = 1;
        int topicId = 1;

        // Act
        var result = TcpContracts.DeleteTopic(streamId, topicId).AsSpan();

        // Assert
        int expectedBytesLength = sizeof(int) * 2;

        Assert.Equal(expectedBytesLength, result.Length);
        Assert.Equal(streamId, BitConverter.ToInt32(result[..4]));
        Assert.Equal(topicId, BitConverter.ToInt32(result[4..8]));
    }

    [Fact]
    public void TcpContracts_UpdateOffset_HasCorrectBytes()
    {
        // Arrange
        int streamId = 1;
        int topicId = 1;
        var contract = OffsetFactory.CreateOffsetContract();

        // Act
        var result = TcpContracts.UpdateOffset(streamId, topicId, contract).AsSpan();

        // Assert
        int expectedBytesLength = sizeof(int) * 4 + sizeof(ulong) + 1;

        Assert.Equal(expectedBytesLength, result.Length);
        Assert.Equal(0, result[0]);
        Assert.Equal(contract.ConsumerId, BitConverter.ToInt32(result[1..5]));
        Assert.Equal(streamId, BitConverter.ToInt32(result[5..9]));
        Assert.Equal(topicId, BitConverter.ToInt32(result[9..13]));
        Assert.Equal(contract.PartitionId, BitConverter.ToInt32(result[13..17]));
        Assert.Equal(contract.Offset, BitConverter.ToUInt64(result[17..25]));
    }

    [Fact]
    public void TcpContracts_GetOffset_HasCorrectBytes()
    {
        // Arrange
        var request = OffsetFactory.CreateOffsetRequest();

        // Act
        var result = TcpContracts.GetOffset(request).AsSpan();

        // Assert
        int expectedBytesLength = sizeof(int) * 4 + 1;

        Assert.Equal(expectedBytesLength, result.Length);
        Assert.Equal(0, result[0]);
        Assert.Equal(request.ConsumerId, BitConverter.ToInt32(result[1..5]));
        Assert.Equal(request.StreamId, BitConverter.ToInt32(result[5..9]));
        Assert.Equal(request.TopicId, BitConverter.ToInt32(result[9..13]));
        Assert.Equal(request.PartitionId, BitConverter.ToInt32(result[13..17]));
    }
		
}