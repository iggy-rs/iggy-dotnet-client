namespace Iggy_SDK.Exceptions;

public sealed class InvalidBaseAdressException : Exception
{

	public InvalidBaseAdressException() : base("Invalid Base Adress, use ':' only to describe the port")
	{
		
	}
}