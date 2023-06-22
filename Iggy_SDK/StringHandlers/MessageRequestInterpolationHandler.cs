using System.Runtime.CompilerServices;
using Iggy_SDK.Enums;

namespace Iggy_SDK.StringHandlers;

[InterpolatedStringHandler]
public ref struct MessageRequestInterpolationHandler
{
	private DefaultInterpolatedStringHandler _innerHandler;
	public MessageRequestInterpolationHandler(int literalLength, int formattedCount)
	{
		_innerHandler = new DefaultInterpolatedStringHandler(literalLength, formattedCount);
	}

	public void AppendLiteral(string message)
	{
		_innerHandler.AppendLiteral(message);	
	}
	public void AppendFormatted<T>(T t)
	{
		switch (t)
		{
			case MessagePolling pollingStrat:
			{
				var str = pollingStrat switch
				{
					MessagePolling.Offset => "offset",
					MessagePolling.Timestamp => "timestamp",
					MessagePolling.First => "first",
					MessagePolling.Last => "last",
					MessagePolling.Next => "next",
					_ => "offset"
				};
				_innerHandler.AppendFormatted(str);
				break;
			}
			case bool tBool:
				_innerHandler.AppendFormatted(tBool.ToString().ToLower());
				break;
			default:
				_innerHandler.AppendFormatted(t);
				break;
		}
	}

	public override string ToString()
	{
		return _innerHandler.ToString();
	}

	public string ToStringAndClear()
	{
		return _innerHandler.ToStringAndClear();	
	}
}