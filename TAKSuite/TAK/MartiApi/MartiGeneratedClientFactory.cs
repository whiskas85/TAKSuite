using Microsoft.Extensions.Configuration;
using System.Security.Cryptography.X509Certificates;

namespace TAKSuite.TAK.MartiApi
{
    public class MartiHttpClientProvider
    {
        public HttpClient HttpClient { get; }

        public MartiHttpClientProvider(IConfiguration configuration)
        {
            var martiConfig = configuration.GetSection("MartiServer");
            string serverIp = martiConfig["Ip"] ?? throw new ArgumentNullException("Marti IP not configured.");
            int serverPort = int.TryParse(martiConfig["Port"], out int port) ? port : throw new ArgumentException("Marti Port not configured.");
            string certPath = martiConfig["CertPath"] ?? throw new ArgumentNullException("Marti CertPath not configured.");
            string certPassword = martiConfig["CertPassword"] ?? "";

            var cert = new X509Certificate2(certPath, certPassword,
                X509KeyStorageFlags.UserKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);

            var handler = new HttpClientHandler();
            handler.ClientCertificates.Add(cert);
            handler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;

            HttpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri($"https://{serverIp}:{serverPort}/")
            };
        }
    }
}
