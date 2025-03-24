using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace TAKSuite.TAK.Helper
{
    public class WebSocketManagerCustom
    {
        private readonly ConcurrentDictionary<Guid, WebSocket> _clients = new();
        private readonly TimeSpan _keepAliveInterval;

        public WebSocketManagerCustom(IConfiguration configuration)
        {
            var webSocketConfig = configuration.GetSection("WebSocketServer");
            int keepAliveMinutes = int.TryParse(webSocketConfig["KeepAliveMinutes"], out int minutes) ? minutes : 2;
            _keepAliveInterval = TimeSpan.FromMinutes(keepAliveMinutes);
        }

        public async Task HandleConnectionAsync(WebSocket webSocket)
        {
            var clientId = Guid.NewGuid();
            _clients.TryAdd(clientId, webSocket);

            Console.WriteLine($"🔗 Nuova connessione WebSocket: {clientId}");

            var buffer = new byte[1024 * 4];
            while (webSocket.State == WebSocketState.Open)
            {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Console.WriteLine($"📩 Messaggio ricevuto da {clientId}: {receivedMessage}");
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Chiusura richiesta", CancellationToken.None);
                    _clients.TryRemove(clientId, out _);
                    Console.WriteLine($"❌ Connessione chiusa: {clientId}");
                }
            }
        }

        public async Task BroadcastMessageAsync(string message)
        {
            var buffer = Encoding.UTF8.GetBytes(message);
            var tasks = new List<Task>();

            foreach (var client in _clients.Values)
            {
                if (client.State == WebSocketState.Open)
                {
                    tasks.Add(client.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None));
                }
            }

            await Task.WhenAll(tasks);
            Console.WriteLine($"📢 Messaggio inviato a tutti i client WebSocket: {message}");
        }
    }
}
