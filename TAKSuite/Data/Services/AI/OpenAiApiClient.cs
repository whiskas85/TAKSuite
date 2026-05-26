using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace TAKSuite.Data.Services.AI
{
    public class OpenAiApiClient : IAiClient
    {
        private readonly HttpClient _http;
        private readonly string _model;

        public OpenAiApiClient(string endpoint, string apiKey, string model)
        {
            _http = new HttpClient { BaseAddress = new Uri(endpoint.TrimEnd('/') + "/") };
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            _model = model;
        }

        public async Task<string> CompleteAsync(List<AiChatMessage> messages, CancellationToken ct = default)
        {
            var body = new
            {
                model = _model,
                messages = messages.Select(m => new { role = m.Role, content = m.Content }).ToArray()
            };

            var response = await _http.PostAsJsonAsync("chat/completions", body, ct);
            response.EnsureSuccessStatusCode();

            using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
            return doc.RootElement
                      .GetProperty("choices")[0]
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

            var request = new HttpRequestMessage(HttpMethod.Post, "chat/completions")
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
                if (string.IsNullOrEmpty(line) || !line.StartsWith("data: ")) continue;

                var data = line["data: ".Length..];
                if (data == "[DONE]") yield break;

                using var doc = JsonDocument.Parse(data);
                var choices = doc.RootElement.GetProperty("choices");
                if (choices.GetArrayLength() == 0) continue;

                var delta = choices[0].GetProperty("delta");
                if (delta.TryGetProperty("content", out var contentProp))
                {
                    var chunk = contentProp.GetString();
                    if (!string.IsNullOrEmpty(chunk))
                        yield return chunk;
                }
            }
        }

        public void Dispose() => _http.Dispose();
    }
}
