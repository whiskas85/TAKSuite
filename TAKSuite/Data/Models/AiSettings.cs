namespace TAKSuite.Data.Models
{
    public enum AiProvider
    {
        Ollama = 0,
        OpenAI = 1,
        ClaudeCli = 2
    }

    public class AiSettings
    {
        public int Id { get; set; } = 1;
        public bool IsEnabled { get; set; } = false;
        public AiProvider Provider { get; set; } = AiProvider.Ollama;
        public string? Endpoint { get; set; }   // Ollama: http://localhost:11434
        public string? ApiKey { get; set; }     // OpenAI / OpenAI-compatible
        public string Model { get; set; } = "";  // es. "llama3", "gpt-4o"
        public string? ExtraSystemPrompt { get; set; }
    }
}
