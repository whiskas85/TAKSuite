using System.Runtime.CompilerServices;
using TAKSuite.Data.Models;

namespace TAKSuite.Data.Services.AI
{
    public class AiService(AiSettingsService settingsService)
    {
        public async Task<bool> IsAvailableAsync()
        {
            var s = await settingsService.GetOrCreateAsync();
            if (!s.IsEnabled) return false;
            // Claude CLI non richiede un modello esplicito
            if (s.Provider == AiProvider.ClaudeCli) return true;
            return !string.IsNullOrEmpty(s.Model);
        }

        // Crea il client corretto in base alla configurazione salvata
        public async Task<IAiClient?> CreateClientAsync()
        {
            var s = await settingsService.GetOrCreateAsync();
            if (!s.IsEnabled) return null;

            return s.Provider switch
            {
                AiProvider.Ollama =>
                    new OllamaAiClient(
                        string.IsNullOrWhiteSpace(s.Endpoint) ? "http://localhost:11434" : s.Endpoint,
                        s.Model),

                AiProvider.OpenAI =>
                    new OpenAiApiClient(
                        string.IsNullOrWhiteSpace(s.Endpoint) ? "https://api.openai.com/v1" : s.Endpoint,
                        s.ApiKey ?? "",
                        s.Model),

                AiProvider.ClaudeCli =>
                    new ClaudeCliAiClient(string.IsNullOrWhiteSpace(s.Model) ? null : s.Model),

                _ => null
            };
        }

        // Risposta completa (usata dal test connessione)
        public async Task<string?> ChatAsync(List<AiChatMessage> messages, CancellationToken ct = default)
        {
            using var client = await CreateClientAsync();
            if (client == null) return null;
            return await client.CompleteAsync(messages, ct);
        }

        // Streaming: ogni chunk del testo generato viene emesso appena disponibile.
        // Il timeout si azzera ad ogni chunk ricevuto (60 s di silenzio = abort).
        public async IAsyncEnumerable<string> StreamChatAsync(List<AiChatMessage> messages,
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            using var client = await CreateClientAsync();
            if (client == null) yield break;

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromSeconds(60));

            await foreach (var chunk in client.StreamAsync(messages, cts.Token))
            {
                cts.CancelAfter(TimeSpan.FromSeconds(60)); // reset timeout su ogni chunk
                yield return chunk;
            }
        }

        // Costruisce il prompt di sistema base (aggiungibile con contesto specifico per pagina)
        public async Task<string> BuildBaseSystemPromptAsync(string? extraContext = null)
        {
            var s = await settingsService.GetOrCreateAsync();
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Sei un assistente integrato in TAKSuite, un'applicazione di gestione missioni tattiche.");
            sb.AppendLine("Rispondi sempre in italiano, in modo conciso e operativo.");
            sb.AppendLine("Puoi leggere le informazioni di contesto fornite e rispondere a domande su di esse.");
            if (!string.IsNullOrWhiteSpace(s.ExtraSystemPrompt))
                sb.AppendLine(s.ExtraSystemPrompt);
            if (!string.IsNullOrWhiteSpace(extraContext))
                sb.AppendLine(extraContext);
            return sb.ToString();
        }
    }
}
