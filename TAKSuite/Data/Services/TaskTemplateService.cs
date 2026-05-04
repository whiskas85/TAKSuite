using Markdig;
using System.IO.Compression;
using System.Text;
using System.Web;
using TAKSuite.Data.Models;

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

    private static string SanitizeFilename(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return string.Concat(name.Select(c => invalid.Contains(c) ? '_' : c));
    }
}
