using System.Text.Json.Serialization;
using Iggy_SDK.JsonConfiguration;

namespace Iggy_SDK.Contracts.Http;

public sealed class MessageResponse
{
	public required ulong Offset { get; init; }
	public required ulong Timestamp { get; init; }
	public Guid Id { get; init; }
	public required byte[] Payload { get; init; }
}