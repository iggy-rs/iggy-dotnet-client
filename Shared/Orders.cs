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
	public int Id { get; init; }
	public string CurrencyPair { get; init; }
	public double Price { get; init; }
	public double Quantity { get; init; }
	public string Side { get; init; }
	public ulong Timestamp { get; init; }

	private string ToJsonPrint()
	{
		return JsonSerializer.Serialize(this, _jsonSerializerOptions); 
	}
	
	public string ToJson()
	{
		var envelope = new Envelope();
		var env = envelope.New<OrderCreated>("order_created", this);
		return JsonSerializer.Serialize(env, _jsonSerializerOptions);
	}

	public byte[] ToBytes()
	{
		var envelope = new Envelope();
		var env = envelope.New<OrderCreated>("order_created", this);
		var json = JsonSerializer.Serialize(env, _jsonSerializerOptions);
		return Encoding.UTF8.GetBytes(json);
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
	public int Id { get; init; }
	public double Price { get; init; }
	public ulong Timestamp { get; init; }
	public string ToJson()
	{
		var envelope = new Envelope();
		var env = envelope.New<OrderConfirmed>("order_confirmed", this);
		return JsonSerializer.Serialize(env, _jsonSerializerOptions);
	}

	public byte[] ToBytes()
	{
		var envelope = new Envelope();
		var env = envelope.New<OrderConfirmed>("order_confirmed", this);
		var json = JsonSerializer.Serialize(env, _jsonSerializerOptions);
		return Encoding.UTF8.GetBytes(json);
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
	public int Id { get; init; }
	public ulong Timestamp	{ get; init; }
	public string Reason { get; init; }
	public string ToJson()
	{
		var envelope = new Envelope();
		var env = envelope.New<OrderRejected>("order_rejected", this);
		return JsonSerializer.Serialize(env, _jsonSerializerOptions);
	}

	public byte[] ToBytes()
	{
		var envelope = new Envelope();
		var env = envelope.New<OrderRejected>("order_rejected", this);
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

