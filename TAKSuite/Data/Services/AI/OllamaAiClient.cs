using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace TAKSuite.Data.Services.AI
{
    public class OllamaAiClient : IAiClient
    {
        private readonly HttpClient _http;
        private readonly string _model;

        public OllamaAiClient(string endpoint, string model)
        {
            _http = new HttpClient { BaseAddress = new Uri(endpoint.TrimEnd('/') + "/") };
            _model = model;
        }

        public async Task<string> CompleteAsync(List<AiChatMessage> messages, CancellationToken ct = default)
        {
            var body = new
            {
                model = _model,
                stream = false,
                messages = messages.Select(m => new { role = m.Role, content = m.Content }).ToArray()
            };

            var response = await _http.PostAsJsonAsync("api/chat", body, ct);
            response.EnsureSuccessStatusCode();

            using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
            return doc.RootElement
                      .GetProperty("message")
                      .GetProperty("content")
                      .GetString() ?? "";
        }

        public async IAsyncEnumerable<string> StreamAsync(List<AiChatMessage> messages,
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            var body = new
            {
                model = _model,
                stream = true,
                messages = messages.Select(m => new { role = m.Role, content = m.Content }).ToArray()
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "api/chat")
            {
                Content = JsonContent.Create(body)
            };
            using var response = await _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync(ct);
            using var reader = new System.IO.StreamReader(stream);

            while (!reader.EndOfStream && !ct.IsCancellationRequested)
            {
                var line = await reader.ReadLineAsync(ct);
                if (string.IsNullOrEmpty(line)) continue;

                using var doc = JsonDocument.Parse(line);
                var content = doc.RootElement
                                 .GetProperty("message")
                                 .GetProperty("content")
                                 .GetString();
                if (!string.IsNullOrEmpty(content))
                    yield return content;

                if (doc.RootElement.TryGetProperty("done", out var done) && done.GetBoolean())
                    yield break;
            }
        }

        public void Dispose() => _http.Dispose();
    }
}
