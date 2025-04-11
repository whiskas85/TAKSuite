using System;
using System.IO;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Security;
using System.Xml.Linq;
using TAKSuite.Data.ServicesTak;

public class CoTApiClient
{
    private readonly string serverIp;
    private readonly int serverPort;
    private readonly X509Certificate2 clientCertificate;
    private readonly CachedDataService _cacheData;
    private readonly string _keepAliveFilePath = "wwwroot/cot/presCot.xml";

    public Func<string, Task> ProcessMessageHandler { get; internal set; }

    public CoTApiClient(IConfiguration configuration, CachedDataService cacheService)
    {
        _cacheData = cacheService;

        var cotConfig = configuration.GetSection("CoTServer");
        serverIp = cotConfig["Ip"] ?? throw new ArgumentNullException("IP non configurato.");
        serverPort = int.TryParse(cotConfig["Port"], out int port) ? port : throw new ArgumentException("Porta non valida.");

        string certPath = cotConfig["CertPath"] ?? throw new ArgumentNullException("Percorso certificato non configurato.");
        string certPassword = cotConfig["CertPassword"] ?? "";

        clientCertificate = new X509Certificate2(certPath, certPassword,
            X509KeyStorageFlags.UserKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);
    }

    /// <summary>
    /// Avvia il listener per ricevere messaggi XML dal server
    /// </summary>
    public async Task StartListening(CancellationToken cancellationToken)
    {
        List<string> buffer = new();
        string? rootTagName = null;

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                using var tcpClient = new TcpClient(serverIp, serverPort);
                using var sslStream = new SslStream(tcpClient.GetStream(), false,
                    (sender, certificate, chain, sslPolicyErrors) => true); // ⚠️ NON sicuro in produzione

                sslStream.AuthenticateAsClient(serverIp, new X509Certificate2Collection(clientCertificate),
                    System.Security.Authentication.SslProtocols.Tls12, false);

                if (!sslStream.IsAuthenticated)
                {
                    Console.WriteLine("Autenticazione SSL fallita.");
                    return;
                }

                Console.WriteLine("Autenticazione SSL riuscita.");

                using var reader = new StreamReader(sslStream, Encoding.UTF8);
                while (!cancellationToken.IsCancellationRequested)
                {
                    string? line = await reader.ReadLineAsync();
                    if (line == null) continue;

                    Console.WriteLine($"Messaggio ricevuto: {line}");

                    // Ignora la dichiarazione XML
                    if (line.StartsWith("<?xml")) continue;

                    // Se non abbiamo ancora trovato il root tag
                    if (rootTagName == null)
                    {
                        // Cerca il nome del tag root
                        var match = Regex.Match(line, @"<([a-zA-Z0-9:_\-]+)[\s>]");
                        if (match.Success)
                        {
                            rootTagName = match.Groups[1].Value;
                            Console.WriteLine($"Root tag rilevato: <{rootTagName}>");
                        }
                    }

                    buffer.Add(line);

                    // Se il tag di chiusura del root è presente nella linea, processiamo
                    if (rootTagName != null && line.Contains($"</{rootTagName}>"))
                    {
                        string completeMessage = string.Join(Environment.NewLine, buffer);
                        Console.WriteLine($"Messaggio XML completo:\n{completeMessage}");

                        await ProcessMessage(completeMessage);

                        // Reset
                        buffer.Clear();
                        rootTagName = null;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore nella connessione o nel ricevere i messaggi: {ex.Message}");
                Console.WriteLine("Riprovo in 5 secondi...");
                await Task.Delay(5000, cancellationToken);
            }
        }
    }


    /// <summary>
    /// Avvia un loop che invia un messaggio di keep-alive ogni 5 minuti
    /// </summary>
    public async Task StartKeepAliveLoop(CancellationToken cancellationToken)
    {
        var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));

        while (!cancellationToken.IsCancellationRequested)
        {
            await SendKeepAliveAsync();
            await timer.WaitForNextTickAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Invia un messaggio di Keep-Alive
    /// </summary>
    private async Task SendKeepAliveAsync()
    {
        try
        {
            if (!File.Exists(_keepAliveFilePath))
            {
                Console.WriteLine($"File {_keepAliveFilePath} non trovato.");
                return;
            }

            //string xmlTemplate = await File.ReadAllTextAsync(_keepAliveFilePath);
            //string xmlContent = ReplacePlaceholders(xmlTemplate);

            //SendMessage(xmlContent);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Errore nell'invio del keep-alive: {ex.Message}");
        }
    }

    /// <summary>
    /// Sostituisce i placeholder nel file XML con i valori attuali
    /// </summary>
    private string ReplacePlaceholders(string template)
    {
        string time = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
        string start = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
        string stale = DateTime.UtcNow.AddMinutes(5).ToString("yyyy-MM-ddTHH:mm:ssZ");

        return template
            .Replace("{time}", time)
            .Replace("{start}", start)
            .Replace("{stale}", stale)
            .Replace("{uid}", "tls:344");
    }

    /// <summary>
    /// Invia un messaggio al server
    /// </summary>
    public void SendMessage(string message)
    {
        try
        {
            using TcpClient client = new(serverIp, serverPort);
            using SslStream sslStream = new(client.GetStream(), false,
                (sender, certificate, chain, sslPolicyErrors) => true);

            sslStream.AuthenticateAsClient(serverIp, new X509Certificate2Collection(clientCertificate),
                System.Security.Authentication.SslProtocols.Tls12, false);

            if (!sslStream.IsAuthenticated)
            {
                Console.WriteLine("Autenticazione SSL fallita.");
                return;
            }

            byte[] messageBytes = Encoding.UTF8.GetBytes(message + "\n");
            sslStream.Write(messageBytes, 0, messageBytes.Length);
            sslStream.Flush();

            Console.WriteLine("Messaggio inviato con successo!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Errore nell'invio del messaggio: {ex.Message}");
        }
    }

    private async Task ProcessMessage(string message)
    {
        try
        {
            Console.WriteLine($"Messaggio elaborato: {message}");

            if (_cacheData != null)
            {
                var cleanXml = CleanXml(message);
                XDocument doc = XDocument.Parse(cleanXml);
                XElement? eventElement = doc.Root;

                var uid = eventElement?.Attribute("uid")?.Value;
                var type = eventElement?.Attribute("type")?.Value;
                if (type != null)
                {
                    if (type.Contains("a-f-G-U-C"))
                    {
                        _cacheData.Add(uid, cleanXml, TimeSpan.MaxValue);
                    }
                }
            }

            if (ProcessMessageHandler != null)
                await ProcessMessageHandler(message);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Errore nell'elaborazione del messaggio: {ex.Message}");
        }
    }

    private string CleanXml(string xml)
    {
        return Regex.Replace(xml, @"<\?xml.*?\?>", "").Trim();
    }
}
