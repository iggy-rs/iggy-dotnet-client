
namespace Iggy_SDK.Utils;

public static class EmptyList<T>
{
    private static class Empty<T>
    {
        internal static readonly IReadOnlyList<T> Value = new List<T>().AsReadOnly();
    }
    public static IReadOnlyList<T> Instance => Empty<T>.Value;
}

