using Iggy_SDK.MessageStream;

namespace Iggy_SDK.HostedService;

public sealed class MessageSenderDispatcher
{
	private readonly PeriodicTimer _timer;
	private Task? _timerTask;
	private readonly CancellationTokenSource _cts = new();
	private readonly IMessageClient _messager;

	public MessageSenderDispatcher(TimeSpan interval, IMessageClient messager)
	{
		_timer = new PeriodicTimer(interval);
		_messager = messager;
	}

	public void Start()
	{
		_timerTask = DoWorkAsync();
	}

	private async Task DoWorkAsync()
	{
		try
		{
			while (await _timer.WaitForNextTickAsync(_cts.Token))
			{
				//Do the work there
			}

		}
		catch 
		{
			
		}
		
	}

	public async Task StopAsync()
	{
		if (_timerTask is null)
		{
			return;
		}
		_cts.Cancel();
		await _timerTask;
		_cts.Dispose();
	}

}
