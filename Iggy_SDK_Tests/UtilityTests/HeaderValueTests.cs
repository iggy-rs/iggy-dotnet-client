using Iggy_SDK.Extensions;
using Iggy_SDK.Headers;
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
        HeaderValue header = HeaderValue.FromFloat32(value);

        Assert.Equal(HeaderKind.Float32, header.Kind);
        Assert.Equal(value, BitConverter.ToSingle(header.Value));
    }

    [Fact]
    public void Float64_ReturnsCorrectValue()
    {
        double value = 2.71828;
        HeaderValue header = HeaderValue.FromFloat64(value);

        Assert.Equal(HeaderKind.Float64, header.Kind);
        Assert.Equal(value, BitConverter.ToDouble(header.Value));
    }

}