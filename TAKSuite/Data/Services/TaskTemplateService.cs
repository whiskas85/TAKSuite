using Markdig;
using System.IO.Compression;
using System.Text;
using System.Web;
using TAKSuite.Data.Models;
using TAKSuite.Data.ServicesTak;

namespace TAKSuite.Data.Services;

public class TaskTemplateService
{
    private readonly IWebHostEnvironment _env;
    private const string RelativePath = "templates/task-template.html";
    private static readonly MarkdownPipeline _pipeline =
        new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();

    public TaskTemplateService(IWebHostEnvironment env) => _env = env;

    private string FullPath => Path.Combine(_env.WebRootPath, RelativePath);

    public async Task<string> LoadTemplateAsync()
    {
        if (!File.Exists(FullPath)) return string.Empty;
        return await File.ReadAllTextAsync(FullPath, Encoding.UTF8);
    }

    public async Task SaveTemplateAsync(string html)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(FullPath)!);
        await File.WriteAllTextAsync(FullPath, html, Encoding.UTF8);
    }

    public async Task<string> CompileAsync(TaskEntity task)
    {
        var template = await LoadTemplateAsync();
        return Compile(template, task);
    }

    public string Compile(string template, TaskEntity task)
    {
        var description = Markdown.ToHtml(task.Description ?? string.Empty, _pipeline);

        var infos = task.Items
            .Where(i => i.Type == TaskStringItemType.Info)
            .OrderBy(i => i.Order)
            .Select(i => $"<div class=\"info\">{HttpUtility.HtmlEncode(i.Value)}</div>");

        var notes = task.Items
            .Where(i => i.Type == TaskStringItemType.Note)
            .OrderBy(i => i.Order)
            .Select(i => $"<div class=\"note\">{HttpUtility.HtmlEncode(i.Value)}</div>");

        var objectives = task.Items
            .Where(i => i.Type == TaskStringItemType.Action)
            .OrderBy(i => i.Order)
            .Select(i => $"<li>{HttpUtility.HtmlEncode(i.Value)}</li>");

        return template
            .Replace("{{TITLE}}", HttpUtility.HtmlEncode(task.Name))
            .Replace("{{TYPE}}", HttpUtility.HtmlEncode(task.TipologiaObiettivo))
            .Replace("{{DURATION}}", task.Durata?.ToString() ?? "-")
            .Replace("{{RADIO_CHANNEL}}", HttpUtility.HtmlEncode(task.RadioChannel?.Name ?? "-"))
            .Replace("{{DESCRIPTION}}", description)
            .Replace("{{INFO}}", string.Join("\n", infos))
            .Replace("{{NOTES}}", string.Join("\n", notes))
            .Replace("{{OBJECTIVES}}", string.Join("\n", objectives));
    }

    public async Task<byte[]> ExportAllAsZipAsync(IEnumerable<TaskEntity> tasks)
    {
        var template = await LoadTemplateAsync();
        using var ms = new MemoryStream();
        using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (var task in tasks)
            {
                var html = Compile(template, task);
                var entryName = $"{SanitizeFilename(task.Name)}.html";
                var entry = zip.CreateEntry(entryName, CompressionLevel.Fastest);
                using var writer = new StreamWriter(entry.Open(), Encoding.UTF8);
                await writer.WriteAsync(html);
            }
        }
        ms.Position = 0;
        return ms.ToArray();
    }

    public async Task<bool> PushHtmlToTakAsync(TaskEntity task, string missionUid, AttachmentService attachmentService)
    {
        if (string.IsNullOrEmpty(task.PoiUid)) return false;
        var html = await CompileAsync(task);
        var bytes = Encoding.UTF8.GetBytes(html);
        var filename = $"{SanitizeFilename(task.Name)}.html";
        return await attachmentService.AttachHtmlAsync(missionUid, task.PoiUid, bytes, filename);
    }

    public async Task<PushToTakResult> PushAllHtmlToTakAsync(
        IEnumerable<TaskEntity> tasks, string missionUid, AttachmentService attachmentService)
    {
        var template = await LoadTemplateAsync();
        int succeeded = 0, skipped = 0, failed = 0;

        foreach (var task in tasks)
        {
            if (string.IsNullOrEmpty(task.PoiUid)) { skipped++; continue; }
            var html = Compile(template, task);
            var bytes = Encoding.UTF8.GetBytes(html);
            var filename = $"{SanitizeFilename(task.Name)}.html";
            var ok = await attachmentService.AttachHtmlAsync(missionUid, task.PoiUid, bytes, filename);
            if (ok) succeeded++; else failed++;
        }

        return new PushToTakResult(succeeded, skipped, failed);
    }

    private static string SanitizeFilename(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return string.Concat(name.Select(c => invalid.Contains(c) ? '_' : c));
    }
}

public record PushToTakResult(int Succeeded, int Skipped, int Failed)
{
    public override string ToString() =>
        $"Completati: {Succeeded} | Saltati (no UID): {Skipped} | Falliti: {Failed}";
}
