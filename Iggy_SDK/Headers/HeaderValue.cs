using Iggy_SDK.Extensions;
using System.Buffers.Binary;
using System.Globalization;
using System.Text;

namespace Iggy_SDK.Headers;

public readonly struct HeaderValue
{
    public required HeaderKind Kind { get; init; }
    public required byte[] Value { get; init; }

    public static HeaderValue FromBytes(byte[] value)
    {
        return new HeaderValue
        {
            Kind = HeaderKind.Raw,
            Value = value
        };
    }

    public static HeaderValue FromString(string value)
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

    public static HeaderValue FromBool(bool value)
    {
        return new HeaderValue
        {
            Kind = HeaderKind.Bool,
            Value = BitConverter.GetBytes(value)
        };
    }

    public static HeaderValue FromInt32(int value)
    {
        var bytes = new byte[4];
        BinaryPrimitives.WriteInt32LittleEndian(bytes, value);
        return new HeaderValue
        {
            Kind = HeaderKind.Int32,
            Value = bytes
        };
    }

    public static HeaderValue FromInt64(long value)
    {
        var bytes = new byte[8];
        BinaryPrimitives.WriteInt64LittleEndian(bytes, value);
        return new HeaderValue
        {
            Kind = HeaderKind.Int64,
            Value = bytes
        };
    }

    public static HeaderValue FromInt128(Int128 value)
    {
        return new HeaderValue
        {
            Kind = HeaderKind.Int128,
            Value = value.GetBytesFromInt128()
        };
    }

    public static HeaderValue FromGuid(Guid value)
    {
        return new HeaderValue
        {
            Kind = HeaderKind.Uint128,
            Value = value.GetBytesFromGuid()
        };
    }

    public static HeaderValue FromUInt32(uint value)
    {
        var bytes = new byte[4];
        BinaryPrimitives.WriteUInt32LittleEndian(bytes, value);
        return new HeaderValue
        {
            Kind = HeaderKind.Uint32,
            Value = bytes
        };
    }

    public static HeaderValue FromUInt64(ulong value)
    {
        var bytes = new byte[8];
        BinaryPrimitives.WriteUInt64LittleEndian(bytes, value);
        return new HeaderValue
        {
            Kind = HeaderKind.Uint64,
            Value = bytes
        };
    }

    public static HeaderValue FromUInt128(UInt128 value)
    {
        return new HeaderValue
        {
            Kind = HeaderKind.Uint128,
            Value = value.GetBytesFromUInt128()
        };
    }

    public static HeaderValue FromFloat32(float value)
    {
        var bytes = new byte[4];
        BinaryPrimitives.TryWriteSingleLittleEndian(bytes, value);
        return new HeaderValue
        {
            Kind = HeaderKind.Float32,
            Value = bytes
        };
    }

    public static HeaderValue FromFloat64(double value)
    {
        var bytes = new byte[8];
        BinaryPrimitives.TryWriteDoubleLittleEndian(bytes, value);
        return new HeaderValue
        {
            Kind = HeaderKind.Float64,
            Value = bytes
        };
    }
    //since the order of headers is known during sending/polling, I can create several methods
    //that will allow to translate raw byte array into specific types, 
    //do an if check on the header kind to make sure that the type is correct
    //if it's not throw exception

    public byte[] ToBytes()
    {
        if (Kind is not HeaderKind.Raw)
        {
            throw new InvalidOperationException("Can't convert header");
        }
        return Value;
    }
    public new string ToString()
    {
        if (Kind is not HeaderKind.String)
        {
            throw new InvalidOperationException("Can't convert header");
        }
        return Encoding.UTF8.GetString(Value); 
    }
    public bool ToBool()
    {
        if (Kind is not HeaderKind.Bool)
        {
            throw new InvalidOperationException("Can't convert header");
        }
        return BitConverter.ToBoolean(Value, 0);
    }
    public int ToInt32()
    {
        if (Kind is not HeaderKind.Int32)
        {
            throw new InvalidOperationException("Can't convert header");
        }
        return BitConverter.ToInt32(Value, 0);
    }
    public long ToInt64()
    {
        if (Kind is not HeaderKind.Int64)
        {
            throw new InvalidOperationException("Can't convert header");
        }
        return BitConverter.ToInt64(Value, 0);
    }

    /*
    public override string ToString()
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
    */
}