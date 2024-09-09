
namespace Iggy_SDK.Contracts.Http;

public sealed class StreamResponse
{
    //{"id":940,"created_at":1725877995143672,"name":"test-stream33seze1n76bbpq","size":"0 B","messages_count":0,"topics_count":0,"topics":[]}
    
    public required int Id { get; init; }
    public required string Name { get; init; }
    public required ulong Size { get; init; }//
    public required DateTimeOffset CreatedAt { get; init; }
    public required ulong MessagesCount { get; init; }
    public required int TopicsCount { get; init; }
    public IEnumerable<TopicResponse>? Topics { get; init; }
}