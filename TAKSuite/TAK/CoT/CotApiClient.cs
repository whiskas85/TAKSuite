using System.Xml.Linq;
using TAKSuite.Data.ServicesTak;
using TAKSuite.TAK;
using TakLib;

public class CoTApiClient
{
    private readonly TakClientProvider _takProvider;
    private readonly CachedDataService _cacheData;

    public Func<string, Task>? ProcessMessageHandler { get; internal set; }

    public CoTApiClient(TakClientProvider takProvider, CachedDataService cacheService)
    {
        _takProvider = takProvider;
        _cacheData   = cacheService;
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

                var sa = CotEditor.BuildSaMessage(_takProvider.ClientUid, _takProvider.Callsign);
                await client.SendOnStreamAsync(sa);

                Console.WriteLine("Streaming CoT connesso.");

                await client.ListenAsync(cotXml =>
                {
                    _ = ProcessMessage(cotXml);
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

    public Task StartKeepAliveLoop(CancellationToken cancellationToken) =>
        Task.CompletedTask;

    public Task SendMessageAsync(string cotXml) =>
        _takProvider.Client?.SendCotAsync(cotXml) ?? Task.CompletedTask;

    public void SendMessage(string message) =>
        SendMessageAsync(message).GetAwaiter().GetResult();

    private async Task ProcessMessage(string message)
    {
        try
        {
            if (_cacheData != null)
            {
                var doc  = XDocument.Parse(message);
                var uid  = (string?)doc.Root?.Attribute("uid");
                var type = (string?)doc.Root?.Attribute("type");
                if (type?.Contains("a-f-G-U-C") == true)
                    _cacheData.Add(uid, message, TimeSpan.MaxValue);
            }

            if (ProcessMessageHandler != null)
                await ProcessMessageHandler(message);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Errore elaborazione messaggio CoT: {ex.Message}");
        }
    }
}
