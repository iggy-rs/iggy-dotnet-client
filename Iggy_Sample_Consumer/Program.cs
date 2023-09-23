using Iggy_SDK;
using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Enums;
using Iggy_SDK.Factory;
using Iggy_SDK.JsonConfiguration;
using Iggy_SDK.Kinds;
using Shared;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

var jsonOptions = new JsonSerializerOptions();
jsonOptions.PropertyNamingPolicy = new ToSnakeCaseNamingPolicy();
jsonOptions.WriteIndented = true;
var protocol = Protocol.Tcp;
var bus = MessageStreamFactory.CreateMessageStream(options =>
{
    options.BaseAdress = "127.0.0.1:8090";
    options.Protocol = protocol;

    options.IntervalBatchingConfig = x =>
    {
        x.Enabled = false;
        x.Interval = TimeSpan.FromMilliseconds(100);
        x.MaxMessagesPerBatch = 1000;
        x.MaxRequests = 4096;
    };
});
var permission = new Permissions
        {
            Global = new GlobalPermissions
            {
                ManageServers = true,
                ReadServers = false,
                ManageUsers = true,
                ReadUsers = false,
                ManageStreams = true,
                ReadStreams = false,
                ManageTopics = true,
                ReadTopics = false,
                PollMessages = true,
                SendMessages = false
            },
            Streams = new Dictionary<int, StreamPermissions>
            {
                {
                    1,
                    new StreamPermissions
                    {
                        ManageStream = true,
                        ReadStream = false,
                        ManageTopics = true,
                        ReadTopics = false,
                        PollMessages = true,
                        SendMessages = false,
                        Topics = new Dictionary<int, TopicPermissions>
                        {
                            {
                                101,
                                new TopicPermissions
                                {
                                    ManageTopic = true,
                                    ReadTopic = false,
                                    PollMessages = true,
                                    SendMessages = false
                                }
                            },
                            /*
                            {
                                102,
                                new TopicPermissions
                                {
                                    ManageTopic = false,
                                    ReadTopic = true,
                                    PollMessages = false,
                                    SendMessages = true
                                }
                            }
                            */
                        }
                    }
                },
                {
                    2,
                    new StreamPermissions
                    {
                        ManageStream = false,
                        ReadStream = true,
                        ManageTopics = false,
                        ReadTopics = true,
                        PollMessages = false,
                        SendMessages = true,
                        Topics = new Dictionary<int, TopicPermissions>
                        {
                            {
                                101,
                                new TopicPermissions
                                {
                                    ManageTopic = true,
                                    ReadTopic = false,
                                    PollMessages = true,
                                    SendMessages = false
                                }
                            },
                        } 
                    }
                }
            }
        };

Console.WriteLine("Using protocol : {0}", protocol.ToString());

int streamIdVal = 1;
int topicIdVal = 1;
var streamId = Identifier.Numeric(streamIdVal);
var topicId = Identifier.Numeric(topicIdVal);
var partitionId = 3;
var consumerId = 1;

Console.WriteLine($"Consumer has started, selected protocol {protocol}");

await ValidateSystem(streamId, topicId, partitionId);
await ConsumeMessages();

async Task ConsumeMessages()
{
    int intervalInMs = 1000;
    Console.WriteLine($"Messages will be polled from stream {streamId}, topic {topicId}, partition {partitionId} with interval {intervalInMs} ms");
    Func<byte[], Envelope> deserializer = serializedData =>
    {
        Envelope envelope = new Envelope();
        int messageTypeLength = BitConverter.ToInt32(serializedData, 0);
        envelope.MessageType = Encoding.UTF8.GetString(serializedData, 4, messageTypeLength);
        envelope.Payload = Encoding.UTF8.GetString(serializedData, 4 + messageTypeLength, serializedData.Length - (4 + messageTypeLength));
        return envelope;
    };
    Func<byte[], byte[]> decryptor = static payload =>
    {
        byte[] key =
        {
            0x2b, 0x7e, 0x15, 0x16, 0x28, 0xae, 0xd2, 0xa6,
            0xab, 0xf7, 0x15, 0x88, 0x09, 0xcf, 0x4f, 0x3c,
            0xa8, 0x8d, 0x2d, 0x0a, 0x9f, 0x9d, 0xea, 0x43,
            0x6c, 0x25, 0x17, 0x13, 0x20, 0x45, 0x78, 0xc8
        };
        byte[] iv =
        {
            0x5f, 0x8a, 0xe4, 0x78, 0x9c, 0x3d, 0x2b, 0x0f,
            0x12, 0x6a, 0x7e, 0x45, 0x91, 0xba, 0xdf, 0x33
        };
        using Aes aes = Aes.Create();
        ICryptoTransform decryptor = aes.CreateDecryptor(key, iv);
        using MemoryStream memoryStream = new MemoryStream(payload);
        using CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
        using BinaryReader binaryReader = new BinaryReader(cryptoStream);
        return binaryReader.ReadBytes(payload.Length);
    };
    
    await foreach (var msgResponse in bus.PollMessagesAsync<Envelope>(new PollMessagesRequest
                   {
                       Consumer = Consumer.New(consumerId),
                       Count = 1,
                       TopicId = topicId,
                       StreamId = streamId,
                       PartitionId = partitionId,
                       PollingStrategy = PollingStrategy.Next(),
                       Interval = TimeSpan.FromMilliseconds(intervalInMs),
                       StoreOffsetStrategy = StoreOffset.AfterProcessingEachMessage,
                   }, deserializer, decryptor))
    {
        HandleMessage(msgResponse);
    }
}

void HandleMessage(MessageResponse<Envelope> messageResponse)
{
    Console.Write(
        $"Handling message type: {messageResponse.Message.MessageType} with state {messageResponse.State.ToString()}, checksum: {messageResponse.Checksum}, at offset: {messageResponse.Offset} with message Id:{messageResponse.Id.ToString()} ");
    Console.WriteLine();
    Console.WriteLine("---------------------------MESSAGE-----------------------------------");
    Console.WriteLine();

    switch (messageResponse.Message.MessageType)
    {
        case "order_created":
            {
                var orderCreated = JsonSerializer.Deserialize<OrderCreated>(messageResponse.Message.Payload, jsonOptions);
                Console.WriteLine(orderCreated);
                break;
            }
        case "order_confirmed":
            {
                var orderConfirmed =
                    JsonSerializer.Deserialize<OrderConfirmed>(messageResponse.Message.Payload, jsonOptions);
                Console.WriteLine(orderConfirmed);
                break;
            }
        case "order_rejected":
            {
                var orderRejected = JsonSerializer.Deserialize<OrderRejected>(messageResponse.Message.Payload, jsonOptions);
                Console.WriteLine(orderRejected);
                break;
            }
    }


    if (messageResponse.Headers is not null)
    {
        Console.WriteLine();
        Console.WriteLine("---------------------------HEADERS-----------------------------------");
        Console.WriteLine();
        foreach (var (headerKey, headerValue) in messageResponse.Headers)
        {
            Console.WriteLine("Found Header: {0} with value: {1}, ", headerKey, headerValue);
        }
        Console.WriteLine();
    }
    //await Task.Delay(1000);
}


async Task ValidateSystem(Identifier streamId, Identifier topicId, int partitionId)
{
    try
    {
        Console.WriteLine($"Validating if stream exists.. {streamId}");
        var result = await bus.GetStreamByIdAsync(streamId);
        Console.WriteLine(result.Name);
        Console.WriteLine($"Validating if topic exists.. {topicId}");
        var topicResult = await bus.GetTopicByIdAsync(streamId, topicId);
        if (topicResult!.PartitionsCount < partitionId)
        {
            throw new SystemException(
                $"Topic {topicId} has only {topicResult.PartitionsCount} partitions, but partition {partitionId} was requested");
        }
    }
    catch
    {

        Console.WriteLine($"Creating stream with {streamId}");
        await bus.CreateStreamAsync(new StreamRequest
        {
            StreamId = streamIdVal,
            Name = "Test Consumer Stream",
        });
        Console.WriteLine($"Creating topic with {topicId}");
        await bus.CreateTopicAsync(streamId, new TopicRequest
        {
            Name = "Test Consumer Topic",
            PartitionsCount = 12,
            TopicId = topicIdVal
        });
        var topicRes = await bus.GetTopicByIdAsync(streamId, topicId);
        if (topicRes!.PartitionsCount < partitionId)
        {
            throw new SystemException(
                $"Topic {topicId} has only {topicRes.PartitionsCount} partitions, but partition {partitionId} was requested");
        }
    }

}