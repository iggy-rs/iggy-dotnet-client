namespace ConsoleApp;
public class OrderCreated
{
	public ulong Id { get; set; }
	public string CurrencyPair { get; set; }
	public double Price { get; set; }
	public double Quantity { get; set; }
	public string Side { get; set; }
	public ulong Timestamp { get; set; }
}

public class OrderConfirmed
{
	public ulong Id { get; set; }
	public double Price { get; set; }
	public ulong Timestamp { get; set; }
}

public class OrderRejected
{
	public ulong Id { get; set; }
	public ulong Timestamp	{ get; set; }
	public string Reason { get; set; }
}

