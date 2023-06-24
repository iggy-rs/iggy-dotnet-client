using System.Text.Json;

namespace Shared;

public sealed class Envelope
{
	public string MessageType { get; set; }
	public string Payload { get; set; }

	public Envelope New<T>(string messageType, T payload) where T : ISerializableMessage
	{
		return new Envelope
		{
			MessageType = messageType,
			Payload = JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true })
		};
	}
}