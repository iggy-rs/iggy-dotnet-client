using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Contracts.Tcp;
using Iggy_SDK.Exceptions;
using Iggy_SDK.Utils;
using System.Buffers;
using System.Net.Sockets;
using System.Text;

namespace Iggy_SDK.MessagesDispatcher;

internal class TcpMessageInvoker : IMessageInvoker
{
    private readonly Socket _socket;
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
        var responseBuffer = MemoryPool<byte>.Shared.Rent(BufferSizes.ExpectedResponseSize);
        try
        {
            TcpContracts.CreateMessage(messageBuffer.Memory.Span[..messageBufferSize], request.StreamId, request.TopicId,
                request.Partitioning,
                messages);
            TcpMessageStreamHelpers.CreatePayload(payloadBuffer.Memory.Span[..payloadBufferSize], 
                messageBuffer.Memory.Span[..messageBufferSize], CommandCodes.SEND_MESSAGES_CODE);

            await _socket.SendAsync(payloadBuffer.Memory[..payloadBufferSize], token);
            await _socket.ReceiveAsync(responseBuffer.Memory, token);

            var response = TcpMessageStreamHelpers.GetResponseLengthAndStatus(responseBuffer.Memory.Span);
            if (response.Status != 0)
            {
                var errorBuffer = new byte[response.Length];
                await _socket.ReceiveAsync(errorBuffer, token);
                throw new InvalidResponseException(Encoding.UTF8.GetString(errorBuffer));
            }
        }
        finally
        {
            messageBuffer.Dispose();
            payloadBuffer.Dispose();
        }
    }
}