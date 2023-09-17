namespace Iggy_SDK.Configuration;

public sealed class IntervalBatchingSettings
{
    public bool Enabled { get; set; } = true;
    public TimeSpan Interval { get; set; } = TimeSpan.FromMilliseconds(150);
    public int MaxMessagesPerBatch { get; set; } = 1000;
    /// <summary>
    /// Defines the maximum number of requests in interval 
    /// Used mainly to avoid flooding the channel (default value 4096).
    /// </summary>
    public int MaxRequests { get; set; } = 4096;
}