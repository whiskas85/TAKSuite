namespace TAKSuite.TAK.CoT
{
    using System;
    using System.IO;
    using System.Net.Sockets;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Net.Security;
    using Microsoft.AspNetCore.Http;
    using TAKSuite.Data.ServicesTak;
    using System.Xml.Linq;
    using System.Text.RegularExpressions;

    public class CoTApiClient
    {
        private readonly string serverIp;
        private readonly int serverPort;
        private readonly X509Certificate2 clientCertificate;
        private readonly CachedDataService _cacheData;


        public Func<string, Task> ProcessMessageHandler { get; internal set; }

        public CoTApiClient(IConfiguration configuration, CachedDataService cacheService)
        {
            _cacheData = cacheService;

            var cotConfig = configuration.GetSection("CoTServer");
            serverIp = cotConfig["Ip"] ?? throw new ArgumentNullException("IP non configurato.");
            serverPort = int.TryParse(cotConfig["Port"], out int port) ? port : throw new ArgumentException("Porta non valida.");

            string certPath = cotConfig["CertPath"] ?? throw new ArgumentNullException("Percorso certificato non configurato.");
            string certPassword = cotConfig["CertPassword"] ?? "";

            try
            {
                // Carica il certificato client
                clientCertificate = new X509Certificate2(certPath, certPassword,
                    X509KeyStorageFlags.UserKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);

                Console.WriteLine("Certificato caricato con successo!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore nel caricamento del certificato: {ex.Message}");
                throw;
            }
        }

        public async Task StartListening(CancellationToken cancellationToken)
        {
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
                        string message = await reader.ReadLineAsync();
                        if (message != null)
                        {
                            Console.WriteLine($"Messaggio ricevuto: {message}");
                            await ProcessMessage(message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Errore nella connessione o nel ricevere i messaggi: {ex.Message}");
                    Console.WriteLine("Riprovo in 5 secondi...");
                    await Task.Delay(5000); // Attendi 5 secondi prima di riprovare a connetterti
                }
            }
        }

        private async Task ProcessMessage(string message)
        {
            // Elabora il messaggio ricevuto (ad esempio, deserializzando il JSON)
            try
            {
                var parsedMessage = message; // Sostituisci con il tipo specifico, se necessario
                Console.WriteLine($"Messaggio elaborato: {parsedMessage}");


                if (_cacheData!=null)
                {
                    var cleanXml = CleanXml(message);
                    XDocument doc = XDocument.Parse(cleanXml);
                    XElement? eventElement = doc.Root;

                    var uid = eventElement?.Attribute("uid")?.Value;
                    var type = eventElement?.Attribute("type")?.Value;

                    if (type.Contains("a-f-G-U-C"))
                    {
                        _cacheData.Add(uid, cleanXml, TimeSpan.MaxValue);
                    }
                }



                if (ProcessMessageHandler != null) 
                    ProcessMessageHandler(message);

                // Condizione che determina quando inviare un messaggio via WebSocket
                if (parsedMessage.Contains("invioWebSocket"))
                {
                    if (ProcessMessageHandler!=null) ProcessMessageHandler(message);
                    // websocket client
                    //_webSocketManager.BroadcastMessageAsync(parsedMessage);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore nell'elaborazione del messaggio: {ex.Message}");
            }
        }


        string CleanXml(string xml)
        {
            return Regex.Replace(xml, @"<\?xml.*?\?>", "").Trim(); // Rimuove la dichiarazione XML
        }







        public void SendMessage(string message)
        {
            try
            {
                using TcpClient client = new(serverIp, serverPort);
                using SslStream sslStream = new(client.GetStream(), false,
                    (sender, certificate, chain, sslPolicyErrors) => true); // ⚠️ NON sicuro in produzione
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

        
    }
}
