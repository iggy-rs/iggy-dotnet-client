namespace Iggy_SDK.Contracts;

public sealed class GroupResponse
{
	public required int Id { get; init; }	
	public required int MembersCount { get; init; }	
	public required int PartitionsCount { get; init; }	
}