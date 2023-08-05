using System.Buffers.Binary;
using System.Text;
using Iggy_SDK.Enums;

namespace Iggy_SDK.Identifiers;

public sealed class Key
{
	public required KeyKind Kind { get; init; }
	public required int Length { get; init; }
	public required byte[] Value { get; init; }

	public static Key None()
	{
		return new Key
		{
			Kind = KeyKind.None,
			Length = 0,
			Value = Array.Empty<byte>()
		};
	}
	public static Key PartitionId(int value)
	{
		Span<byte> bytes = stackalloc byte[4];
		BinaryPrimitives.WriteInt32LittleEndian(bytes, value);

		return new Key
		{
			Kind = KeyKind.PartitionId,
			Length = 4,
			Value = bytes.ToArray(),
		};
	}
	public static Key EntityIdString(string value)
	{
		if (value.Length == 0 || value.Length > 255)
		{
			throw new ArgumentException("Value is too long", nameof(value));
		}
		return new Key
		{
			Kind = KeyKind.EntityId,
			Length = value.Length,
			Value = Encoding.UTF8.GetBytes(value)
		};
	}
	public static Key EntityIdBytes(byte[] value)
	{
		if (value.Length == 0 || value.Length > 255)
		{
			throw new ArgumentException("Value is too long", nameof(value));
		}
		return new Key
		{
			Kind = KeyKind.EntityId,
			Length = value.Length,
			Value = value
		};
	}
	public static Key EntityIdInt(int value)
	{
		Span<byte> bytes = stackalloc byte[4];
		BinaryPrimitives.WriteInt32LittleEndian(bytes, value);
		return new Key
		{
			Kind = KeyKind.EntityId,
			Length = 4,
			Value = bytes.ToArray()
		};
	}
	public static Key EntityIdUlong(ulong value)
	{
		Span<byte> bytes = stackalloc byte[8];
		BinaryPrimitives.WriteUInt64LittleEndian(bytes, value);
		return new Key
		{
			Kind = KeyKind.EntityId,
			Length = 8,
			Value = bytes.ToArray()
		};
	}
	public static Key EntityIdGuid(Guid value)
	{
		var bytes = value.ToByteArray();
		return new Key
		{
			Kind = KeyKind.EntityId,
			Length = 16,
			Value = bytes
		};
	}
}
	
