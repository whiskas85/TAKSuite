using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json.Nodes;
using System.Xml.Linq;

// ─── Configurazione (da appsettings.json di TAKSuite) ───────────────────────
var cfg = new TakConfig
{
    Host          = "10.147.19.5",
    HttpsPort     = 8443,
    StreamingPort = 8089,
    CertPath      = @"..\TAKSuite\wwwroot\certs\webadmin.p12",
    CertPassword  = "atakatak"
};

string missionName   = "op. test";
string? targetUid    = null;       // null = cerca per callsign, o metti il GUID diretto
string? searchCallsign = "WP 1";   // cercato tra tutti i COT della missione (ignorato se targetUid != null)

double? newLat     = null;    // verrà impostato dopo la lettura (+0.001)
double? newLon     = null;
double? newHae     = null;
string? newRemarks = null;
// ────────────────────────────────────────────────────────────────────────────

using var tak = new TakClient(cfg);

// 1. Recupera la missione e l'elenco UID
Console.WriteLine($"[1] Lettura missione '{missionName}'...");
var mission = await tak.GetMissionAsync(missionName);
var uids = ExtractUids(mission);
Console.WriteLine($"    Trovati {uids.Count} COT nella missione:");
foreach (var u in uids) Console.WriteLine($"      - {u}");

if (uids.Count == 0) { Console.WriteLine("Nessun COT nella missione. Stop."); return; }

// 2. Seleziona il COT da modificare (per GUID diretto o per callsign)
string? uidToUpdate = targetUid;
if (uidToUpdate == null && searchCallsign != null)
{
    Console.WriteLine($"\n[2] Ricerca per callsign '{searchCallsign}'...");
    foreach (var u in uids)
    {
        try
        {
            var xml = await tak.GetCotByUidAsync(u);
            var cs  = GetCallsign(xml);
            Console.WriteLine($"      {u} → {cs}");
            if (string.Equals(cs, searchCallsign, StringComparison.OrdinalIgnoreCase))
            { uidToUpdate = u; break; }
        }
        catch { /* COT non trovato, skip */ }
    }
    if (uidToUpdate == null)
    { Console.WriteLine($"    Callsign '{searchCallsign}' non trovato. Stop."); return; }
}
uidToUpdate ??= uids[0];
Console.WriteLine($"\n[2] COT selezionato: {uidToUpdate}");

// 3. Legge il contenuto attuale del COT
Console.WriteLine("\n[3] Lettura contenuto COT...");
string originalCot = await tak.GetCotByUidAsync(uidToUpdate);
Console.WriteLine("    ── Contenuto originale ──");
Console.WriteLine(originalCot);

var info = ParseCotInfo(originalCot);
Console.WriteLine($"    Tipo : {info.Type}");
Console.WriteLine($"    Lat  : {info.Lat}");
Console.WriteLine($"    Lon  : {info.Lon}");
Console.WriteLine($"    HAE  : {info.Hae}");
Console.WriteLine($"    Note : {info.Remarks}");

// 4. Modifica il COT — sposta lat +0.001 per verifica
if (double.TryParse(info.Lat, System.Globalization.NumberStyles.Float,
    System.Globalization.CultureInfo.InvariantCulture, out var origLat))
    newLat = Math.Round(origLat + 0.001, 7);

Console.WriteLine("\n[4] Modifica COT...");
string modifiedCot = CotEditor.Modify(originalCot,
    newLat: newLat, newLon: newLon, newHae: newHae, newRemarks: newRemarks);
Console.WriteLine("    ── COT modificato ──");
Console.WriteLine(modifiedCot);

// 5. Invia il COT al TAK Server (stesso UID = aggiornamento)
Console.WriteLine("\n[5] Invio COT al TAK Server...");
await tak.SendCotAsync(modifiedCot);
Console.WriteLine("    OK");

// 6. Verifica: rilegge il COT dalla missione
await Task.Delay(1000);
Console.WriteLine("\n[6] Verifica: rilettura COT...");
string updatedCot = await tak.GetCotByUidAsync(uidToUpdate);
var updatedInfo = ParseCotInfo(updatedCot);
Console.WriteLine($"    Lat  : {updatedInfo.Lat}  (era {info.Lat})");
var latOk = updatedInfo.Lat != info.Lat;
Console.WriteLine(latOk ? "    OK Aggiornamento lat confermato" : "    !! Lat invariata — il server potrebbe non aver accettato l'update");

Console.WriteLine("\nOperazione completata.");

// ─── Helper ──────────────────────────────────────────────────────────────────
static List<string> ExtractUids(JsonNode? mission)
{
    var list = new List<string>();
    var node = mission?["data"]?[0] ?? mission?["data"] ?? mission;
    var uidsArr = node?["uids"] as JsonArray;
    if (uidsArr != null)
        foreach (var item in uidsArr)
        {
            var u = item?["data"]?.GetValue<string>() ?? item?["uid"]?.GetValue<string>();
            if (!string.IsNullOrEmpty(u)) list.Add(u);
        }
    return list;
}

static string? GetCallsign(string xml)
{
    try
    {
        var doc = XDocument.Parse(xml);
        return (string?)doc.Root?.Element("detail")?.Element("contact")?.Attribute("callsign")
            ?? (string?)doc.Root?.Attribute("uid"); // fallback
    }
    catch { return null; }
}

static (string? Type, string? Lat, string? Lon, string? Hae, string? Remarks) ParseCotInfo(string xml)
{
    var doc = XDocument.Parse(xml);
    var ev  = doc.Root!;
    var pt  = ev.Element("point");
    var rem = ev.Element("detail")?.Element("remarks")?.Value;
    return ((string?)ev.Attribute("type"),
            (string?)pt?.Attribute("lat"),
            (string?)pt?.Attribute("lon"),
            (string?)pt?.Attribute("hae"), rem);
}

// ─── Tipi ────────────────────────────────────────────────────────────────────
record TakConfig
{
    public string Host          { get; init; } = "localhost";
    public int    HttpsPort     { get; init; } = 8443;
    public int    StreamingPort { get; init; } = 8089;
    public string CertPath      { get; init; } = "client.p12";
    public string CertPassword  { get; init; } = "";
}

class TakClient : IDisposable
{
    private readonly TakConfig _cfg;
    private readonly HttpClient _http;
    private readonly X509Certificate2 _cert;

    public TakClient(TakConfig cfg)
    {
        _cfg  = cfg;
        _cert = new X509Certificate2(cfg.CertPath, cfg.CertPassword);
        var handler = new HttpClientHandler();
        handler.ClientCertificates.Add(_cert);
        handler.ServerCertificateCustomValidationCallback =
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
        _http = new HttpClient(handler)
        {
            BaseAddress = new Uri($"https://{cfg.Host}:{cfg.HttpsPort}")
        };
    }

    public async Task<JsonNode?> GetMissionAsync(string name)
    {
        var r = await _http.GetAsync($"/Marti/api/missions/{Uri.EscapeDataString(name)}");
        r.EnsureSuccessStatusCode();
        return JsonNode.Parse(await r.Content.ReadAsStringAsync());
    }

    public async Task<string> GetCotByUidAsync(string uid)
    {
        var r = await _http.GetAsync($"/Marti/api/cot/xml/{Uri.EscapeDataString(uid)}");
        r.EnsureSuccessStatusCode();
        return await r.Content.ReadAsStringAsync();
    }

    public async Task SendCotAsync(string cotXml)
    {
        using var tcp = new TcpClient();
        await tcp.ConnectAsync(_cfg.Host, _cfg.StreamingPort);
        using var ssl = new SslStream(tcp.GetStream(), false);
        await ssl.AuthenticateAsClientAsync(new SslClientAuthenticationOptions
        {
            TargetHost          = _cfg.Host,
            ClientCertificates  = new X509CertificateCollection { _cert },
            EnabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13,
            RemoteCertificateValidationCallback = (_, _, _, _) => true
        });
        var bytes = Encoding.UTF8.GetBytes(cotXml);
        await ssl.WriteAsync(bytes);
        await ssl.FlushAsync();
        Console.WriteLine($"    Inviati {bytes.Length} byte (TLS: {ssl.SslProtocol})");
        await Task.Delay(300);
    }

    public void Dispose() { _http.Dispose(); _cert.Dispose(); }
}

static class CotEditor
{
    public static string Modify(string cotXml,
        double? newLat = null, double? newLon = null, double? newHae = null,
        string? newRemarks = null, string? newType = null)
    {
        var doc  = XDocument.Parse(cotXml);
        var root = doc.Root ?? throw new InvalidOperationException("XML COT non valido");
        var now  = DateTime.UtcNow;
        root.SetAttributeValue("time",  Fmt(now));
        root.SetAttributeValue("start", Fmt(now));
        root.SetAttributeValue("stale", Fmt(now.AddMinutes(10)));
        if (newType != null) root.SetAttributeValue("type", newType);
        var pt = root.Element("point");
        if (pt != null)
        {
            var ic = System.Globalization.CultureInfo.InvariantCulture;
            if (newLat.HasValue) pt.SetAttributeValue("lat", newLat.Value.ToString("F7", ic));
            if (newLon.HasValue) pt.SetAttributeValue("lon", newLon.Value.ToString("F7", ic));
            if (newHae.HasValue) pt.SetAttributeValue("hae", newHae.Value.ToString("F1", ic));
        }
        if (newRemarks != null)
        {
            var detail = root.Element("detail") ?? new XElement("detail");
            if (root.Element("detail") == null) root.Add(detail);
            var rem = detail.Element("remarks");
            if (rem != null) rem.SetValue(newRemarks);
            else             detail.Add(new XElement("remarks", newRemarks));
        }
        // Rimuove _flow-tags_: il server ignora COT con il proprio ID già presente
        root.Element("detail")?.Element("_flow-tags_")?.Remove();

        return "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" + root.ToString();
    }

    static string Fmt(DateTime dt) =>
        dt.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'");
}
