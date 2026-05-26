using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace TAKSuite.Data.Services.AI
{
    public class ClaudeCliAiClient(string? model = null) : IAiClient
    {
        public async Task<string> CompleteAsync(List<AiChatMessage> messages, CancellationToken ct = default)
        {
            var sb = new StringBuilder();
            await foreach (var chunk in StreamAsync(messages, ct))
                sb.Append(chunk);
            return sb.ToString();
        }

        public async IAsyncEnumerable<string> StreamAsync(List<AiChatMessage> messages,
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            var prompt = BuildPrompt(messages);
            var args = string.IsNullOrWhiteSpace(model) ? "-p" : $"-p --model {model}";

            var psi = new ProcessStartInfo
            {
                FileName = "claude",
                Arguments = args,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding  = Encoding.UTF8,
                StandardInputEncoding  = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)
            };

            using var process = Process.Start(psi)
                ?? throw new Exception("Impossibile avviare 'claude'. Verifica che Claude Code CLI sia installato e nel PATH.");

            await process.StandardInput.WriteAsync(prompt);
            process.StandardInput.Close();

            var buffer = new char[256];
            int charsRead;
            try
            {
                while ((charsRead = await process.StandardOutput.ReadAsync(buffer.AsMemory(), ct)) > 0)
                    yield return new string(buffer, 0, charsRead);
            }
            finally
            {
                if (!process.HasExited)
                    process.Kill();
            }

            await process.WaitForExitAsync(CancellationToken.None);
        }

        private static string BuildPrompt(List<AiChatMessage> messages)
        {
            var sb = new StringBuilder();
            foreach (var m in messages)
            {
                if (m.Role == "system")
                    sb.AppendLine($"[SYSTEM]\n{m.Content}\n");
                else if (m.Role == "assistant")
                    sb.AppendLine($"[ASSISTANT]\n{m.Content}\n");
                else
                    sb.AppendLine($"[USER]\n{m.Content}\n");
            }
            return sb.ToString();
        }

        public void Dispose() { }
    }
}
