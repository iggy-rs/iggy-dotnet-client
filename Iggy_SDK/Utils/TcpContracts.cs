using System.ComponentModel;
using System.Text;
using Iggy_SDK.Contracts;

namespace ConsoleApp;

internal static class TcpContracts
{
	internal static byte[] CreateStream(StreamRequest request)
	{
		byte[] streamIdBytes = BitConverter.GetBytes(request.StreamId);
		byte[] nameBytes = Encoding.UTF8.GetBytes(request.Name);
		byte[] bytes = new byte[streamIdBytes.Length + nameBytes.Length];
		streamIdBytes.CopyTo(bytes, 0);
		nameBytes.CopyTo(bytes, streamIdBytes.Length);
		return bytes;
	}

	internal static byte[] CreateGroup(int streamId, int topicId, GroupRequest request)
	{
		byte[] streamIdBytes = BitConverter.GetBytes(streamId);
		byte[] topicIdBytes = BitConverter.GetBytes(topicId);
		byte[] groupIdBytes = BitConverter.GetBytes(request.GroupId);
		byte[] bytes = new byte[streamIdBytes.Length + topicIdBytes.Length + groupIdBytes.Length];
		streamIdBytes.CopyTo(bytes, 0);
		topicIdBytes.CopyTo(bytes, streamIdBytes.Length);
		groupIdBytes.CopyTo(bytes, streamIdBytes.Length + topicIdBytes.Length);
		return bytes;
	}

	internal static byte[] DeleteGroup(int streamId, int topicId, int groupId)
	{
		byte[] streamIdBytes = BitConverter.GetBytes(streamId);
		byte[] topicIdBytes = BitConverter.GetBytes(topicId);
		byte[] groupIdBytes = BitConverter.GetBytes(groupId);
		byte[] bytes = new byte[streamIdBytes.Length + topicIdBytes.Length + groupIdBytes.Length];
		streamIdBytes.CopyTo(bytes, 0);
		topicIdBytes.CopyTo(bytes, streamIdBytes.Length);
		groupIdBytes.CopyTo(bytes, streamIdBytes.Length + topicIdBytes.Length);
		return bytes;
	}
	internal static byte[] GetGroups(int streamId, int topicId)
	{
		byte[] streamIdBytes = BitConverter.GetBytes(streamId);
		byte[] topicIdBytes = BitConverter.GetBytes(topicId);
		byte[] bytes = new byte[streamIdBytes.Length + topicIdBytes.Length];
		streamIdBytes.CopyTo(bytes, 0);
		topicIdBytes.CopyTo(bytes, streamIdBytes.Length);
		return bytes;
	}
	internal static byte[] GetGroup(int streamId, int topicId, int groupId)
	{
		byte[] streamIdBytes = BitConverter.GetBytes(streamId);
		byte[] topicIdBytes = BitConverter.GetBytes(topicId);
		byte[] groupIdBytes = BitConverter.GetBytes(groupId);
		byte[] bytes = new byte[streamIdBytes.Length + topicIdBytes.Length + groupIdBytes.Length];
		streamIdBytes.CopyTo(bytes, 0);
		topicIdBytes.CopyTo(bytes, streamIdBytes.Length);
		groupIdBytes.CopyTo(bytes, streamIdBytes.Length + topicIdBytes.Length);
		return bytes;
	}
	internal static byte[] CreateTopic(int streamId, TopicRequest request)
	{
		byte[] streamIdBytes = BitConverter.GetBytes(streamId);
		byte[] topicIdBytes = BitConverter.GetBytes(request.TopicId);
		byte[] partitionsCountBytes = BitConverter.GetBytes(request.PartitionsCount);
		byte[] nameBytes = Encoding.UTF8.GetBytes(request.Name);

		byte[] bytes = new byte[streamIdBytes.Length + topicIdBytes.Length + partitionsCountBytes.Length +
		                        nameBytes.Length];
		streamIdBytes.CopyTo(bytes, 0);
		topicIdBytes.CopyTo(bytes, streamIdBytes.Length);
		partitionsCountBytes.CopyTo(bytes, streamIdBytes.Length + topicIdBytes.Length);
		nameBytes.CopyTo(bytes, streamIdBytes.Length + topicIdBytes.Length + partitionsCountBytes.Length);

		return bytes;
	}
	
	internal static byte[] GetTopicById(int streamId, int topicId)
	{
		var message = new byte[8];
		var streamIdBytes = BitConverter.GetBytes(streamId);
		var topicIdBytes = BitConverter.GetBytes(topicId);
		Array.Copy(streamIdBytes, 0, message, 0, 4);
		Array.Copy(topicIdBytes, 0, message, 4, 4);
		return message;
	}
	internal static byte[] DeleteTopic(int streamId, int topicId)
	{
		var bytes = new byte[8];
		var streamIdBytes = BitConverter.GetBytes(streamId);
		var topicIdBytes = BitConverter.GetBytes(topicId);
		Array.Copy(streamIdBytes, 0, bytes, 0, 4);
		Array.Copy(topicIdBytes, 0, bytes, 4, 4);
		return bytes;
	}

	internal static byte[] UpdateOffset(int streamId, int topicId, OffsetContract contract)
	{
		var bytes = new byte[25];
		var streamIdBytes = BitConverter.GetBytes(streamId);
		var topicIdBytes = BitConverter.GetBytes(topicId);
		var consumerIdBytes = BitConverter.GetBytes(contract.ConsumerId);
		var partitionIdBytes = BitConverter.GetBytes(contract.PartitionId);
		var offsetBytes = BitConverter.GetBytes((ulong)contract.Offset);
		bytes[0] = 0;
		Array.Copy(streamIdBytes, 0, bytes, 1, 4);
		Array.Copy(topicIdBytes, 0, bytes, 5, 4);
		Array.Copy(consumerIdBytes, 0, bytes, 9, 4);
		Array.Copy(partitionIdBytes, 0, bytes, 13, 4);
		Array.Copy(offsetBytes, 0, bytes, 17, 8);
		return bytes;
	}
	internal static byte[] GetOffset(OffsetRequest request)
	{
		var bytes = new byte[17];
		var streamIdBytes = BitConverter.GetBytes(request.StreamId);
		var topicIdBytes = BitConverter.GetBytes(request.TopicId);
		var consumerIdBytes = BitConverter.GetBytes(request.ConsumerId);
		var partitionIdBytes = BitConverter.GetBytes(request.PartitionId);
		bytes[0] = 0;
		Array.Copy(streamIdBytes, 0, bytes, 1, 4);
		Array.Copy(topicIdBytes, 0, bytes, 5, 4);
		Array.Copy(consumerIdBytes, 0, bytes, 9, 4);
		return bytes;
	}
}