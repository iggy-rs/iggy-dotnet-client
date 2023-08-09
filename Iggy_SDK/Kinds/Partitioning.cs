using System.Buffers.Binary;
using System.Text;
using Iggy_SDK.Enums;

namespace Iggy_SDK.Kinds;

public sealed class Partitioning 
	{
	public required PartitioningKind Kind { get; init; }
	public required int Length { get; init; }
	public required byte[] Value { get; init; }

	public static Partitioning None()
	{
		return new Partitioning
		{
			Kind = PartitioningKind.None,
			Length = 0,
			Value = Array.Empty<byte>()
		};
	}
	public static Partitioning PartitionId(int value)
	{
		Span<byte> bytes = stackalloc byte[4];
		BinaryPrimitives.WriteInt32LittleEndian(bytes, value);

		return new Partitioning
		{
			Kind = PartitioningKind.PartitionId,
			Length = 4,
			Value = bytes.ToArray(),
		};
	}
	public static Partitioning EntityIdString(string value)
	{
		if (value.Length is 0 or > 255)
		{
			throw new ArgumentException("Value is too long", nameof(value));
		}
		return new Partitioning
		{
			Kind = PartitioningKind.EntityId,
			Length = value.Length,
			Value = Encoding.UTF8.GetBytes(value)
		};
	}
	public static Partitioning EntityIdBytes(byte[] value)
	{
		if (value.Length is 0 or > 255)
		{
			throw new ArgumentException("Value is too long", nameof(value));
		}
		return new Partitioning
		{
			Kind = PartitioningKind.EntityId,
			Length = value.Length,
			Value = value
		};
	}
	public static Partitioning EntityIdInt(int value)
	{
		Span<byte> bytes = stackalloc byte[4];
		BinaryPrimitives.WriteInt32LittleEndian(bytes, value);
		return new Partitioning
		{
			Kind = PartitioningKind.EntityId,
			Length = 4,
			Value = bytes.ToArray()
		};
	}
	public static Partitioning EntityIdUlong(ulong value)
	{
		Span<byte> bytes = stackalloc byte[8];
		BinaryPrimitives.WriteUInt64LittleEndian(bytes, value);
		return new Partitioning
		{
			Kind = PartitioningKind.EntityId,
			Length = 8,
			Value = bytes.ToArray()
		};
	}
	public static Partitioning EntityIdGuid(Guid value)
	{
		var bytes = value.ToByteArray();
		return new Partitioning
		{
			Kind = PartitioningKind.EntityId,
			Length = 16,
			Value = bytes
		};
	}
}
	
