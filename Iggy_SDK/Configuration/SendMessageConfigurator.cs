namespace Iggy_SDK.Configuration;

public sealed class SendMessageConfigurator
{
    public TimeSpan PollingInterval { get; set; }
    public int MaxMessagesPerBatch { get; set; }
}