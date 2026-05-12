using System.Diagnostics;
using System.Text;
using System.Text.Json;
using TAKSuite.Data.Models;

namespace TAKSuite.Data.Services
{
    public class AiCoordinatesService
    {
        public async Task<List<AiExtractedPoint>> ExtractPointsAsync(
            string? text, string? imageBase64, string? imageMimeType)
        {
            var prompt = BuildPrompt(text);

            string? tempImagePath = null;
            try
            {
                if (!string.IsNullOrEmpty(imageBase64))
                {
                    var ext = GetImageExtension(imageMimeType);
                    tempImagePath = Path.Combine(Path.GetTempPath(), $"taksuite_ai_{Guid.NewGuid()}{ext}");
                    await File.WriteAllBytesAsync(tempImagePath, Convert.FromBase64String(imageBase64));
                }

                var args = tempImagePath != null
                    ? $"--image \"{tempImagePath}\" -p"
                    : "-p";

                var psi = new ProcessStartInfo
                {
                    FileName = "claude",
                    Arguments = args,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = Process.Start(psi)
                    ?? throw new Exception("Impossibile avviare 'claude'. Verifica che Claude Code CLI sia installato e nel PATH di sistema.");

                await process.StandardInput.WriteAsync(prompt);
                process.StandardInput.Close();

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(120));
                var outputTask = process.StandardOutput.ReadToEndAsync(cts.Token);
                var errorTask  = process.StandardError.ReadToEndAsync(cts.Token);

                await Task.WhenAll(outputTask, errorTask);
                await process.WaitForExitAsync(cts.Token);

                var output = await outputTask;
                var stderr  = await errorTask;

                if (process.ExitCode != 0 && string.IsNullOrWhiteSpace(output))
                    throw new Exception($"Claude CLI ha terminato con errore (exit {process.ExitCode}): {stderr}");

                return ParseResponse(StripJsonFences(output.Trim()));
            }
            finally
            {
                if (tempImagePath != null && File.Exists(tempImagePath))
                    File.Delete(tempImagePath);
            }
        }

        private static string BuildPrompt(string? userText)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Sei un assistente per operazioni tattiche. Analizza il contenuto e individua TUTTI i punti di interesse con coordinate.");
            sb.AppendLine();
            sb.AppendLine("Per ogni punto restituisci:");
            sb.AppendLine("- name: nome/callsign del punto");
            sb.AppendLine("- type: \"WP\" se è un waypoint (parole chiave: WP, Waypoint, PL, Phase Line, punto di passaggio)");
            sb.AppendLine("        \"OBJ\" se è un obiettivo o ricognizione (parole chiave: OBJ, Obiettivo, RECON, Target, Goal, OP)");
            sb.AppendLine("- lat/lon: coordinate WGS84 in gradi decimali.");
            sb.AppendLine("  Le coordinate sono spesso in formato UTM WGS84 (es: 32T 452390E 4520000N oppure 32T 452390 4520000).");
            sb.AppendLine("  Convertile SEMPRE in lat/lon decimali. Zona UTM tipica per l'Italia: 32T o 33T.");
            sb.AppendLine("- notes: informazioni aggiuntive opzionali (stringa vuota se assente)");
            sb.AppendLine();
            sb.AppendLine("Rispondi ESCLUSIVAMENTE con JSON valido, zero testo aggiuntivo:");
            sb.AppendLine("{\"points\":[{\"name\":\"ALFA\",\"type\":\"WP\",\"lat\":45.12,\"lon\":9.65,\"notes\":\"\"}]}");
            sb.AppendLine("Se non ci sono punti: {\"points\":[]}");

            if (!string.IsNullOrWhiteSpace(userText))
            {
                sb.AppendLine();
                sb.AppendLine("--- TESTO DA ANALIZZARE ---");
                sb.Append(userText);
            }

            return sb.ToString();
        }

        private static string StripJsonFences(string text)
        {
            if (text.StartsWith("```"))
            {
                var start = text.IndexOf('\n') + 1;
                var end = text.LastIndexOf("```");
                if (end > start) return text[start..end].Trim();
            }
            return text;
        }

        private static List<AiExtractedPoint> ParseResponse(string json)
        {
            var start = json.IndexOf('{');
            var end = json.LastIndexOf('}');
            if (start < 0 || end <= start) return new();

            using var doc = JsonDocument.Parse(json[start..(end + 1)]);
            if (!doc.RootElement.TryGetProperty("points", out var arr)) return new();

            var result = new List<AiExtractedPoint>();
            foreach (var p in arr.EnumerateArray())
            {
                var name  = p.TryGetProperty("name",  out var n) ? n.GetString() ?? "Punto" : "Punto";
                var type  = p.TryGetProperty("type",  out var t) ? t.GetString() ?? "" : "";
                var lat   = p.TryGetProperty("lat",   out var la) ? la.GetDouble() : 0;
                var lon   = p.TryGetProperty("lon",   out var lo) ? lo.GetDouble() : 0;
                var notes = p.TryGetProperty("notes", out var no) ? no.GetString() : null;

                result.Add(new AiExtractedPoint
                {
                    Name  = name,
                    Type  = type.Equals("OBJ", StringComparison.OrdinalIgnoreCase)
                                ? AiPointType.Objective
                                : AiPointType.Waypoint,
                    Lat   = lat,
                    Lon   = lon,
                    Notes = string.IsNullOrWhiteSpace(notes) ? null : notes,
                    Selected = true
                });
            }

            return result;
        }

        private static string GetImageExtension(string? mimeType) => mimeType switch
        {
            "image/png"  => ".png",
            "image/gif"  => ".gif",
            "image/webp" => ".webp",
            _            => ".jpg"
        };
    }
}
