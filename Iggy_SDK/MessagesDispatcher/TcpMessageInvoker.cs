using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Contracts.Tcp;
using Iggy_SDK.Exceptions;
using Iggy_SDK.Utils;
using System.Buffers;
using System.Net.Sockets;

namespace Iggy_SDK.MessagesDispatcher;

internal class TcpMessageInvoker : IMessageInvoker
{
    private readonly Socket _socket;

    private readonly Memory<byte> _responseBuffer = new(new byte[BufferSizes.ExpectedResponseSize]);

    public TcpMessageInvoker(Socket socket)
    {
        _socket = socket;
    }
    public async Task SendMessagesAsync(MessageSendRequest request,
        CancellationToken token = default)
    {
        var messages = request.Messages;
        var streamTopicIdLength = 2 + request.StreamId.Length + 2 + request.TopicId.Length;
        var messageBufferSize = TcpMessageStreamHelpers.CalculateMessageBytesCount(messages)
                       + request.Partitioning.Length + streamTopicIdLength + 2;
        var payloadBufferSize = messageBufferSize + 4 + BufferSizes.InitialBytesLength;

        var messageBuffer = MemoryPool<byte>.Shared.Rent(messageBufferSize);
        var payloadBuffer = MemoryPool<byte>.Shared.Rent(payloadBufferSize);
        try
        {
            TcpContracts.CreateMessage(messageBuffer.Memory.Span[..messageBufferSize], request.StreamId, request.TopicId,
                request.Partitioning,
                messages);
            TcpMessageStreamHelpers.CreatePayload(payloadBuffer.Memory.Span[..payloadBufferSize], 
                messageBuffer.Memory.Span[..messageBufferSize], CommandCodes.SEND_MESSAGES_CODE);

            await _socket.SendAsync(payloadBuffer.Memory[..payloadBufferSize], token);
            await _socket.ReceiveAsync(_responseBuffer, token);

            var status = TcpMessageStreamHelpers.GetResponseStatus(_responseBuffer.Span);
            if (status != 0)
            {
                throw new InvalidResponseException($"Invalid response status code: {status}");
            }
        }
        finally
        {
            messageBuffer.Dispose();
            payloadBuffer.Dispose();
        }
    }
}