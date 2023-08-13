using System.Buffers.Binary;
using System.Text;
using Iggy_SDK.Enums;

namespace Iggy_SDK;

public sealed class Identifier
{
	
	public required IdKind Kind { get; init; }
	public required int Length { get; init; }
	public required byte[] Value { get; init; }

	public static Identifier Numeric(int value)
	{
		Span<byte> bytes = stackalloc byte[4];
		BinaryPrimitives.WriteInt32LittleEndian(bytes, value);

		return new Identifier
		{
			Kind = IdKind.Numeric,
			Length = 4,
			Value = bytes.ToArray()
		};
	}

	public static Identifier String(string value)
	{
		if (value.Length is 0 or > 255)
		{
			throw new ArgumentException("Value has incorrect size, must be between 1 and 255", nameof(value));
		}
		return new Identifier
		{
			Kind = IdKind.String,
			Length = value.Length,
			Value = Encoding.UTF8.GetBytes(value)
		};
	}

	public override string ToString()
	{
		return Kind switch
		{
			IdKind.Numeric => BitConverter.ToInt32(Value).ToString(),
			IdKind.String => Encoding.UTF8.GetString(Value),
			_ => throw new ArgumentOutOfRangeException()
		};
	}
}

