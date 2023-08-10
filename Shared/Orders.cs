using System.Text;
using System.Text.Json;

namespace Shared;
public class OrderCreated : ISerializableMessage
{
	private readonly JsonSerializerOptions _jsonSerializerOptions;

	public OrderCreated()
	{
		_jsonSerializerOptions = new();
		_jsonSerializerOptions.WriteIndented = true;
	}
	public required int Id { get; init; }
	public required string CurrencyPair { get; init; }
	public required double Price { get; init; }
	public required double Quantity { get; init; }
	public required string Side { get; init; }
	public required ulong Timestamp { get; init; }

	private string ToJsonPrint()
	{
		return JsonSerializer.Serialize(this, _jsonSerializerOptions); 
	}
	
	public string ToJson()
	{
		var envelope = new Envelope();
		var env = envelope.New("order_created", this);
		return JsonSerializer.Serialize(env, _jsonSerializerOptions);
	}

	public byte[] ToBytes()
	{
		var envelope = new Envelope();
		var env = envelope.New("order_created", this);
		var json = JsonSerializer.Serialize(env, _jsonSerializerOptions);
		return Encoding.UTF8.GetBytes(json);
	}
	public Envelope ToEnvelope()
	{
		var envelope = new Envelope();
		var env = envelope.New("order_created", this);
		return env;
	}

	public override string ToString()
	{
		return $"OrderCreated {ToJsonPrint()}";
	}
	
}

public class OrderConfirmed : ISerializableMessage
{
	private readonly JsonSerializerOptions _jsonSerializerOptions;

	public OrderConfirmed()
	{
		_jsonSerializerOptions = new();
		_jsonSerializerOptions.WriteIndented = true;
	}
	public required int Id { get; init; }
	public required double Price { get; init; }
	public required ulong Timestamp { get; init; }
	public string ToJson()
	{
		var envelope = new Envelope();
		var env = envelope.New("order_confirmed", this);
		return JsonSerializer.Serialize(env, _jsonSerializerOptions);
	}

	public byte[] ToBytes()
	{
		var envelope = new Envelope();
		var env = envelope.New("order_confirmed", this);
		var json = JsonSerializer.Serialize(env, _jsonSerializerOptions);
		return Encoding.UTF8.GetBytes(json);
	}

	public Envelope ToEnvelope()
	{
		var envelope = new Envelope();
		var env = envelope.New("order_confirmed", this);
		return env;
	}

	private string ToJsonPrint()
	{
		return JsonSerializer.Serialize(this, _jsonSerializerOptions);
	}
	public override string ToString()
	{
		return $"OrderConfirmed {ToJsonPrint()}";
	}
}

public class OrderRejected : ISerializableMessage
{
	private readonly JsonSerializerOptions _jsonSerializerOptions;

	public OrderRejected()
	{
		_jsonSerializerOptions = new();
		_jsonSerializerOptions.WriteIndented = true;
	}
	public required int Id { get; init; }
	public required ulong Timestamp	{ get; init; }
	public required string Reason { get; init; }
	public string ToJson()
	{
		var envelope = new Envelope();
		var env = envelope.New("order_rejected", this);
		return JsonSerializer.Serialize(env, _jsonSerializerOptions);
	}
	public Envelope ToEnvelope()
	{
		var envelope = new Envelope();
		var env = envelope.New("order_rejected", this);
		return env;
	}

	public byte[] ToBytes()
	{
		var envelope = new Envelope();
		var env = envelope.New("order_rejected", this);
		var json = JsonSerializer.Serialize(env, _jsonSerializerOptions);
		return Encoding.UTF8.GetBytes(json);
	}

	private string ToJsonPrint()
	{
		return JsonSerializer.Serialize(this, _jsonSerializerOptions);
	}
	
	public override string ToString()
	{
		return $"OrderRejected {ToJsonPrint()}";
	}
}

