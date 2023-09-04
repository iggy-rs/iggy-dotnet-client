namespace Iggy_SDK.Contracts.Http;

//TODO - create a polled instance of this that is empty
public sealed class PolledMessages<T>
{
    public required uint PartitionId { get; init; }
    public required ulong CurrentOffset { get; init; }
    public required IReadOnlyList<MessageResponse<T>> Messages { get; init; }

    public static PolledMessages<T> Empty =>
        new()
        {
            Messages = new List<MessageResponse<T>>().AsReadOnly(),
            CurrentOffset = 0,
            PartitionId = 0,
        };
}