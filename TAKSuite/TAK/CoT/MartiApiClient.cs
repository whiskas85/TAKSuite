namespace TAKSuite.TAK.CoT
{
    using System.Text;
    using System.Text.Json;
    using TAKSuite.Data.ModelsTak;
    using TAKSuite.Data.ServicesTak;
    using TAKSuite.TAK;
    using TAKSuite.TAK.MartiApi;
    using TakLib;

    public class MartiApiClient
    {
        private readonly TakClientProvider _takProvider;
        private readonly HttpClient _http;
        private readonly CachedDataService _cache;

        private TakClient TakClient =>
            _takProvider.Client ?? throw new InvalidOperationException("TakClient non disponibile. Configurare il certificato nelle impostazioni TAK.");
        private string CreatorUid => _takProvider.ClientUid;

        public MartiApiClient(
            TakClientProvider takProvider,
            MartiHttpClientProvider httpProvider,
            CachedDataService cacheService)
        {
            _takProvider = takProvider;
            _http        = httpProvider.HttpClient;
            _cache       = cacheService;
        }

        // ── Lista missioni ────────────────────────────────────────────────────

        public async Task<List<MissionAtak>> GetAllMissionsDataAsync()
        {
            try
            {
                var response = await _http.GetAsync("Marti/api/missions/");
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Errore HTTP GetAllMissions: {response.StatusCode}");
                    return null;
                }
                var json = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<MissionsRoot>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return data?.Data?.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Eccezione GetAllMissions: {ex.Message}");
                return null;
            }
        }

        public async Task<List<UidEntry>> GetAllMissionsUidAsync(string missionName)
        {
            try
            {
                var json    = await TakClient.GetMissionRawAsync(missionName);
                var mission = JsonSerializer.Deserialize<MissionsRoot>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return mission?.Data?[0]?.Uids?.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Eccezione GetAllMissionsUid: {ex.Message}");
                return null;
            }
        }

        public async Task<string?> GetMissionDataAsync(string missionName)
        {
            try
            {
                return await TakClient.GetMissionRawAsync(missionName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Eccezione GetMissionData: {ex.Message}");
                return null;
            }
        }

        // ── File ─────────────────────────────────────────────────────────────

        public async Task<AtakAttachment?> GetFileAsync(string fileHash)
        {
            try
            {
                var response = await _http.GetAsync($"Marti/api/files/{fileHash}");
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Errore HTTP GetFile: {response.StatusCode}");
                    return null;
                }
                var cd = response.Content.Headers.ContentDisposition;
                return new AtakAttachment
                {
                    Name      = (cd?.FileName ?? cd?.FileNameStar ?? fileHash).Trim('"'),
                    MediaType = response.Content.Headers.ContentType?.MediaType?.Trim('"'),
                    FileBytes = await response.Content.ReadAsByteArrayAsync()
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Eccezione GetFile: {ex.Message}");
                return null;
            }
        }

        public async Task<string?> UploadFileAsync(byte[] content, string filename, string contentType = "text/html")
        {
            try
            {
                var url = $"Marti/sync/upload?name={Uri.EscapeDataString(filename)}";
                using var form = new MultipartFormDataContent();
                var fileContent = new ByteArrayContent(content);
                fileContent.Headers.ContentType =
                    new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
                form.Add(fileContent, "assetfile", filename);

                var response = await _http.PostAsync(url, form);
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Errore HTTP UploadFile: {response.StatusCode}");
                    return null;
                }
                var body = (await response.Content.ReadAsStringAsync()).Trim();
                using var doc = JsonDocument.Parse(body);
                if (doc.RootElement.TryGetProperty("hash", out var hashEl))
                    return hashEl.GetString()?.Trim('"').Trim();
                return body.Trim('"');
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Eccezione UploadFile: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> AddFileToMissionAsync(string missionName, string hash)
        {
            try
            {
                var url  = $"Marti/api/missions/{Uri.EscapeDataString(missionName)}/contents";
                var body = JsonSerializer.Serialize(new { hashes = new[] { hash } });
                var resp = await _http.PutAsync(url, new StringContent(body, Encoding.UTF8, "application/json"));
                return resp.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Eccezione AddFileToMission: {ex.Message}");
                return false;
            }
        }

        public async Task<string?> UploadFileToMissionAsync(string missionName, byte[] content, string filename, string contentType = "application/octet-stream")
        {
            var hash = await UploadFileAsync(content, filename, contentType);
            if (string.IsNullOrEmpty(hash)) return null;
            await AddFileToMissionAsync(missionName, hash);
            return hash;
        }

        // ── CoT ──────────────────────────────────────────────────────────────

        public async Task<string?> GetInfoAsync(string uid)
        {
            if (_cache != null)
            {
                var cached = _cache.Get(uid);
                if (cached != null) return cached;
            }
            try
            {
                return await TakClient.GetCotByUidAsync(uid);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Eccezione GetInfo: {ex.Message}");
                return null;
            }
        }

        public async Task<EventData?> GetInfoOblectAsync(string uid)
        {
            try
            {
                var xml = await TakClient.GetCotByUidAsync(uid);
                return EventData.LoadFromString(xml);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Eccezione GetInfoObject: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> SubmitCotAsync(string xml)
        {
            try
            {
                var url      = $"Marti/sync/cot?creatorUid={CreatorUid}";
                var content  = new StringContent(xml, Encoding.UTF8, "application/xml");
                var response = await _http.PostAsync(url, content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Eccezione SubmitCot: {ex.Message}");
                return false;
            }
        }

        // ── Contenuti missione ────────────────────────────────────────────────

        internal async Task<bool> UpdateMissionUidAsync(string missionUid, IEnumerable<string> uidsList)
        {
            try
            {
                await TakClient.AddUidsToMissionAsync(missionUid, uidsList, CreatorUid);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Eccezione UpdateMissionUid(list): {ex.Message}");
                return false;
            }
        }

        internal async Task<bool> UpdateMissionUidAsync(string missionUid, string uid)
        {
            try
            {
                await TakClient.AddUidsToMissionAsync(missionUid, new[] { uid }, CreatorUid);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Eccezione UpdateMissionUid: {ex.Message}");
                return false;
            }
        }

        internal async Task<bool> DeleteMissionContentAsync(string missionUid, string hash)
        {
            try
            {
                var response = await _http.DeleteAsync(
                    $"/Marti/api/missions/{missionUid}/contents?hash={hash}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Eccezione DeleteMissionContent: {ex.Message}");
                return false;
            }
        }

        internal async Task<bool> DeleteMissionUidAsync(string missionUid, string uid)
        {
            try
            {
                await TakClient.RemoveUidFromMissionAsync(missionUid, uid, CreatorUid);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Eccezione DeleteMissionUid: {ex.Message}");
                return false;
            }
        }

        internal async Task<bool> DeleteAttachmentAsync(string mission, string fileHash)
        {
            try
            {
                await DeleteMissionContentAsync(mission, fileHash);
                var response = await _http.DeleteAsync($"Marti/api/files/{fileHash}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Eccezione DeleteAttachment: {ex.Message}");
                return false;
            }
        }

        internal async Task<bool> SendMissionAsync(string missionName)
        {
            try
            {
                var url      = $"Marti/api/missions/{Uri.EscapeDataString(missionName)}/send?creatorUid={CreatorUid}";
                var response = await _http.PostAsync(url, null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Eccezione SendMission: {ex.Message}");
                return false;
            }
        }

        // ── Subscription ─────────────────────────────────────────────────────

        public async Task<bool> SubscribeMissionAsync(string missionName)
        {
            try
            {
                await TakClient.SubscribeAsync(missionName, CreatorUid);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Eccezione SubscribeMission: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UnsubscribeMissionAsync(string missionName)
        {
            try
            {
                await TakClient.UnsubscribeAsync(missionName, CreatorUid);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Eccezione UnsubscribeMission: {ex.Message}");
                return false;
            }
        }
    }
}
