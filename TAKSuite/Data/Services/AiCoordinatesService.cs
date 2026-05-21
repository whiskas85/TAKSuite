using System.Diagnostics;
using System.Text;
using System.Text.Json;
using TAKSuite.Data.Models;
using TAKSuite.TAK.Helper;

namespace TAKSuite.Data.Services
{
    public class AiCoordinatesService(TakSettingsService settingsService)
    {
        public static readonly string DefaultPromptTemplate =
            "Sei un assistente per operazioni tattiche. Analizza il contenuto e individua TUTTI i punti di interesse con coordinate.\n\n" +
            "Per ogni punto restituisci:\n" +
            "- name: nome/callsign del punto\n" +
            "- type: \"WP\" se è un waypoint (parole chiave: WP, Waypoint, PL, Phase Line, punto di passaggio)\n" +
            "        \"OBJ\" se è un obiettivo o ricognizione (parole chiave: OBJ, Obiettivo, RECON, Target, Goal, OP)\n" +
            "- raw: la stringa di coordinate ESATTAMENTE come appare nel testo (es: \"32T 452390 4520000\" oppure \"45.123, 8.456\")\n" +
            "  NON convertire le coordinate — copia il testo originale nel campo raw.\n" +
            "- color: colore TAK suggerito in base al contesto. Valori ammessi (usa il nome esatto):\n" +
            "  White, Yellow, Orange, Magenta, Red, Maroon, Purple, Dark Blue, Blue, Cyan, Teal, Green, Dark Green, Grey, Black\n" +
            "  Linea guida: OBJ/Target/Goal → Red, WP/checkpoint → Green, RECON/ISR → Blue,\n" +
            "  PL/Phase Line → Yellow, OP → Orange. Se incerto, usa Red per OBJ e Green per WP.\n" +
            "- notes: informazioni aggiuntive opzionali (stringa vuota se assente)\n\n" +
            "Rispondi ESCLUSIVAMENTE con JSON valido, zero testo aggiuntivo:\n" +
            "{\"points\":[{\"name\":\"ALFA\",\"type\":\"WP\",\"raw\":\"32T 452390 4520000\",\"color\":\"Green\",\"notes\":\"\"}]}\n" +
            "Se non ci sono punti: {\"points\":[]}\n\n" +
            "--- TESTO DA ANALIZZARE ---\n" +
            "{{INPUT}}";

        public async Task<List<AiExtractedPoint>> ExtractPointsAsync(
            string? text, string? imageBase64, string? imageMimeType)
        {
            var settings = await settingsService.GetOrCreateAsync();
            var prompt = BuildPrompt(text, settings.AiPromptTemplate);

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

        private static string BuildPrompt(string? userText, string? customTemplate)
        {
            var template = string.IsNullOrWhiteSpace(customTemplate)
                ? DefaultPromptTemplate
                : customTemplate;
            return template.Replace("{{INPUT}}", userText ?? string.Empty);
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
                var notes = p.TryGetProperty("notes", out var no) ? no.GetString() : null;

                double lat = 0, lon = 0;

                // Try server-side conversion from raw coordinate string (accurate)
                if (p.TryGetProperty("raw", out var rawEl) && !string.IsNullOrWhiteSpace(rawEl.GetString()))
                {
                    GeoUtils.TryParseCoordinate(rawEl.GetString()!, out lat, out lon);
                }

                // Fall back to lat/lon fields if raw is missing or unparseable
                if (lat == 0 && lon == 0)
                {
                    lat = p.TryGetProperty("lat", out var la) ? la.GetDouble() : 0;
                    lon = p.TryGetProperty("lon", out var lo) ? lo.GetDouble() : 0;
                }

                var isObj = type.Equals("OBJ", StringComparison.OrdinalIgnoreCase);

                // Default smart color: Red for objectives, Green for waypoints
                var argbColor = isObj ? -65536 : -16711936;
                if (p.TryGetProperty("color", out var colorEl))
                {
                    var colorName = colorEl.GetString();
                    if (!string.IsNullOrWhiteSpace(colorName))
                    {
                        var found = ATAKHelper.ATAKPredefinedColors
                            .FirstOrDefault(c => c.Name.Equals(colorName, StringComparison.OrdinalIgnoreCase));
                        if (found != null) argbColor = found.Argb;
                    }
                }

                result.Add(new AiExtractedPoint
                {
                    Name      = name,
                    Type      = isObj ? AiPointType.Objective : AiPointType.Waypoint,
                    Lat       = lat,
                    Lon       = lon,
                    Notes     = string.IsNullOrWhiteSpace(notes) ? null : notes,
                    Selected  = true,
                    ArgbColor = argbColor
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
