using TakLib;
using TAKSuite.Data.Models;
using TAKSuite.Data.Services;

namespace TAKSuite.TAK
{
    public sealed class TakClientProvider : IDisposable
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _configuration;
        private TakClient? _client;
        private readonly object _lock = new();

        public TakClient? Client { get { lock (_lock) return _client; } }
        public bool IsReady => Client != null;
        public string ClientUid { get; private set; } = "TAKSUITE-SERVER";
        public string Callsign  { get; private set; } = "TAKSuiteServer";

        public TakClientProvider(IConfiguration configuration, IServiceScopeFactory scopeFactory)
        {
            _configuration = configuration;
            _scopeFactory  = scopeFactory;

            try
            {
                var cfg = BuildConfigFromSettings(configuration);
                ClientUid = cfg.ClientUid;
                _client   = new TakClient(cfg);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TakClientProvider] Impossibile creare TakClient dall'appsettings: {ex.Message}");
            }
        }

        public async Task InitializeFromDbAsync()
        {
            using var scope    = _scopeFactory.CreateScope();
            var svc            = scope.ServiceProvider.GetRequiredService<TakSettingsService>();
            var settings       = await svc.GetOrCreateAsync();

            if (!settings.HasCertificate) return;

            await ReconfigureAsync(settings);
        }

        public async Task ReconfigureAsync(TakSettings settings)
        {
            if (settings.CertificateP12 is { Length: > 0 })
            {
                const string certPath = "wwwroot/certs/webadmin.p12";
                Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(certPath))!);
                await File.WriteAllBytesAsync(certPath, settings.CertificateP12);
            }

            var cfg = BuildConfigFromDb(settings);

            TakClient? newClient;
            try { newClient = new TakClient(cfg); }
            catch (Exception ex)
            {
                Console.WriteLine($"[TakClientProvider] ReconfigureAsync fallito: {ex.Message}");
                return;
            }

            ClientUid = cfg.ClientUid;
            Callsign  = settings.Callsign;

            TakClient? old;
            lock (_lock) { old = _client; _client = newClient; }
            if (old != null) { try { old.Dispose(); } catch { } }
        }

        private static TakConfig BuildConfigFromDb(TakSettings s) => new TakConfig
        {
            Host           = s.TakServerIp,
            HttpsPort      = s.HttpsPort,
            StreamingPort  = s.StreamingPort,
            CertPath       = "wwwroot/certs/webadmin.p12",
            CertPassword   = s.CertificatePassword,
            ClientUid      = s.ClientUid,
            EnrollmentPort = s.EnrollmentPort,
            EnrollUser     = s.EnrollUser,
            EnrollPassword = s.EnrollPassword
        };

        private static TakConfig BuildConfigFromSettings(IConfiguration configuration)
        {
            var tak   = configuration.GetSection("TakServer");
            var marti = configuration.GetSection("MartiServer");
            var cot   = configuration.GetSection("CoTServer");

            return new TakConfig
            {
                Host           = tak["Ip"]            ?? marti["Ip"]          ?? "localhost",
                HttpsPort      = int.TryParse(tak["HttpsPort"]     ?? marti["Port"], out var hp) ? hp : 8443,
                StreamingPort  = int.TryParse(tak["StreamingPort"] ?? cot["Port"],   out var sp) ? sp : 8089,
                CertPath       = tak["CertPath"]       ?? marti["CertPath"]    ?? "cert/client.p12",
                CertPassword   = tak["CertPassword"]   ?? marti["CertPassword"] ?? "",
                ClientUid      = tak["ClientUid"]      ?? "TAKSUITE-SERVER",
                EnrollmentPort = int.TryParse(tak["EnrollmentPort"], out var ep) ? ep : 8446,
                EnrollUser     = tak["EnrollUser"]     ?? "admin",
                EnrollPassword = tak["EnrollPassword"] ?? ""
            };
        }

        public void Dispose()
        {
            lock (_lock) { try { _client?.Dispose(); } catch { } }
        }
    }
}
