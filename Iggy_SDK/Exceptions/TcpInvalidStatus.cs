namespace Iggy_SDK.Exceptions;

public sealed class TcpInvalidStatus : Exception
{
	public TcpInvalidStatus() : base("Received an Invalid Response Status")
	{
		
	}	
}