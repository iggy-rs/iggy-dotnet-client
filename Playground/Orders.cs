namespace ConsoleApp;
public class OrderCreated
{
	public ulong Id { get; init; }
	public string CurrencyPair { get; init; }
	public double Price { get; init; }
	public double Quantity { get; init; }
	public string Side { get; init; }
	public ulong Timestamp { get; init; }
}

public class OrderConfirmed
{
	public ulong Id { get; init; }
	public double Price { get; init; }
	public ulong Timestamp { get; init; }
}

public class OrderRejected
{
	public ulong Id { get; init; }
	public ulong Timestamp	{ get; init; }
	public string Reason { get; init; }
}

