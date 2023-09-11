namespace Iggy_SDK.Configuration;

public sealed class SendMessageConfigurator
{
    public TimeSpan PollingInterval { get; set; }
    public int MaxMessagesPerBatch { get; set; }
    /// <summary>
    /// Defines the maximum number of requests allowed to be sent through the channel during the polling process.
    /// Use mainly to avoid flooding the channel (default value 4096).
    /// </summary>
    public int MaxRequestsInPoll { get; set; }
}