using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using TAKSuite.Data;
using TAKSuite.Data.Models;

namespace TAKSuite.Data.Services;

public class AtakMapsService
{
    private const string GhApiUrl = "https://api.github.com/repos/joshuafuller/ATAK-Maps/releases/latest";

    private readonly string _configPath;
    private readonly ILogger<AtakMapsService> _log;
    private readonly IHttpClientFactory _http;
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

    private volatile bool _downloading;
    private int _progress;
    private string? _error;

    public string MapsFolder { get; }
    public event Action? StateChanged;

    public bool IsDownloading => _downloading;
    public int DownloadProgress => _progress;
    public string? LastError => _error;

    public AtakMapsService(
        IWebHostEnvironment env,
        ILogger<AtakMapsService> log,
        IHttpClientFactory http,
        IDbContextFactory<ApplicationDbContext> dbFactory)
    {
        _log = log;
        _http = http;
        _dbFactory = dbFactory;
        MapsFolder = Path.Combine(env.ContentRootPath, "atak-maps");
        _configPath = Path.Combine(env.ContentRootPath, "atak-maps.json");
        Directory.CreateDirectory(MapsFolder);
    }

    // ─── Config file (installed version info) ───────────────────────────────

    public AtakMapsConfig GetConfig()
    {
        try
        {
            if (File.Exists(_configPath))
                return JsonSerializer.Deserialize<AtakMapsConfig>(File.ReadAllText(_configPath)) ?? new();
        }
        catch { }
        return new();
    }

    private void SaveConfig(AtakMapsConfig cfg)
    {
        try { File.WriteAllText(_configPath, JsonSerializer.Serialize(cfg, new JsonSerializerOptions { WriteIndented = true })); }
        catch (Exception ex) { _log.LogError(ex, "ATAK-Maps: save config failed"); }
    }

    // ─── GitHub API ──────────────────────────────────────────────────────────

    public async Task<GhRelease?> GetLatestReleaseAsync()
    {
        try
        {
            var c = _http.CreateClient();
            c.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "TAKSuite/1.0");
            var r = await c.GetAsync(GhApiUrl);
            if (!r.IsSuccessStatusCode) return null;
            return JsonSerializer.Deserialize<GhRelease>(await r.Content.ReadAsStringAsync());
        }
        catch (Exception ex) { _log.LogError(ex, "ATAK-Maps: GitHub API error"); return null; }
    }

    public async Task<(bool Available, string? LatestVersion)> CheckUpdateAsync()
    {
        var cfg = GetConfig();
        var rel = await GetLatestReleaseAsync();
        if (rel == null) return (false, null);
        return (rel.TagName != cfg.InstalledVersion, rel.TagName);
    }

    // ─── Download ────────────────────────────────────────────────────────────

    public async Task DownloadLatestAsync()
    {
        if (_downloading) return;
        _downloading = true; _progress = 0; _error = null;
        StateChanged?.Invoke();

        string? savedZipPath = null;
        try
        {
            var rel = await GetLatestReleaseAsync();
            if (rel == null) { _error = "Impossibile contattare GitHub"; return; }

            var asset = rel.Assets?.FirstOrDefault(a => a.Name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                     ?? rel.Assets?.FirstOrDefault();
            if (asset == null) { _error = "Nessun asset trovato nella release"; return; }

            var dest = Path.Combine(MapsFolder, asset.Name);
            var c = _http.CreateClient();
            c.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "TAKSuite/1.0");
            c.Timeout = TimeSpan.FromMinutes(30);

            using var resp = await c.GetAsync(asset.BrowserDownloadUrl, HttpCompletionOption.ResponseHeadersRead);
            resp.EnsureSuccessStatusCode();

            var total = resp.Content.Headers.ContentLength ?? 0;
            await using var src = await resp.Content.ReadAsStreamAsync();
            await using var dst = File.Create(dest);

            var buf = new byte[65536];
            long read = 0; int n;
            while ((n = await src.ReadAsync(buf)) > 0)
            {
                await dst.WriteAsync(buf.AsMemory(0, n));
                read += n;
                if (total > 0)
                {
                    var pct = (int)(read * 100 / total);
                    if (pct != _progress) { _progress = pct; StateChanged?.Invoke(); }
                }
            }

            dst.Close();
            savedZipPath = dest;

            var cfg = GetConfig();
            cfg.InstalledVersion = rel.TagName;
            cfg.InstalledAt = DateTime.UtcNow;
            cfg.ZipFileName = asset.Name;
            cfg.FileSizeBytes = asset.Size;
            SaveConfig(cfg);
            _progress = 100;
            StateChanged?.Invoke();
        }
        catch (Exception ex) { _log.LogError(ex, "ATAK-Maps: download failed"); _error = ex.Message; }
        finally { _downloading = false; StateChanged?.Invoke(); }

        if (savedZipPath != null)
        {
            try { await ImportMapsFromZipAsync(savedZipPath); }
            catch (Exception ex) { _log.LogError(ex, "ATAK-Maps: import failed after download"); }
        }
    }

    // ─── ZIP import ──────────────────────────────────────────────────────────

    public async Task ImportMapsFromZipAsync(string zipPath)
    {
        var parsed = new List<AtakMapSource>();

        using (var zip = ZipFile.OpenRead(zipPath))
        {
            foreach (var entry in zip.Entries)
            {
                if (!entry.Name.EndsWith(".xml", StringComparison.OrdinalIgnoreCase)) continue;
                try
                {
                    await using var stream = entry.Open();
                    var map = ParseMapXml(stream, entry.FullName);
                    if (map != null) parsed.Add(map);
                }
                catch (Exception ex)
                {
                    _log.LogDebug(ex, "ATAK-Maps: skip entry {Entry}", entry.FullName);
                }
            }
        }

        if (parsed.Count == 0) return;

        await using var db = await _dbFactory.CreateDbContextAsync();
        var existingByFile = await db.AtakMapSources
            .ToDictionaryAsync(m => m.FileName, StringComparer.OrdinalIgnoreCase);

        int added = 0, updated = 0;
        foreach (var map in parsed)
        {
            if (existingByFile.TryGetValue(map.FileName, out var existing))
            {
                var changed = existing.Name != map.Name || existing.Url != map.Url
                    || existing.Layers != map.Layers || existing.SourceType != map.SourceType
                    || existing.MinZoom != map.MinZoom || existing.MaxZoom != map.MaxZoom;
                if (changed)
                {
                    existing.Name = map.Name;
                    existing.Url = map.Url;
                    existing.Layers = map.Layers;
                    existing.SourceType = map.SourceType;
                    existing.MinZoom = map.MinZoom;
                    existing.MaxZoom = map.MaxZoom;
                    existing.Attribution = map.Attribution;
                    existing.UpdatedAt = DateTime.UtcNow;
                    updated++;
                }
            }
            else
            {
                db.AtakMapSources.Add(map);
                added++;
            }
        }

        await db.SaveChangesAsync();
        _log.LogInformation("ATAK-Maps: import complete — {Added} aggiunte, {Updated} aggiornate", added, updated);
        StateChanged?.Invoke();
    }

    private static AtakMapSource? ParseMapXml(Stream stream, string entryPath)
    {
        XDocument doc;
        try { doc = XDocument.Load(stream); }
        catch { return null; }

        var root = doc.Root;
        if (root == null) return null;

        var rootName = root.Name.LocalName;
        string sourceType;
        if (rootName.Equals("customWMSMapSource", StringComparison.OrdinalIgnoreCase))
            sourceType = "WMS";
        else if (rootName.Equals("customMapSource", StringComparison.OrdinalIgnoreCase)
              || rootName.Equals("customTileSourceMapSource", StringComparison.OrdinalIgnoreCase))
            sourceType = "XYZ";
        else
            return null;

        var name = root.Element("name")?.Value?.Trim();
        var url  = root.Element("url")?.Value?.Trim();
        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(url)) return null;
        if (!url.StartsWith("http", StringComparison.OrdinalIgnoreCase)) return null;

        // Convert ATAK tile placeholders to Leaflet format
        url = url.Replace("{$z}", "{z}").Replace("{$x}", "{x}").Replace("{$y}", "{y}");

        var layers = root.Element("layers")?.Value?.Trim();
        int.TryParse(root.Element("minZoom")?.Value, out var minZoom);
        if (!int.TryParse(root.Element("maxZoom")?.Value, out var maxZoom)) maxZoom = 19;
        var attr = root.Element("attributionUrl")?.Value?.Trim()
                ?? root.Element("attribution")?.Value?.Trim() ?? "";

        // Strip leading version-directory from path so keys stay stable across releases
        var fileName = NormalizeEntryPath(entryPath);

        return new AtakMapSource
        {
            FileName    = fileName,
            Name        = name,
            Url         = url,
            SourceType  = sourceType,
            Layers      = string.IsNullOrEmpty(layers) ? null : layers,
            MinZoom     = minZoom,
            MaxZoom     = maxZoom,
            Attribution = string.IsNullOrEmpty(attr) ? null : attr,
            IsHidden    = false,
            ImportedAt  = DateTime.UtcNow
        };
    }

    private static string NormalizeEntryPath(string fullName)
    {
        // Strip first directory segment (version folder like "ATAK-Maps-v1.0/")
        var slash = fullName.IndexOf('/');
        return slash >= 0 ? fullName[(slash + 1)..] : fullName;
    }

    // ─── DB queries ──────────────────────────────────────────────────────────

    public async Task<List<AtakMapSource>> GetAllMapsAsync()
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        return await db.AtakMapSources.OrderBy(m => m.Name).ToListAsync();
    }

    public async Task<List<AtakMapSource>> GetVisibleMapsAsync()
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        return await db.AtakMapSources.Where(m => !m.IsHidden).OrderBy(m => m.Name).ToListAsync();
    }

    public async Task SetHiddenAsync(int id, bool hidden)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var map = await db.AtakMapSources.FindAsync(id);
        if (map == null) return;
        map.IsHidden = hidden;
        await db.SaveChangesAsync();
        StateChanged?.Invoke();
    }

    // ─── Local files ─────────────────────────────────────────────────────────

    public IEnumerable<FileInfo> GetInstalledFiles() =>
        Directory.Exists(MapsFolder)
            ? new DirectoryInfo(MapsFolder).GetFiles("*.zip").OrderByDescending(f => f.LastWriteTime)
            : Enumerable.Empty<FileInfo>();

    // ─── GitHub types ────────────────────────────────────────────────────────

    public record GhRelease(
        [property: JsonPropertyName("tag_name")] string TagName,
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("published_at")] DateTime PublishedAt,
        [property: JsonPropertyName("html_url")] string HtmlUrl,
        [property: JsonPropertyName("body")] string? Body,
        [property: JsonPropertyName("assets")] List<GhAsset>? Assets
    );

    public record GhAsset(
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("size")] long Size,
        [property: JsonPropertyName("browser_download_url")] string BrowserDownloadUrl,
        [property: JsonPropertyName("content_type")] string? ContentType
    );
}

public class AtakMapsConfig
{
    public string? InstalledVersion { get; set; }
    public DateTime? InstalledAt { get; set; }
    public string? ZipFileName { get; set; }
    public long FileSizeBytes { get; set; }
}
