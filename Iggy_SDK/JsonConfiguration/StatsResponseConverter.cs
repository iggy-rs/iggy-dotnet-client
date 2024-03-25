using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Extensions;
using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Iggy_SDK.JsonConfiguration;

public class StatsResponseConverter : JsonConverter<StatsResponse>
{
    public override StatsResponse? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        //int processId = root.GetProperty(nameof(Stats.ProcessId).ToSnakeCase()).GetInt32();
        int processId = root.GetProperty(nameof(Stats.ProcessId).ToSnakeCase()).GetInt32();
        float cpuUsage = root.GetProperty(nameof(Stats.CpuUsage).ToSnakeCase()).GetSingle();
        float totalCpuUsage = root.GetProperty(nameof(Stats.TotalCpuUsage).ToSnakeCase()).GetSingle();
        string? memoryUsageString = root.GetProperty(nameof(Stats.MemoryUsage).ToSnakeCase()).GetString();
        string[] memoryUsageStringSplit = memoryUsageString.Split(' ');
        (ulong memoryUsageBytesVal, string memoryUnit) = (ulong.Parse(memoryUsageStringSplit[0]), memoryUsageStringSplit[1]);
        ulong memoryUsage = memoryUnit switch
        {
            "B" => memoryUsageBytesVal,
            "KiB" => memoryUsageBytesVal * (ulong)1e03,
            "MiB" => memoryUsageBytesVal * (ulong)1e06,
            "GiB" => memoryUsageBytesVal * (ulong)1e09,
            "TiB" => memoryUsageBytesVal * (ulong)1e12,
            _ => throw new InvalidEnumArgumentException("Error Wrong Unit when deserializing MemoryUsage")
        };
        string? totalMemoryString = root.GetProperty(nameof(Stats.TotalMemory).ToSnakeCase()).GetString();
        string[] totalMemoryStringSplit = totalMemoryString.Split(' ');
        (ulong totalMemoryUsageBytesVal, string totalMemoryUnit) = (ulong.Parse(totalMemoryStringSplit[0]), totalMemoryStringSplit[1]);
        ulong totalMemoryUsage = totalMemoryUnit switch
        {
            "B" => totalMemoryUsageBytesVal,
            "KiB" => totalMemoryUsageBytesVal * (ulong)1e03,
            "MiB" => totalMemoryUsageBytesVal * (ulong)1e06,
            "GiB" => totalMemoryUsageBytesVal * (ulong)1e09,
            "TiB" => totalMemoryUsageBytesVal * (ulong)1e12,
            _ => throw new InvalidEnumArgumentException("Error Wrong Unit when deserializing TotalMemoryUsage")
        };
        string? availableMemoryString = root.GetProperty(nameof(Stats.AvailableMemory).ToSnakeCase()).GetString();
        string[] availableMemoryStringSplit = availableMemoryString.Split(' ');
        (ulong availableMemoryBytesVal, string availableMemoryUnit) = (ulong.Parse(availableMemoryStringSplit[0]), availableMemoryStringSplit[1]);
        ulong availableMemory = availableMemoryUnit switch
        {
            "B" => availableMemoryBytesVal,
            "KiB" => availableMemoryBytesVal * (ulong)1e03,
            "MiB" => availableMemoryBytesVal * (ulong)1e06,
            "GiB" => availableMemoryBytesVal * (ulong)1e09,
            "TiB" => availableMemoryBytesVal * (ulong)1e12,
            _ => throw new InvalidEnumArgumentException("Error Wrong Unit when deserializing AvailableMemory")
        };
        var runtimeRoot = root.GetProperty(nameof(Stats.RunTime).ToSnakeCase());
        ulong runtimeNanos = 0;
        foreach (var runtimeProp in runtimeRoot.EnumerateObject())
        {
            var durationProp = runtimeProp.Value;
            var props = durationProp.EnumerateObject();
            ulong secsValue = props.First().Value.GetUInt64();
            ulong nanosValue = props.Last().Value.GetUInt64();
            nanosValue += secsValue * (ulong)1e09;
            runtimeNanos = nanosValue;
        }

        ulong startTime = root.GetProperty(nameof(Stats.StartTime).ToSnakeCase()).GetUInt64();
        string? readBytesString = root.GetProperty(nameof(Stats.ReadBytes).ToSnakeCase()).GetString();
        string[] readBytesStringSplit = readBytesString.Split(' ');
        (ulong readBytesVal, string readBytesUnit) = (ulong.Parse(readBytesStringSplit[0]), readBytesStringSplit[1]);
        ulong readBytes = readBytesUnit switch
        {
            "B" => readBytesVal,
            "KB" => readBytesVal * (ulong)1e03,
            "MB" => readBytesVal * (ulong)1e06,
            "GB" => readBytesVal * (ulong)1e09,
            "TB" => readBytesVal * (ulong)1e12,
            _ => throw new InvalidEnumArgumentException("Error Wrong Unit when deserializing ReadBytes")
        };
        string? writtenBytesString = root.GetProperty(nameof(Stats.WrittenBytes).ToSnakeCase()).GetString();
        string[] writtenBytesStringSplit = writtenBytesString.Split(' ');
        (ulong writtenBytesVal, string writtenBytesUnit) = (ulong.Parse(writtenBytesStringSplit[0]), writtenBytesStringSplit[1]);
        ulong writtenBytes = writtenBytesUnit switch
        {
            "B" => writtenBytesVal,
            "KiB" => writtenBytesVal * (ulong)1e03,
            "MiB" => writtenBytesVal * (ulong)1e06,
            "GiB" => writtenBytesVal * (ulong)1e09,
            "TiB" => writtenBytesVal * (ulong)1e12,
            _ => throw new InvalidEnumArgumentException("Error Wrong Unit when deserializing WrittenBytes")
        };
        string? messageWrittenSizeBytesString = root.GetProperty(nameof(Stats.MessagesSizeBytes).ToSnakeCase()).GetString();
        string[] messageWrittenSizeBytesStringSplit = messageWrittenSizeBytesString.Split(' ');
        (ulong messageWrittenSizeBytesVal, string messageWrittenSizeBytesUnit) = (ulong.Parse(messageWrittenSizeBytesStringSplit[0]), messageWrittenSizeBytesStringSplit[1]);
        ulong messageWrittenSizeBytes = messageWrittenSizeBytesUnit switch
        {
            "B" => messageWrittenSizeBytesVal,
            "KB" => messageWrittenSizeBytesVal * (ulong)1e03,
            "MB" => messageWrittenSizeBytesVal * (ulong)1e06,
            "GB" => messageWrittenSizeBytesVal * (ulong)1e09,
            "TB" => messageWrittenSizeBytesVal * (ulong)1e12,
            _ => throw new InvalidEnumArgumentException("Error Wrong Unit when deserializing MessageWrittenBytes")
        };
        int streamsCount = root.GetProperty(nameof(Stats.StreamsCount).ToSnakeCase()).GetInt32();
        int topicsCount = root.GetProperty(nameof(Stats.TopicsCount).ToSnakeCase()).GetInt32();
        int partitionsCount = root.GetProperty(nameof(Stats.PartitionsCount).ToSnakeCase()).GetInt32();
        int segmentsCount = root.GetProperty(nameof(Stats.SegmentsCount).ToSnakeCase()).GetInt32();
        ulong messagesCount = root.GetProperty(nameof(Stats.MessagesCount).ToSnakeCase()).GetUInt64();
        int clientsCount = root.GetProperty(nameof(Stats.ClientsCount).ToSnakeCase()).GetInt32();
        int consumerGroupsCount = root.GetProperty(nameof(Stats.ConsumerGroupsCount).ToSnakeCase()).GetInt32();
        string? hostname = root.GetProperty(nameof(Stats.Hostname).ToSnakeCase()).GetString();
        string? osName = root.GetProperty(nameof(Stats.OsName).ToSnakeCase()).GetString();
        string? osVersion = root.GetProperty(nameof(Stats.OsVersion).ToSnakeCase()).GetString();
        string? kernelVersion = root.GetProperty(nameof(Stats.KernelVersion).ToSnakeCase()).GetString();

        return new StatsResponse
        {
            AvailableMemory = availableMemory,
            ClientsCount = clientsCount,
            ConsumerGroupsCount = consumerGroupsCount,
            CpuUsage = cpuUsage,
            Hostname = hostname,
            KernelVersion = kernelVersion,
            MemoryUsage = memoryUsage,
            MessagesCount = messagesCount,
            MessagesSizeBytes = messageWrittenSizeBytes,
            OsName = osName,
            OsVersion = osVersion,
            PartitionsCount = partitionsCount,
            ProcessId = processId,
            ReadBytes = readBytes,
            RunTime = runtimeNanos,
            SegmentsCount = segmentsCount,
            StartTime = startTime,
            StreamsCount = streamsCount,
            TopicsCount = topicsCount,
            TotalCpuUsage = totalCpuUsage,
            TotalMemory = totalMemoryUsage,
            WrittenBytes = writtenBytes
        };

    }

    public override void Write(Utf8JsonWriter writer, StatsResponse value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}