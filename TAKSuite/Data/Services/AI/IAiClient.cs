namespace TAKSuite.Data.Services.AI
{
    public record AiChatMessage(string Role, string Content);

    public interface IAiClient : IDisposable
    {
        Task<string> CompleteAsync(List<AiChatMessage> messages, CancellationToken ct = default);
        IAsyncEnumerable<string> StreamAsync(List<AiChatMessage> messages, CancellationToken ct = default);
    }
}
