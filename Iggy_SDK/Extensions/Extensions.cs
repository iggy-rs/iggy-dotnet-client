using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace Iggy_SDK.Extensions;

internal static class Extensions
{
    internal static string ToSnakeCase(this string input)
    {
        Debug.Assert(!string.IsNullOrEmpty(input));
        if (CountUppercaseLetters(input) == 0)
            return input.ToLower();

        var len = input.Length + CountUppercaseLetters(input) - 1;
        return string.Create(len, input, (span, value) =>
        {
            value.AsSpan().CopyTo(span);
            span[0] = char.ToLower(span[0]);

            for (int i = 0; i < len; ++i)
            {
                if (char.IsUpper(span[i]))
                {
                    span[i] = char.ToLower(span[i]);
                    span[i..].ShiftSliceRight();
                    span[i] = '_';
                }
            }
        });
    }

    internal static UInt128 ToUInt128(this Guid g)
    {
        Span<byte> array = stackalloc byte[16];
        MemoryMarshal.TryWrite(array, ref g);
        var hi = BinaryPrimitives.ReadUInt64LittleEndian(array[..8]);
        var lo = BinaryPrimitives.ReadUInt64LittleEndian(array[8..16]);
        return new UInt128(hi, lo);
    }
    internal static UInt128 ToUInt128(this byte[] bytes)
    {
        var lo = BinaryPrimitives.ReadUInt64LittleEndian(bytes[..8]);
        var hi = BinaryPrimitives.ReadUInt64LittleEndian(bytes[8..16]);
        return new UInt128(hi, lo);
    }
    internal static Int128 ToInt128(this byte[] bytes)
    {
        var lo = BinaryPrimitives.ReadUInt64LittleEndian(bytes[..8]);
        var hi = BinaryPrimitives.ReadUInt64LittleEndian(bytes[8..16]);
        return new Int128(hi, lo);
    }
    internal static byte[] GetBytesFromGuid(this Guid value)
    {
        Span<byte> bytes = stackalloc byte[16];
        MemoryMarshal.TryWrite(bytes, ref value);
        return bytes.ToArray();
    }
    internal static byte[] GetBytesFromUInt128(this UInt128 value)
    {
        Span<byte> result = stackalloc byte[16];
        var span = MemoryMarshal.Cast<UInt128, byte>(MemoryMarshal.CreateReadOnlySpan(ref value, 1));
        return span.ToArray();
    }
    internal static byte[] GetBytesFromInt128(this Int128 value)
    {
        Span<byte> result = stackalloc byte[16];
        var span = MemoryMarshal.Cast<Int128, byte>(MemoryMarshal.CreateReadOnlySpan(ref value, 1));
        return span.ToArray();
    }

    internal static UInt128 GetUInt128(this JsonElement jsonElement)
    {
        return UInt128.Parse(jsonElement.ToString());
    }

    private static int CountUppercaseLetters(string input)
    {
        return input.Count(char.IsUpper);
    }
    private static void ShiftSliceRight(this Span<char> slice)
    {
        for (int i = slice.Length - 2; i >= 0; i--)
        {
            slice[i + 1] = slice[i];
        }
    }
    internal static void InvokeOrUseDefault<T>(this Action<T>? action, T arg, Action<T> defaultAction)
    {
        (action ?? defaultAction).Invoke(arg);
    }
}

internal static class DateTimeOffsetUtils
{
    internal static DateTimeOffset FromUnixTimeMicroSeconds(ulong microSeconds)
    {
        return DateTimeOffset.FromUnixTimeSeconds((long)(microSeconds / 1e+6));
    }
}