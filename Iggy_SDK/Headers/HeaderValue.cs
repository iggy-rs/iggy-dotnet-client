using System.Buffers.Binary;
using System.Globalization;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using Iggy_SDK.Extensions;

namespace Iggy_SDK.Headers;

public sealed class HeaderValue
{
	public required HeaderKind Kind { get; init; }
	public required byte[] Value { get; init; }

	public static HeaderValue Raw(byte[] value)
	{
		return new HeaderValue
		{
			Kind = HeaderKind.Raw,
			Value = value
		};
	}
	
	public static HeaderValue String(string value)
	{
		if (value.Length is 0 or > 255)
		{
			throw new ArgumentException("Value has incorrect size, must be between 1 and 255", nameof(value));
		}
		return new HeaderValue
		{
			Kind = HeaderKind.String,
			Value = Encoding.UTF8.GetBytes(value)
		};
	}
	
	public static HeaderValue Bool(bool value)
	{
		return new HeaderValue
		{
			Kind = HeaderKind.Bool,
			Value = BitConverter.GetBytes(value)
		};
	}
	
	public static HeaderValue Int32(int value)
	{
		var bytes = new byte[4];
		BinaryPrimitives.WriteInt32LittleEndian(bytes, value);
		return new HeaderValue
		{
			Kind = HeaderKind.Int32,
			Value = bytes
		};
	}
	
	public static HeaderValue Int64(long value)
	{
		var bytes = new byte[8];
		BinaryPrimitives.WriteInt64LittleEndian(bytes, value);
		return new HeaderValue
		{
			Kind = HeaderKind.Int64,
			Value = bytes
		};
	}

	public static HeaderValue Int128(Int128 value)
	{
		return new HeaderValue
		{
			Kind = HeaderKind.Int128,
			Value = value.GetBytesFromInt128()
		};
	}

	public static HeaderValue Guid(Guid value)
	{
		return new HeaderValue
		{
			Kind = HeaderKind.Uint128,
			Value = value.GetBytesFromGuid()
		};
	}
	
	public static HeaderValue UInt32(uint value)
	{
		var bytes = new byte[4];
		BinaryPrimitives.WriteUInt32LittleEndian(bytes, value);
		return new HeaderValue
		{
			Kind = HeaderKind.Uint32,
			Value = bytes
		};
	}
	
	public static HeaderValue UInt64(ulong value)
	{
		var bytes = new byte[8];
		BinaryPrimitives.WriteUInt64LittleEndian(bytes, value);
		return new HeaderValue
		{
			Kind = HeaderKind.Uint64,
			Value = bytes
		};
	}
	
	public static HeaderValue UInt128(UInt128 value)
	{
		return new HeaderValue
		{
			Kind = HeaderKind.Uint128,
			Value = value.GetBytesFromUInt128()
		};
	}

	public static HeaderValue Float32(float value)
	{
		var bytes = new byte[4];
		BinaryPrimitives.TryWriteSingleLittleEndian(bytes, value);
		return new HeaderValue
		{
			Kind = HeaderKind.Float32,
			Value = bytes
		};
	}
	
	public static HeaderValue Float64(double value)
	{
		var bytes = new byte[8];
		BinaryPrimitives.TryWriteDoubleLittleEndian(bytes, value);
		return new HeaderValue
		{
			Kind = HeaderKind.Float64,
			Value = bytes
		};
	}
	
	public override string? ToString()
	{
		var sb = new StringBuilder();
		foreach (var t in Value)
		{
			sb.Append(t);
		}
		var byteArrString = sb.ToString();
		
		return Kind switch
		{
			HeaderKind.Raw => byteArrString,
			HeaderKind.String => Encoding.UTF8.GetString(Value),
			HeaderKind.Bool => BitConverter.ToBoolean(Value, 0).ToString(),
			HeaderKind.Int32 => BitConverter.ToInt32(Value, 0).ToString(),
			HeaderKind.Int64 => BitConverter.ToInt64(Value, 0).ToString(),
			HeaderKind.Int128 => Value.ToInt128().ToString(),
			HeaderKind.Uint32 => BitConverter.ToUInt32(Value, 0).ToString(),
			HeaderKind.Uint64 => BitConverter.ToUInt64(Value, 0).ToString(),
			HeaderKind.Uint128 => new Guid(Value).ToString(),
			HeaderKind.Float32 => BitConverter.ToSingle(Value, 0).ToString(CultureInfo.InvariantCulture),
			HeaderKind.Float64 => BitConverter.ToDouble(Value, 0).ToString(CultureInfo.InvariantCulture),
			_ => throw new ArgumentOutOfRangeException()
		};
	}
}

