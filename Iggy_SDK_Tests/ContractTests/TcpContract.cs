using System.Buffers.Binary;
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
        Assert.Equal(streamId, BitConverter.ToInt32(result[1..5]));
        Assert.Equal(topicId, BitConverter.ToInt32(result[5..9]));
        Assert.Equal(contract.ConsumerId, BitConverter.ToInt32(result[9..13]));
        Assert.Equal(contract.PartitionId, BitConverter.ToInt32(result[13..17]));
        Assert.Equal((ulong)contract.Offset, BitConverter.ToUInt64(result[17..25]));
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
        Assert.Equal(request.StreamId, BitConverter.ToInt32(result[1..5]));
        Assert.Equal(request.TopicId, BitConverter.ToInt32(result[5..9]));
        Assert.Equal(request.ConsumerId, BitConverter.ToInt32(result[9..13]));
        Assert.Equal(request.PartitionId, BitConverter.ToInt32(result[13..17]));
    }
		
}