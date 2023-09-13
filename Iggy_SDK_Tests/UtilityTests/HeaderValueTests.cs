using Iggy_SDK.Extensions;
using Iggy_SDK.Headers;
using System.Buffers.Binary;
using System.Text;

namespace Iggy_SDK_Tests.UtilityTests;

public sealed class HeaderValueTests
{

    [Fact]
    public void Raw_ReturnsCorrectValue()
    {
        byte[] data = new byte[] { 1, 2, 3 };
        HeaderValue header = HeaderValue.FromBytes(data);

        Assert.Equal(HeaderKind.Raw, header.Kind);
        Assert.Equal(data, header.Value);
    }

    [Fact]
    public void String_ThrowsArgumentExceptionForInvalidValue()
    {
        Assert.Throws<ArgumentException>(() => HeaderValue.FromString(""));
        Assert.Throws<ArgumentException>(() => HeaderValue.FromString(new string('a', 256)));
    }

    [Fact]
    public void String_ReturnsCorrectValue()
    {
        string value = "TestString";
        HeaderValue header = HeaderValue.FromString(value);

        Assert.Equal(HeaderKind.String, header.Kind);
        Assert.Equal(value, Encoding.UTF8.GetString(header.Value));
    }

    [Fact]
    public void Bool_ReturnsCorrectValue()
    {
        bool value = true;
        HeaderValue header = HeaderValue.FromBool(value);

        Assert.Equal(HeaderKind.Bool, header.Kind);
        Assert.Equal(BitConverter.GetBytes(value), header.Value);
    }

    [Fact]
    public void Int32_ReturnsCorrectValue()
    {
        int value = 42;
        HeaderValue header = HeaderValue.FromInt32(value);

        Assert.Equal(HeaderKind.Int32, header.Kind);
        Assert.Equal(value, BitConverter.ToInt32(header.Value));
    }

    [Fact]
    public void Int64_ReturnsCorrectValue()
    {
        long value = 1234567890L;
        HeaderValue header = HeaderValue.FromInt64(value);

        Assert.Equal(HeaderKind.Int64, header.Kind);
        Assert.Equal(value, BitConverter.ToInt64(header.Value));
    }

    [Fact]
    public void Int128_ReturnsCorrectValue()
    {
        // You should provide a valid Int128 value here for testing.
        Int128 value = new Int128(123, 456);
        HeaderValue header = HeaderValue.FromInt128(value);

        Assert.Equal(HeaderKind.Int128, header.Kind);
        Assert.Equal(value.GetBytesFromInt128(), header.Value);
    }

    [Fact]
    public void Guid_ReturnsCorrectValue()
    {
        Guid value = Guid.NewGuid();
        HeaderValue header = HeaderValue.FromGuid(value);

        Assert.Equal(HeaderKind.Uint128, header.Kind);
        byte[] guidBytes = value.ToByteArray();
        byte[] headerValueBytes = header.Value;

        Assert.Equal(guidBytes.Length, headerValueBytes.Length);
        for (int i = 0; i < guidBytes.Length; i++)
        {
            Assert.Equal(guidBytes[i], headerValueBytes[i]);
        }
    }

    [Fact]
    public void UInt32_ReturnsCorrectValue()
    {
        uint value = 12345U;
        HeaderValue header = HeaderValue.FromUInt32(value);

        Assert.Equal(HeaderKind.Uint32, header.Kind);
        Assert.Equal(value, BitConverter.ToUInt32(header.Value));
    }

    [Fact]
    public void UInt64_ReturnsCorrectValue()
    {
        ulong value = 9876543210UL;
        HeaderValue header = HeaderValue.FromUInt64(value);

        Assert.Equal(HeaderKind.Uint64, header.Kind);
        Assert.Equal(value, BitConverter.ToUInt64(header.Value));
    }

    [Fact]
    public void UInt128_ReturnsCorrectValue()
    {
        UInt128 value = new UInt128(789, 101112);
        HeaderValue header = HeaderValue.FromUInt128(value);

        Assert.Equal(HeaderKind.Uint128, header.Kind);
        Assert.Equal(value.GetBytesFromUInt128(), header.Value);
    }

    [Fact]
    public void Float32_ReturnsCorrectValue()
    {
        float value = 3.14f;
        HeaderValue header = HeaderValue.FromFloat(value);

        Assert.Equal(HeaderKind.Float, header.Kind);
        Assert.Equal(value, BitConverter.ToSingle(header.Value));
    }

    [Fact]
    public void Float64_ReturnsCorrectValue()
    {
        double value = 2.71828;
        HeaderValue header = HeaderValue.FromDouble(value);

        Assert.Equal(HeaderKind.Double, header.Kind);
        Assert.Equal(value, BitConverter.ToDouble(header.Value));
    }
    [Fact]
    public void ToBytes_ValidKind_ReturnsValue()
    {
        // Arrange
        var headerValue = new HeaderValue
        {
            Kind = HeaderKind.Raw,
            Value = Encoding.UTF8.GetBytes("TestValue")
        };

        // Act
        var result = headerValue.ToBytes();

        // Assert
        Assert.Equal(Encoding.UTF8.GetBytes("TestValue"), result);
    }

    [Fact]
    public void ToBytes_InvalidKind_ThrowsInvalidOperationException()
    {
        // Arrange
        var headerValue = new HeaderValue
        {
            Kind = HeaderKind.String,
            Value = Encoding.UTF8.GetBytes("TestValue")
        };

        // Act and Assert
        Assert.Throws<InvalidOperationException>(() => headerValue.ToBytes());
    }

    [Fact]
    public void ToString_ValidKind_ReturnsDecodedString()
    {
        // Arrange
        var headerValue = new HeaderValue
        {
            Kind = HeaderKind.String,
            Value = Encoding.UTF8.GetBytes("TestValue")
        };

        // Act
        var result = headerValue.ToString();

        // Assert
        Assert.Equal("TestValue", result);
    }

    [Fact]
    public void ToString_InvalidKind_ThrowsInvalidOperationException()
    {
        // Arrange
        var headerValue = new HeaderValue
        {
            Kind = HeaderKind.Raw,
            Value = Encoding.UTF8.GetBytes("TestValue")
        };

        // Act and Assert
        Assert.Throws<InvalidOperationException>(() => headerValue.ToString());
    }

    [Fact]
    public void ToBool_ValidKind_ReturnsValue()
    {
        // Arrange
        var headerValue = new HeaderValue
        {
            Kind = HeaderKind.Bool,
            Value = BitConverter.GetBytes(true)
        };

        // Act
        var result = headerValue.ToBool();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ToBool_InvalidKind_ThrowsInvalidOperationException()
    {
        // Arrange
        var headerValue = new HeaderValue
        {
            Kind = HeaderKind.String,
            Value = Encoding.UTF8.GetBytes("TestValue")
        };

        // Act and Assert
        Assert.Throws<InvalidOperationException>(() => headerValue.ToBool());
    }
    [Fact]
    public void ToInt32_ValidKind_ReturnsValue()
    {
        // Arrange
        var intValue = 42;
        var bytes = new byte[4];
        BinaryPrimitives.WriteInt32LittleEndian(bytes, intValue);

        var headerValue = new HeaderValue
        {
            Kind = HeaderKind.Int32,
            Value = bytes
        };

        // Act
        var result = headerValue.ToInt32();

        // Assert
        Assert.Equal(intValue, result);
    }

    [Fact]
    public void ToInt32_InvalidKind_ThrowsInvalidOperationException()
    {
        // Arrange
        var headerValue = new HeaderValue
        {
            Kind = HeaderKind.String,
            Value = Encoding.UTF8.GetBytes("TestValue")
        };

        // Act and Assert
        Assert.Throws<InvalidOperationException>(() => headerValue.ToInt32());
    }

    [Fact]
    public void ToInt64_ValidKind_ReturnsValue()
    {
        // Arrange
        var longValue = 1234567890L;
        var bytes = new byte[8];
        BinaryPrimitives.WriteInt64LittleEndian(bytes, longValue);

        var headerValue = new HeaderValue
        {
            Kind = HeaderKind.Int64,
            Value = bytes
        };

        // Act
        var result = headerValue.ToInt64();

        // Assert
        Assert.Equal(longValue, result);
    }

    [Fact]
    public void ToInt64_InvalidKind_ThrowsInvalidOperationException()
    {
        // Arrange
        var headerValue = new HeaderValue
        {
            Kind = HeaderKind.Float,
            Value = BitConverter.GetBytes(3.14f)
        };

        // Act and Assert
        Assert.Throws<InvalidOperationException>(() => headerValue.ToInt64());
    }
     [Fact]
    public void ToUInt32_ValidKind_ReturnsValue()
    {
        // Arrange
        uint uintValue = 12345;
        var bytes = new byte[4];
        BinaryPrimitives.WriteUInt32LittleEndian(bytes, uintValue);

        var headerValue = new HeaderValue
        {
            Kind = HeaderKind.Uint32,
            Value = bytes
        };

        // Act
        var result = headerValue.ToUInt32();

        // Assert
        Assert.Equal(uintValue, result);
    }

    [Fact]
    public void ToUInt32_InvalidKind_ThrowsInvalidOperationException()
    {
        // Arrange
        var headerValue = new HeaderValue
        {
            Kind = HeaderKind.Double,
            Value = BitConverter.GetBytes(3.14)
        };

        // Act and Assert
        Assert.Throws<InvalidOperationException>(() => headerValue.ToUInt32());
    }

    [Fact]
    public void ToUInt64_ValidKind_ReturnsValue()
    {
        // Arrange
        ulong ulongValue = 9876543210UL;
        var bytes = new byte[8];
        BinaryPrimitives.WriteUInt64LittleEndian(bytes, ulongValue);

        var headerValue = new HeaderValue
        {
            Kind = HeaderKind.Uint64,
            Value = bytes
        };

        // Act
        var result = headerValue.ToUInt64();

        // Assert
        Assert.Equal(ulongValue, result);
    }

    [Fact]
    public void ToUInt64_InvalidKind_ThrowsInvalidOperationException()
    {
        // Arrange
        var headerValue = new HeaderValue
        {
            Kind = HeaderKind.Int128,
            Value = new byte[16]
        };

        // Act and Assert
        Assert.Throws<InvalidOperationException>(() => headerValue.ToUInt64());
    }

    [Fact]
    public void ToFloat_ValidKind_ReturnsValue()
    {
        // Arrange
        float floatValue = 3.14f;
        var bytes = new byte[4];
        BinaryPrimitives.TryWriteSingleLittleEndian(bytes, floatValue);

        var headerValue = new HeaderValue
        {
            Kind = HeaderKind.Float,
            Value = bytes
        };

        // Act
        var result = headerValue.ToFloat();

        // Assert
        Assert.Equal(floatValue, result);
    }

    [Fact]
    public void ToFloat_InvalidKind_ThrowsInvalidOperationException()
    {
        // Arrange
        var headerValue = new HeaderValue
        {
            Kind = HeaderKind.String,
            Value = Encoding.UTF8.GetBytes("TestValue")
        };

        // Act and Assert
        Assert.Throws<InvalidOperationException>(() => headerValue.ToFloat());
    }

    [Fact]
    public void ToDouble_ValidKind_ReturnsValue()
    {
        // Arrange
        double doubleValue = 3.14159265359;
        var bytes = new byte[8];
        BinaryPrimitives.TryWriteDoubleLittleEndian(bytes, doubleValue);

        var headerValue = new HeaderValue
        {
            Kind = HeaderKind.Double,
            Value = bytes
        };

        // Act
        var result = headerValue.ToDouble();

        // Assert
        Assert.Equal(doubleValue, result);
    }

    [Fact]
    public void ToDouble_InvalidKind_ThrowsInvalidOperationException()
    {
        // Arrange
        var headerValue = new HeaderValue
        {
            Kind = HeaderKind.Bool,
            Value = BitConverter.GetBytes(true)
        };

        // Act and Assert
        Assert.Throws<InvalidOperationException>(() => headerValue.ToDouble());
    }

    [Fact]
    public void ToInt128_ValidKind_ReturnsValue()
    {
        // Arrange
        var int128Value = new Int128(123, 456);
        var headerValue = new HeaderValue
        {
            Kind = HeaderKind.Int128,
            Value = int128Value.GetBytesFromInt128()
        };

        // Act
        var result = headerValue.ToInt128();

        // Assert
        Assert.Equal(int128Value, result);
    }

    [Fact]
    public void ToInt128_InvalidKind_ThrowsInvalidOperationException()
    {
        // Arrange
        var headerValue = new HeaderValue
        {
            Kind = HeaderKind.Uint64,
            Value = BitConverter.GetBytes(9876543210UL)
        };

        // Act and Assert
        Assert.Throws<InvalidOperationException>(() => headerValue.ToInt128());
    }

    [Fact]
    public void ToUInt128_ValidKind_ReturnsValue()
    {
        // Arrange
        var uint128Value = new UInt128(123, 456);
        var headerValue = new HeaderValue
        {
            Kind = HeaderKind.Uint128,
            Value = uint128Value.GetBytesFromUInt128()
        };

        // Act
        var result = headerValue.ToUInt128();

        // Assert
        Assert.Equal(uint128Value, result);
    }

    [Fact]
    public void ToUInt128_InvalidKind_ThrowsInvalidOperationException()
    {
        // Arrange
        var headerValue = new HeaderValue
        {
            Kind = HeaderKind.String,
            Value = Encoding.UTF8.GetBytes("TestValue")
        };

        // Act and Assert
        Assert.Throws<InvalidOperationException>(() => headerValue.ToUInt128());
    }

    [Fact]
    public void ToGuid_ValidKind_ReturnsValue()
    {
        // Arrange
        var guidValue = Guid.NewGuid();
        var headerValue = new HeaderValue
        {
            Kind = HeaderKind.Uint128,
            Value = guidValue.ToByteArray()
        };

        // Act
        var result = headerValue.ToGuid();

        // Assert
        Assert.Equal(guidValue, result);
    }

    [Fact]
    public void ToGuid_InvalidKind_ThrowsInvalidOperationException()
    {
        // Arrange
        var headerValue = new HeaderValue
        {
            Kind = HeaderKind.Bool,
            Value = BitConverter.GetBytes(true)
        };

        // Act and Assert
        Assert.Throws<InvalidOperationException>(() => headerValue.ToGuid());
    }
}