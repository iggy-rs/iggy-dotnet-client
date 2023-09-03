using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Contracts.Tcp;
using Iggy_SDK.Exceptions;
using Iggy_SDK.Utils;
using System.Buffers;
using System.Net.Sockets;

namespace Iggy_SDK.MessagesDispatcher;

internal sealed class TcpMessageInvoker : MessageInvoker
{
    private readonly Socket _socket;

    //TODO - make this readonly
    private Memory<byte> _responseBuffer = new(new byte[BufferSizes.ExpectedResponseSize]);

    public TcpMessageInvoker(Socket socket)
    {
        _socket = socket;
    }
    internal override async Task SendMessagesAsync(MessageSendRequest request,
        CancellationToken token = default)
    {
        var messages = request.Messages;
        var streamTopicIdLength = 2 + request.StreamId.Length + 2 + request.TopicId.Length;
        var messageBufferSize = TcpMessageStreamHelpers.CalculateMessageBytesCount(messages)
                       + request.Partitioning.Length + streamTopicIdLength + 2;
        var payloadBufferSize = messageBufferSize + 4 + BufferSizes.InitialBytesLength;

        var message = ArrayPool<byte>.Shared.Rent(messageBufferSize);
        var payload = ArrayPool<byte>.Shared.Rent(payloadBufferSize);
        try
        {
            TcpContracts.CreateMessage(message.AsSpan()[..messageBufferSize], request.StreamId, request.TopicId,
                request.Partitioning,
                messages);
            TcpMessageStreamHelpers.CreatePayload(payload.AsSpan()[..payloadBufferSize], message.AsSpan()[..messageBufferSize], CommandCodes.SEND_MESSAGES_CODE);

            var recv = _socket.ReceiveAsync(_responseBuffer, token);
            await _socket.SendAsync(payload.AsMemory()[..payloadBufferSize], token);

            await recv;

            var status = TcpMessageStreamHelpers.GetResponseStatus(_responseBuffer.Span);
            if (status != 0)
            {
                throw new InvalidResponseException($"Invalid response status code: {status}");
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(message);
            ArrayPool<byte>.Shared.Return(payload);
        }
    }
}