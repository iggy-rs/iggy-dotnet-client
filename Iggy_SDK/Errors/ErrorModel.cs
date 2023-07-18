namespace Iggy_SDK.Utils;

public sealed class ErrorModel
{
	public required int Id { get; init; }
	public required string Code { get; init; }
	public required string Reason { get; init; }
	
}