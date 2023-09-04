using Iggy_SDK.Enums;

namespace Iggy_SDK.Kinds;

public readonly struct Consumer
{
    public required ConsumerType Type { get; init; }
    public required int Id { get; init; }

    public static Consumer New(int id)
    {
        return new Consumer
        {
            Id = id,
            Type = ConsumerType.Consumer
        };
    }

    public static Consumer Group(int id)
    {
        return new Consumer
        {
            Id = id,
            Type = ConsumerType.ConsumerGroup
        };
    }
}