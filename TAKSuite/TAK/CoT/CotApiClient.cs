using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Xml.Linq;
using TAKSuite.Data.Services;
using TAKSuite.Data.ServicesTak;
using TAKSuite.TAK;
using TakLib;

public class CoTApiClient
{
    private readonly TakClientProvider _takProvider;
    private readonly CachedDataService _cacheData;
    private readonly TakTrafficLogger _trafficLog;

    public Func<string, Task>? ProcessMessageHandler { get; internal set; }

    public CoTApiClient(TakClientProvider takProvider, CachedDataService cacheService, TakTrafficLogger trafficLog)
    {
        _takProvider = takProvider;
        _cacheData   = cacheService;
        _trafficLog  = trafficLog;
    }

    public async Task StartListening(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var client = _takProvider.Client;
                if (client == null)
                {
                    await Task.Delay(10000, cancellationToken);
                    continue;
                }

                await client.ConnectStreamingAsync();

                var sa = BuildFullSaMessage(_takProvider);
                if (!string.IsNullOrEmpty(sa))
                    await client.SendOnStreamAsync(sa);

                Console.WriteLine("Streaming CoT connesso.");

                await client.ListenAsync(cotXml =>
                {
                    _ = ProcessMessage(cotXml, "TCP");
                }, cancellationToken);
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore streaming: {ex.Message}. Riprovo in 5 secondi...");
                await Task.Delay(5000, cancellationToken);
            }
        }
    }

    public async Task StartKeepAliveLoop(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(60), cancellationToken);
                await SendSaAsync();
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex)
            {
                Console.WriteLine($"[KeepAlive] Errore SA: {ex.Message}");
            }
        }
    }

    public async Task SendSaAsync()
    {
        var client = _takProvider.Client;
        if (client == null) return;
        var sa = BuildFullSaMessage(_takProvider);
        if (string.IsNullOrEmpty(sa)) return;
        try
        {
            await client.SendCotAsync(sa);
            _trafficLog.Write("SA-TX", $"callsign={_takProvider.Callsign} uid={_takProvider.ClientUid} lat={_takProvider.Latitude} lon={_takProvider.Longitude}");
        }
        catch (Exception ex) { Console.WriteLine($"[SendSaAsync] {ex.Message}"); }
    }

    public Task SendMessageAsync(string cotXml) =>
        _takProvider.Client?.SendCotAsync(cotXml) ?? Task.CompletedTask;

    public void SendMessage(string message) =>
        SendMessageAsync(message).GetAwaiter().GetResult();

    // multicastGroup: se non null, si unisce al gruppo multicast (es. "239.2.3.1" per porta 6969)
    // logFile: se non null, ogni pacchetto ricevuto viene scritto nel file per analisi
    public async Task StartUdpListeningAsync(int port, CancellationToken cancellationToken, string? multicastGroup = null, string? logFile = null)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                using var udp = new UdpClient();
                udp.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                udp.Client.Bind(new IPEndPoint(IPAddress.Any, port));

                if (multicastGroup != null)
                {
                    try { udp.JoinMulticastGroup(IPAddress.Parse(multicastGroup), IPAddress.Any); }
                    catch (Exception ex) { Console.WriteLine($"[CoT UDP] JoinMulticast fallito: {ex.Message}"); }
                }

                Console.WriteLine($"[CoT UDP] In ascolto su UDP :{port}" + (multicastGroup != null ? $" (multicast {multicastGroup})" : ""));

                while (!cancellationToken.IsCancellationRequested)
                {
                    var result    = await udp.ReceiveAsync(cancellationToken);
                    var from      = result.RemoteEndPoint;
                    var rawBytes  = result.Buffer;

                    // Try protobuf (ATAK Mesh SA on 6969); fall back to plain XML (8089)
                    var protoXml = TakProtoDecoder.TryConvertToXml(rawBytes);
                    var xml      = protoXml ?? Encoding.UTF8.GetString(rawBytes);

                    if (logFile != null)
                    {
                        try
                        {
                            var hexDump = BitConverter.ToString(rawBytes, 0, Math.Min(rawBytes.Length, 32)).Replace("-", " ");
                            var mode    = protoXml != null ? "PROTO→XML" : "XML";
                            var header  = $"--- {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} from={from} port={port} [{mode}] len={rawBytes.Length} hex={hexDump}";
                            var body    = protoXml ?? Encoding.UTF8.GetString(rawBytes);
                            await File.AppendAllTextAsync(logFile, $"{header}{Environment.NewLine}{body}{Environment.NewLine}", cancellationToken);
                        }
                        catch { }
                    }

                    _ = ProcessMessage(xml, $"UDP:{port} {from.Address}");
                }
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex)
            {
                Console.WriteLine($"[CoT UDP] Errore: {ex.Message}. Riprovo in 10s...");
                await Task.Delay(10000, cancellationToken);
            }
        }
    }

    private static string? BuildFullSaMessage(TakClientProvider p)
    {
        if (p.Latitude == null || p.Longitude == null) return null;

        var now   = DateTime.UtcNow;
        var stale = now.AddMinutes(5);
        string Fmt(DateTime d) => d.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'");

        var lat = p.Latitude.Value.ToString("F6", System.Globalization.CultureInfo.InvariantCulture);
        var lon = p.Longitude.Value.ToString("F6", System.Globalization.CultureInfo.InvariantCulture);

        var xml = new System.Text.StringBuilder();
        xml.Append($"<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        xml.Append($"<event version=\"2.0\" uid=\"{p.ClientUid}\" type=\"a-f-G-U-C\" how=\"m-g\"");
        xml.Append($" time=\"{Fmt(now)}\" start=\"{Fmt(now)}\" stale=\"{Fmt(stale)}\">");
        xml.Append($"<point lat=\"{lat}\" lon=\"{lon}\" hae=\"0\" ce=\"9999999.0\" le=\"9999999.0\"/>");
        xml.Append("<detail>");
        xml.Append($"<contact callsign=\"{p.Callsign}\" endpoint=\"*:-1:stcp\"/>");
        xml.Append($"<uid Droid=\"{p.Callsign}\"/>");
        var groupName = string.IsNullOrEmpty(p.Color) ? "Cyan" : p.Color;
        var role      = string.IsNullOrEmpty(p.Role)  ? "Team Member" : p.Role;
        xml.Append($"<__group name=\"{groupName}\" role=\"{role}\"/>");
        xml.Append("<status battery=\"100\"/>");
        xml.Append("<takv device=\"TAKSuite Server\" platform=\"TAKSuite\" os=\"Windows\" version=\"1.0\"/>");
        xml.Append("</detail>");
        xml.Append("</event>");
        return xml.ToString();
    }

    private async Task ProcessMessage(string message, string source = "TCP")
    {
        try
        {
            var doc      = XDocument.Parse(message);
            var uid      = (string?)doc.Root?.Attribute("uid") ?? "?";
            var type     = (string?)doc.Root?.Attribute("type") ?? "?";
            var callsign = (string?)doc.Root?.Element("detail")?.Element("contact")?.Attribute("callsign") ?? "";
            var pt       = doc.Root?.Element("point");
            var lat      = (string?)pt?.Attribute("lat") ?? "";
            var lon      = (string?)pt?.Attribute("lon") ?? "";
            var team     = (string?)doc.Root?.Element("detail")?.Element("__group")?.Attribute("name") ?? "";

            var summary = $"[{source}] uid={uid} type={type}";
            if (!string.IsNullOrEmpty(callsign)) summary += $" cs={callsign}";
            if (!string.IsNullOrEmpty(team))     summary += $" team={team}";
            if (!string.IsNullOrEmpty(lat))      summary += $" lat={lat} lon={lon}";
            _trafficLog.Write("COT-RX", summary);

            if (type?.Contains("a-f-G-U-C") == true)
                _cacheData?.Add(uid, message, TimeSpan.MaxValue);

            if (ProcessMessageHandler != null)
                await ProcessMessageHandler(message);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Errore elaborazione messaggio CoT: {ex.Message}");
        }
    }
}
