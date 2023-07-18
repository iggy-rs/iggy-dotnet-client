namespace Iggy_SDK.Contracts.Http;

public sealed class GroupResponse
{
	public required int Id { get; init; }	
	public required int MembersCount { get; init; }	
	public required int PartitionsCount { get; init; }	
}