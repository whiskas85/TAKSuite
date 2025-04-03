namespace TAKSuite.TAK.CoT
{
    using TAKSuite.TAK.Helper;
    using System;
    using System.Net.Http;
    using System.Security.Cryptography.X509Certificates;
    using System.Xml;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using TAKSuite.Components.Pages;
    using System.Text.Json;
    using System.Text;
    using System.Buffers.Text;
    using TAKSuite.Data.ModelsTak;
    using TAKSuite.Data.ServicesTak;
    using System.Security.Cryptography;

    public class MartiApiClient
    {
        private readonly HttpClient client;
        private readonly CachedDataService _cache;

        public MartiApiClient(IConfiguration configuration, CachedDataService cacheService)
        {
            _cache = cacheService;

            var martiConfig = configuration.GetSection("MartiServer");
            string serverIp = martiConfig["Ip"] ?? throw new ArgumentNullException("IP non configurato.");
            int serverPort = int.TryParse(martiConfig["Port"], out int port) ? port : throw new ArgumentException("Porta non valida.");
            string certPath = martiConfig["CertPath"] ?? throw new ArgumentNullException("Percorso certificato non configurato.");
            string certPassword = martiConfig["CertPassword"] ?? "";

            try
            {
                // Carica il certificato client
                var cert = new X509Certificate2(certPath, certPassword,
                    X509KeyStorageFlags.UserKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);

                // Configura il gestore HTTP con il certificato client
                var handler = new HttpClientHandler();
                handler.ClientCertificates.Add(cert);
                handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true; // ⚠️ NON sicuro in produzione

                client = new HttpClient(handler)
                {
                    BaseAddress = new Uri($"https://{serverIp}:{serverPort}/")
                };

                Console.WriteLine("Certificato caricato con successo!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore durante il caricamento del certificato: {ex.Message}");
                throw;
            }
        }
        public async Task<string?> TestDataAsync(string missionName)
        {
            try
            {
                string url = $"Marti/api/datafeeds";
                HttpResponseMessage response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var res = response.Content.ReadAsStringAsync();
                    return await res;
                }

                Console.WriteLine($"Errore HTTP: {response.StatusCode}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Eccezione nella richiesta: {ex.Message}");
                return null;
            }
        }


        public async Task<List<MissionAtak>> GetAllMissionsDataAsync()
        {
            try
            {
                //TestDataAsync(missionName);

                string url = $"Marti/api/missions/";
                HttpResponseMessage response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var res = response.Content.ReadAsStringAsync();
                    var json = res.Result;
                    
                    var data = JsonSerializer.Deserialize<MissionsRoot>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    var missionList = data.Data.ToList();

                    return missionList; 
                }

                Console.WriteLine($"Errore HTTP: {response.StatusCode}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Eccezione nella richiesta: {ex.Message}");
                return null;
            }
        }

        public async Task<List<UidEntry>> GetAllMissionsUidAsync(string missionName)
        {
            try
            {
                string url = $"Marti/api/missions/{missionName}";
                HttpResponseMessage response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var res = response.Content.ReadAsStringAsync();
                    var json = res.Result;

                    var mission = JsonSerializer.Deserialize<MissionsRoot>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    var missionList = mission.Data[0].Uids.ToList();

                    return missionList;
                }

                Console.WriteLine($"Errore HTTP: {response.StatusCode}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Eccezione nella richiesta: {ex.Message}");
                return null;
            }
        }

        public async Task<string?> GetMissionDataAsync(string missionName)
        {
            try
            {
                //TestDataAsync(missionName);

                string url = $"Marti/api/missions/{missionName}/";
                HttpResponseMessage response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var res = response.Content.ReadAsStringAsync();
                    return await res;
                }

                Console.WriteLine($"Errore HTTP: {response.StatusCode}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Eccezione nella richiesta: {ex.Message}");
                return null;
            }
        }


        public async Task<AtakAttachment?> GetFileAsync(string fileHash)
        {
            try
            {
                string url = $"Marti/api/files/{fileHash}";
                HttpResponseMessage response = await client.GetAsync(url);



                if (response.IsSuccessStatusCode)
                {
                    AtakAttachment atc = new AtakAttachment();
                    atc.Name = response.Content.Headers.ContentDisposition.FileName.Trim('"');
                    atc.MediaType = response.Content.Headers.ContentType?.MediaType.Trim('"');

                    // Leggi il contenuto binario
                    byte[] fileBytes = await response.Content.ReadAsByteArrayAsync();
                    atc.FileBytes = fileBytes;


                    return atc;
                }

                Console.WriteLine($"Errore HTTP: {response.StatusCode}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Eccezione nella richiesta: {ex.Message}");
                return null;
            }
        }
        public async Task<string?> GetInfoAsync(string uid)
        {
            if(_cache!=null)
            {
                var message = _cache.Get(uid);
                if (message!= null)
                {
                    return message;
                }
            }

            try
            {
                string url = $"Marti/api/cot/xml/{uid}";
                HttpResponseMessage response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }

                Console.WriteLine($"Errore HTTP: {response.StatusCode}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Eccezione nella richiesta: {ex.Message}");
                return null;
            }
        }
        public async Task<EventData> GetInfoOblectAsync(string uid)
        {
            try
            {
                string url = $"Marti/api/cot/xml/{uid}";
                HttpResponseMessage response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var message = await response.Content.ReadAsStringAsync();
                    EventData eventData = EventData.LoadFromString(message);


                    return eventData;
                }

                Console.WriteLine($"Errore HTTP: {response.StatusCode}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Eccezione nella richiesta: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> ChangeCotColor(string uid, int newColor)
        {
            try
            {
                var response = await GetInfoAsync(uid);
                if (response == null) return false;

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(response);

                ATAKHelper.ChangeSpotmapColor(doc, newColor);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Eccezione nella modifica del colore: {ex.Message}");
                return false;
            }
        }

        internal async Task<bool> UpdateMissionUidAsync(string uid, string missionUid)
        {

            try
            {
                string url = $"Marti/api/missions/{missionUid}/contents";

                var requestBody = new
                {
                    uids = new[] { uid } // Sostituisci "uid" con il valore corretto
                };

                string json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PutAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                    //return await response.Content.ReadAsStringAsync();
                }

                Console.WriteLine($"Errore HTTP: {response.StatusCode}, {response.Content}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Eccezione nella richiesta: {ex.Message}");
                return false;
            }
        }


        internal async Task<bool> DeleteMissionContentAsync(string missionUid, string hash)
        {

            try
            {
                string url = $"/Marti/api/missions/{missionUid}/contents?hash={hash}";
                HttpResponseMessage response = await client.DeleteAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                    //return await response.Content.ReadAsStringAsync();
                }

                Console.WriteLine($"Errore HTTP: {response.StatusCode}, {response.Content}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Eccezione nella richiesta: {ex.Message}");
                return false;
            }
        }

        internal async Task<bool> DeleteMissionUidAsync(string missionUid, string uid)
        {

            try
            {
                string url = $"/Marti/api/missions/{missionUid}/contents?uid={uid}";
                HttpResponseMessage response = await client.DeleteAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                    //return await response.Content.ReadAsStringAsync();
                }

                Console.WriteLine($"Errore HTTP: {response.StatusCode}, {response.Content}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Eccezione nella richiesta: {ex.Message}");
                return false;
            }
        }



        internal async Task<bool> DeleteAttachmentAsync(string mission, string fileHash)
        {
            try
            {
                var res = await DeleteMissionContentAsync(mission, fileHash);


                string url = $"Marti/api/files/{fileHash}";
                HttpResponseMessage response = await client.DeleteAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }

                Console.WriteLine($"Errore HTTP: {response.StatusCode}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Eccezione nella richiesta: {ex.Message}");
                return false;
            }
        }

        internal async Task<bool> SubscribeMissionAsync(string missionUid)
        {
            try
            {
                string url = $"Marti/api/missions/subscriptions/add";

                var requestBody = new
                {
                    uid = "tls:372",               // Identificativo del client
                    protocol = "tls",     // Protocollo usato (es. "tls")
                    subaddr = "10.147.19.211",       // Indirizzo a cui inviare i dati
                    subport = 1234,       // Porta di destinazione
                    //to = "",                 // Valore non specificato, puoi definirlo
                    //xpath = "",              // Valore non specificato, puoi definirlo
                    //filterGroups = "",       // Valore non specificato, puoi definirlo
                    //iface = ""               // Valore non specificato, puoi definirlo
                };

                string json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("✅ Sottoscrizione alla missione avvenuta con successo!");
                    return true;
                }

                Console.WriteLine($"❌ Errore HTTP: {response.StatusCode}, {await response.Content.ReadAsStringAsync()}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Eccezione nella richiesta: {ex.Message}");
                return false;
            }
        }

        internal async Task<bool> UnsubscribeMissionAsync(string missionUid)
        {
            try
            {
                string url = $"Marti/api/subscriptions/delete/TAKSuitePortalServer";
                HttpResponseMessage response = await client.DeleteAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                    //return await response.Content.ReadAsStringAsync();
                }

                Console.WriteLine($"Errore HTTP: {response.StatusCode}, {response.Content}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Eccezione nella richiesta: {ex.Message}");
                return false;
            }
        }
    }
}


