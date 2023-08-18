using System.Buffers.Binary;
using System.Text;
using Iggy_SDK;
using Iggy_SDK_Tests.Utils.Groups;
using Iggy_SDK_Tests.Utils.Messages;
using Iggy_SDK_Tests.Utils.Offset;
using Iggy_SDK_Tests.Utils.Partitions;
using Iggy_SDK_Tests.Utils.Streams;
using Iggy_SDK_Tests.Utils.Topics;
using Iggy_SDK.Contracts.Tcp;
using Iggy_SDK.Enums;
using Iggy_SDK.Kinds;

namespace Iggy_SDK_Tests.ContractTests;

public sealed class TcpContract
{
	public TcpContract()
	{
		
	}

    [Fact]
    public void TcpContracts_MessageFetchRequest_HasCorrectBytes()
    {
		const int messageBufferSize = 23 + 2 + 4 + 2 + 4;
        // Arrange
        var request = MessageFactory.CreateMessageFetchRequest();
        var result = new byte[messageBufferSize];
        
        // Act
        TcpContracts.GetMessages(result, request);
        
        // Assert
        Assert.Equal(result[0] switch { 1 => ConsumerType.Consumer , 2 => ConsumerType.ConsumerGroup,
            _ => throw new ArgumentOutOfRangeException()
        } , request.Consumer.Type);
        Assert.Equal(request.Consumer.Id, BitConverter.ToInt32(result[1..5]));
        Assert.Equal(request.StreamId.Value, BytesToIdentifierNumeric(result, 5).Value);
        Assert.Equal(request.TopicId.Value, BytesToIdentifierNumeric(result , 11).Value);
        Assert.Equal(request.StreamId.Kind, BytesToIdentifierNumeric(result, 5).Kind);
        Assert.Equal(request.TopicId.Kind, BytesToIdentifierNumeric(result , 11).Kind);
        Assert.Equal(request.StreamId.Length, BytesToIdentifierNumeric(result, 5).Length);
        Assert.Equal(request.TopicId.Length, BytesToIdentifierNumeric(result , 11).Length);
        Assert.Equal(request.PartitionId, BitConverter.ToInt32(result[17..21]));
        Assert.Equal(
            result[21] switch
            {
                1 => MessagePolling.Offset, 2 => MessagePolling.Timestamp, 3 => MessagePolling.First,
                4 => MessagePolling.Last, 5 => MessagePolling.Next,
                _ => throw new ArgumentOutOfRangeException()
            }, request.PollingStrategy.Kind);
        Assert.Equal(request.PollingStrategy.Value, BitConverter.ToUInt64(result[22..30]));
        Assert.Equal(request.Count, BitConverter.ToInt32(result[30..34]));
        Assert.Equal(request.AutoCommit, result[34] switch { 0 => false, 1 => true,
            _ => throw new ArgumentOutOfRangeException()
        });
    }

    
    
    [Fact]
    public void TcpContracts_MessageSendRequest_HasCorrectBytes()
    {
        // Arrange
        var streamId = Identifier.Numeric(1);
        var topicId = Identifier.Numeric(1);
        var request = MessageFactory.CreateMessageSendRequest();
        var messageBufferSize = request.Messages.Sum(message => 16 + 4 + message.Payload.Length)
	        + request.Partitioning.Length + 14;
        var result = new byte[messageBufferSize];
        

        // Act
        TcpContracts.CreateMessage(result, streamId, topicId, request.Partitioning, request.Messages);
        
        //Assert
        Assert.Equal(streamId.Value, BytesToIdentifierNumeric(result, 0).Value);
        Assert.Equal(topicId.Value, BytesToIdentifierNumeric(result, 6).Value);
        Assert.Equal(streamId.Length, BytesToIdentifierNumeric(result, 0).Length);
        Assert.Equal(topicId.Length, BytesToIdentifierNumeric(result, 6).Length);
        Assert.Equal(streamId.Kind, BytesToIdentifierNumeric(result, 0).Kind);
        Assert.Equal(topicId.Kind, BytesToIdentifierNumeric(result, 6).Kind);
        Assert.Equal(request.Partitioning.Kind, result[12] switch { 1 => PartitioningKind.Balanced, 2 => PartitioningKind.PartitionId, 3 => PartitioningKind.MessageKey,
            _ => throw new ArgumentOutOfRangeException()
        });
        Assert.Equal(request.Partitioning.Length, result[13]);
        Assert.Equal(request.Partitioning.Value.Length, result[14..(14 + request.Partitioning.Length)].Length);
        
        int currentIndex = 14 + request.Partitioning.Length;
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
        var streamId = Identifier.String("my-stream");
        var topicId = Identifier.String("my-topic");
        var request = GroupFactory.CreateGroupRequest();

        // Act
        var result = TcpContracts.CreateGroup(streamId, topicId, request).AsSpan();

        // Assert
        int expectedBytesLength = 2 + streamId.Length + 2 + topicId.Length + 4;

        Assert.Equal(expectedBytesLength, result.Length);
        Assert.Equal(streamId.Value, BytesToIdentifierString(result, 0).Value);
        Assert.Equal(topicId.Value, BytesToIdentifierString(result, 2 + streamId.Length).Value);
        Assert.Equal(streamId.Kind, BytesToIdentifierString(result, 0).Kind);
        Assert.Equal(topicId.Kind, BytesToIdentifierString(result, 2 + streamId.Length).Kind);
        Assert.Equal(streamId.Length, BytesToIdentifierString(result, 0).Length);
        Assert.Equal(topicId.Length, BytesToIdentifierString(result, 2 + streamId.Length).Length);
        var position = 2 + streamId.Length + 2 + topicId.Length;
        Assert.Equal(request.ConsumerGroupId, BitConverter.ToInt32(result[position..(position + 4)]));
    }

    [Fact]
    public void TcpContracts_DeleteGroup_HasCorrectBytes()
    {
        // Arrange
        var streamId = Identifier.Numeric(1);
        var topicId = Identifier.Numeric(1);
        int groupId = 1;

        // Act
        var result = TcpContracts.DeleteGroup(streamId, topicId, groupId).AsSpan();

        // Assert
        int expectedBytesLength = 2 + streamId.Length + 2 + topicId.Length + 4;

        Assert.Equal(expectedBytesLength, result.Length);
        Assert.Equal(streamId.Value, BytesToIdentifierNumeric(result, 0).Value);
        Assert.Equal(topicId.Value, BytesToIdentifierNumeric(result, 6).Value);
        Assert.Equal(streamId.Length, BytesToIdentifierNumeric(result, 0).Length);
        Assert.Equal(topicId.Length, BytesToIdentifierNumeric(result, 6).Length);
        Assert.Equal(streamId.Kind, BytesToIdentifierNumeric(result, 0).Kind);
        Assert.Equal(groupId, BitConverter.ToInt32(result[12..16]));
    }

    [Fact]
    public void TcpContracts_GetGroups_HasCorrectBytes()
    {
        // Arrange
        var streamId = Identifier.String("my-stream");
        var topicId = Identifier.Numeric(1);

        // Act
        var result = TcpContracts.GetGroups(streamId, topicId).AsSpan();

        // Assert
        int expectedBytesLength = 2 + streamId.Length + 2 + topicId.Length; 

        Assert.Equal(expectedBytesLength, result.Length);
        Assert.Equal(streamId.Value, BytesToIdentifierString(result, 0).Value);
        Assert.Equal(topicId.Value, BytesToIdentifierNumeric(result, 2 + streamId.Length).Value);
        Assert.Equal(streamId.Kind, BytesToIdentifierString(result, 0).Kind);
        Assert.Equal(topicId.Kind, BytesToIdentifierNumeric(result, 2 + streamId.Length).Kind);
        Assert.Equal(streamId.Length, BytesToIdentifierString(result, 0).Length);
        Assert.Equal(topicId.Length, BytesToIdentifierNumeric(result, 2 + streamId.Length).Length);
        
    }
    
    [Fact]
    public void TcpContracts_JoinGroup_HasCorrectBytes()
    {
        // Arrange
        var request = GroupFactory.CreateJoinGroupRequest();

        // Act
        var result = TcpContracts.JoinGroup(request).AsSpan();

        // Assert
        int expectedBytesLength = 2 + request.StreamId.Length + 2 + request.TopicId.Length + 4;

        Assert.Equal(expectedBytesLength, result.Length);
        Assert.Equal(request.StreamId.Value, BytesToIdentifierNumeric(result, 0).Value);
        Assert.Equal(request.TopicId.Value, BytesToIdentifierNumeric(result, 6).Value);
        Assert.Equal(request.StreamId.Length, BytesToIdentifierNumeric(result, 0).Length);
        Assert.Equal(request.StreamId.Kind, BytesToIdentifierNumeric(result, 0).Kind);
        Assert.Equal(request.TopicId.Kind, BytesToIdentifierNumeric(result, 6).Kind);
        Assert.Equal(request.TopicId.Length, BytesToIdentifierNumeric(result, 6).Length);
        Assert.Equal(request.ConsumerGroupId, BitConverter.ToInt32(result[12..16]));
    }

    
    [Fact]
    public void TcpContracts_LeaveGroup_HasCorrectBytes()
    {
        // Arrange
        var request = GroupFactory.CreateLeaveGroupRequest();

        // Act
        var result = TcpContracts.LeaveGroup(request).AsSpan();

        // Assert
        int expectedBytesLength = 2 + request.StreamId.Length + 2 + request.TopicId.Length + 4;

        Assert.Equal(expectedBytesLength, result.Length);
        Assert.Equal(request.StreamId.Value, BytesToIdentifierNumeric(result, 0).Value);
        Assert.Equal(request.TopicId.Value, BytesToIdentifierNumeric(result, 6).Value);
        Assert.Equal(request.StreamId.Length, BytesToIdentifierNumeric(result, 0).Length);
        Assert.Equal(request.StreamId.Kind, BytesToIdentifierNumeric(result, 0).Kind);
        Assert.Equal(request.TopicId.Kind, BytesToIdentifierNumeric(result, 6).Kind);
        Assert.Equal(request.TopicId.Length, BytesToIdentifierNumeric(result, 6).Length);
        Assert.Equal(request.ConsumerGroupId, BitConverter.ToInt32(result[12..16]));
    }

    
    [Fact]
    public void TcpContracts_GetGroup_HasCorrectBytes()
    {
        // Arrange
        var streamId = Identifier.String("my-stream");
        var topicId = Identifier.Numeric(1);
        int groupId = 1;

        // Act
        var result = TcpContracts.GetGroup(streamId, topicId, groupId).AsSpan();

        // Assert
        int expectedBytesLength = 2 + streamId.Length + 2 + topicId.Length + 4;

        Assert.Equal(expectedBytesLength, result.Length);
        Assert.Equal(streamId.Value, BytesToIdentifierString(result, 0).Value);
        Assert.Equal(topicId.Value, BytesToIdentifierNumeric(result, 2 + streamId.Length).Value);
        Assert.Equal(streamId.Kind, BytesToIdentifierString(result, 0).Kind);
        Assert.Equal(topicId.Kind, BytesToIdentifierNumeric(result, 2 + streamId.Length).Kind);
        Assert.Equal(streamId.Length, BytesToIdentifierString(result, 0).Length);
        Assert.Equal(topicId.Length, BytesToIdentifierNumeric(result, 2 + streamId.Length).Length);
        var position = 2 + streamId.Length + 2 + topicId.Length; 
        Assert.Equal(groupId, BitConverter.ToInt32(result[position..(position + 4)]));
    }
    

    [Fact]
    public void TcpContracts_CreateTopic_HasCorrectBytes()
    {
        // Arrange
        var streamId = Identifier.Numeric(1);
        var request = TopicFactory.CreateTopicRequest();

        // Act
        var result = TcpContracts.CreateTopic(streamId, request).AsSpan();

        // Assert
        int expectedBytesLength = 2 + streamId.Length + 8 + request.Name.Length;

        Assert.Equal(expectedBytesLength, result.Length);
        Assert.Equal(streamId.Value, BytesToIdentifierNumeric(result, 0).Value);
        Assert.Equal(streamId.Length, BytesToIdentifierNumeric(result, 0).Length);
        Assert.Equal(streamId.Kind, BytesToIdentifierNumeric(result, 0).Kind);
        Assert.Equal(request.TopicId, BitConverter.ToInt32(result[6..10]));
        Assert.Equal(request.PartitionsCount, BitConverter.ToInt32(result[10..14]));
        Assert.Equal(request.Name, Encoding.UTF8.GetString(result[14..]));
    }

    
    [Fact]
    public void TcpContracts_GetTopicById_HasCorrectBytes()
    {
        // Arrange
        var streamId = Identifier.Numeric(1);
        var topicId = Identifier.Numeric(1);

        // Act
        var result = TcpContracts.GetTopicById(streamId, topicId).AsSpan();

        // Assert
        int expectedBytesLength = 2 + streamId.Length + 2 + topicId.Length;

        Assert.Equal(expectedBytesLength, result.Length);
        Assert.Equal(streamId.Value, BytesToIdentifierNumeric(result, 0).Value);
        Assert.Equal(streamId.Length, BytesToIdentifierNumeric(result, 0).Length);
        Assert.Equal(streamId.Kind, BytesToIdentifierNumeric(result, 0).Kind);
        Assert.Equal(topicId.Value, BytesToIdentifierNumeric(result, 6).Value);
        Assert.Equal(topicId.Length, BytesToIdentifierNumeric(result, 6).Length);
        Assert.Equal(topicId.Kind, BytesToIdentifierNumeric(result, 6).Kind);
    }
    

    [Fact]
    public void TcpContracts_DeleteTopic_HasCorrectBytes()
    {
        // Arrange
        var streamId = Identifier.Numeric(1);
        var topicId = Identifier.Numeric(1);

        // Act
        var result = TcpContracts.DeleteTopic(streamId, topicId).AsSpan();

        // Assert
        int expectedBytesLength = 2 + streamId.Length + 2 + topicId.Length;

        Assert.Equal(expectedBytesLength, result.Length);
        Assert.Equal(streamId.Value, BytesToIdentifierNumeric(result, 0).Value);
        Assert.Equal(streamId.Length, BytesToIdentifierNumeric(result, 0).Length);
        Assert.Equal(streamId.Kind, BytesToIdentifierNumeric(result, 0).Kind);
        Assert.Equal(topicId.Value, BytesToIdentifierNumeric(result, 6).Value);
        Assert.Equal(topicId.Length, BytesToIdentifierNumeric(result, 6).Length);
        Assert.Equal(topicId.Kind, BytesToIdentifierNumeric(result, 6).Kind);
    }

    [Fact]
    public void TcpContracts_UpdateOffset_HasCorrectBytes()
    {
        // Arrange
        var streamId = Identifier.Numeric(1);
        var topicId = Identifier.Numeric(1);
        var contract = OffsetFactory.CreateOffsetContract();

        // Act
        var result = TcpContracts.UpdateOffset(streamId, topicId, contract).AsSpan();

        // Assert
        int expectedBytesLength = 2 + streamId.Length + 2 + topicId.Length + 5 + 4 + 8;

        Assert.Equal(expectedBytesLength, result.Length);
        Assert.Equal(1, result[0]);
        Assert.Equal(contract.Consumer.Id, BitConverter.ToInt32(result[1..5]));
        Assert.Equal(streamId.Value, BytesToIdentifierNumeric(result, 5).Value);
        Assert.Equal(streamId.Length, BytesToIdentifierNumeric(result, 5).Length);
        Assert.Equal(streamId.Kind, BytesToIdentifierNumeric(result, 5).Kind);
        Assert.Equal(topicId.Value, BytesToIdentifierNumeric(result, 11).Value);
        Assert.Equal(topicId.Length, BytesToIdentifierNumeric(result, 11).Length);
        Assert.Equal(topicId.Kind, BytesToIdentifierNumeric(result, 11).Kind);
        Assert.Equal(contract.PartitionId, BitConverter.ToInt32(result[17..21]));
        Assert.Equal(contract.Offset, BitConverter.ToUInt64(result[21..29]));
    }

    
    
    [Fact]
    public void TcpContracts_GetOffset_HasCorrectBytes()
    {
        // Arrange
        var request = OffsetFactory.CreateOffsetRequest();

        // Act
        var result = TcpContracts.GetOffset(request).AsSpan();

        // Assert
        int expectedBytesLength = 2 + request.StreamId.Length + 2 + request.TopicId.Length + 5 + 4;

        Assert.Equal(expectedBytesLength, result.Length);
        Assert.Equal(1, result[0]);
        Assert.Equal(request.Consumer.Id, BitConverter.ToInt32(result[1..5]));
        Assert.Equal(request.StreamId.Value, BytesToIdentifierNumeric(result, 5).Value);
        Assert.Equal(request.StreamId.Length, BytesToIdentifierNumeric(result, 5).Length);
        Assert.Equal(request.StreamId.Kind, BytesToIdentifierNumeric(result, 5).Kind);
        Assert.Equal(request.TopicId.Value, BytesToIdentifierNumeric(result, 11).Value);
        Assert.Equal(request.TopicId.Length, BytesToIdentifierNumeric(result, 11).Length);
        Assert.Equal(request.TopicId.Kind, BytesToIdentifierNumeric(result, 11).Kind);
        Assert.Equal(request.PartitionId, BitConverter.ToInt32(result[17..21]));
    }

    [Fact]
    public void TcpContracts_CreatePartitions_HasCorrectBytes()
    {
        // Arrange
        var streamId = Identifier.Numeric(1);
        var topicId = Identifier.Numeric(1);
        var request = PartitionFactory.CreatePartitionsRequest();
        
        // Act
        var result = TcpContracts.CreatePartitions(streamId, topicId, request).AsSpan();
        
        // Assert
        Assert.Equal(streamId.Value, BytesToIdentifierNumeric(result, 0).Value);
        Assert.Equal(streamId.Length, BytesToIdentifierNumeric(result, 0).Length);
        Assert.Equal(streamId.Kind, BytesToIdentifierNumeric(result, 0).Kind);
        Assert.Equal(topicId.Value, BytesToIdentifierNumeric(result, 6).Value);
        Assert.Equal(topicId.Length, BytesToIdentifierNumeric(result, 6).Length);
        Assert.Equal(topicId.Kind, BytesToIdentifierNumeric(result, 6).Kind);
        Assert.Equal(request.PartitionsCount, BitConverter.ToInt32(result[12..16]));
        
    }
		
    
    [Fact]
    public void TcpContracts_DeletePartitions_HasCorrectBytes()
    {
        // Arrange
        var streamId = Identifier.Numeric(1);
        var topicId = Identifier.Numeric(1);
        var request = PartitionFactory.CreateDeletePartitionsRequest();
        
        // Act
        var result = TcpContracts.DeletePartitions(streamId, topicId, request).AsSpan();
        
        // Assert
        Assert.Equal(streamId.Value, BytesToIdentifierNumeric(result, 0).Value);
        Assert.Equal(streamId.Length, BytesToIdentifierNumeric(result, 0).Length);
        Assert.Equal(streamId.Kind, BytesToIdentifierNumeric(result, 0).Kind);
        Assert.Equal(topicId.Value, BytesToIdentifierNumeric(result, 6).Value);
        Assert.Equal(topicId.Length, BytesToIdentifierNumeric(result, 6).Length);
        Assert.Equal(topicId.Kind, BytesToIdentifierNumeric(result, 6).Kind);
        Assert.Equal(request.PartitionsCount, BitConverter.ToInt32(result[12..16]));
        
    }
    

    private static Identifier BytesToIdentifierNumeric(Span<byte> bytes, int startPos)
    {
        var idKind = bytes[startPos] switch
        {
            1 => IdKind.Numeric,
            2 => IdKind.String,
            _ => throw new ArgumentOutOfRangeException()
        };
        var identifierLength = (int)bytes[startPos + 1];
        var valueBytes = new byte[identifierLength];
        for (int i = 0; i < identifierLength; i++)
        {
            valueBytes[i] = bytes[i + startPos + 2]; 
        }

        return new Identifier
        {
            Kind = IdKind.Numeric,
            Length = identifierLength,
            Value = valueBytes
        };
    }
    private static Identifier BytesToIdentifierString(Span<byte> bytes, int startPos)
    {
        var idKind = bytes[startPos] switch
        {
            1 => IdKind.Numeric,
            2 => IdKind.String,
            _ => throw new ArgumentOutOfRangeException()
        };
        var identifierLength = (int)bytes[startPos + 1];
        var valueBytes = new byte[identifierLength];
        for (int i = 0; i < identifierLength; i++)
        {
            valueBytes[i] = bytes[i + startPos + 2]; 
        }

        return new Identifier
        {
            Kind = IdKind.String,
            Length = identifierLength,
            Value = valueBytes
        };
    }
    private static void WriteBytesFromStreamAndTopicIdToSpan(Identifier streamId, Identifier topicId, Span<byte> bytes, int startPos = 0)
    {
        bytes[startPos] = streamId.Kind switch
        {
            IdKind.Numeric => 1,
            IdKind.String => 2,
            _ => throw new ArgumentOutOfRangeException()
        };
        bytes[startPos + 1] = (byte)streamId.Length;
        for (int i = 0; i < streamId.Length; i++)
        {
            bytes[i + startPos + 2] = streamId.Value[i];
        }

        int position = startPos + 2 + streamId.Length;
        bytes[position] = topicId.Kind switch
        {
            IdKind.Numeric => 1,
            IdKind.String => 2,
            _ => throw new ArgumentOutOfRangeException()
        };
        bytes[position + 1] = (byte)topicId.Length;
        for (int i = 0; i < topicId.Length; i++)
        {
            bytes[i + position + 2] = topicId.Value[i];
        }
    }
    
}