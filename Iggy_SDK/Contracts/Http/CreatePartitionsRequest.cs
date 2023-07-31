namespace Iggy_SDK.Contracts.Http;

public sealed class CreatePartitionsRequest
{
	public required int PartitionsCount { get; init; }
}