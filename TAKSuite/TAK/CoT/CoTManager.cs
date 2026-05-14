using System.Xml;
using TAKSuite.Helper;
using TAKSuite.TAK;
using TAKSuite.TAK.Helper;
using TakLib;

namespace TAKSuite.TAK.CoT
{
    public class CoTManager
    {
        private readonly TakClientProvider _takProvider;
        private readonly CoTApiClient _coTApiClient;
        private readonly WebSocketManagerCustom _webSocketManager;

        private TakClient TakClient =>
            _takProvider.Client ?? throw new InvalidOperationException("TakClient non disponibile. Configurare il certificato nelle impostazioni TAK.");
        private string CreatorUid => _takProvider.ClientUid;

        public CoTManager(
            TakClientProvider takProvider,
            CoTApiClient coTApiClient,
            WebSocketManagerCustom webSocketManager)
        {
            _takProvider      = takProvider;
            _coTApiClient     = coTApiClient;
            _webSocketManager = webSocketManager;

            _coTApiClient.ProcessMessageHandler = ProcessMessage;
        }

        // ── Event infrastructure (invariata, usata dalle pagine Blazor) ──────

        public class CoTMessageEventArgs
        {
            public CoTMessageEventArgs(string message) { Message = message; }
            public string Message { get; set; }
        }

        public delegate void CotMessageReceivedHandler(CoTMessageEventArgs e);
        public event CotMessageReceivedHandler CoTMessageReceived;

        private void OnMessageReceived(string message) =>
            CoTMessageReceived?.Invoke(new CoTMessageEventArgs(message));

        private async Task ProcessMessage(string message)
        {
            OnMessageReceived(message);
        }

        // ── Operazioni missione ───────────────────────────────────────────────

        public async Task<bool> DeleteMissionUidAsync(string missionUid, string uid)
        {
            try
            {
                await TakClient.RemoveUidFromMissionAsync(missionUid, uid, CreatorUid);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore DeleteMissionUid: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateCoTMission(string missionUid, string uid, XmlDocument doc)
        {
            ATAKHelper.CleanFlowTags(doc);
            ATAKHelper.RefreshTimestamps(doc);

            var xml = XmlHelper.FormatXmlPlain(doc);
            await TakClient.SendCotAsync(xml);

            // Attende che il server TAK elabori il CoT ricevuto via TCP prima di
            // aggiornare i contenuti della missione via REST.
            await Task.Delay(500);

            await TakClient.AddUidsToMissionAsync(missionUid, new[] { uid }, CreatorUid);
            return true;
        }

        public async Task<bool> TryUpdateCoTMission(string missionUid, string uid)
        {
            try
            {
                await TakClient.AddUidsToMissionAsync(missionUid, new[] { uid }, CreatorUid);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore TryUpdateCoTMission: {ex.Message}");
                return false;
            }
        }

        public async Task<string?> CreateAndPublishPoiAsync(
            string missionUid, string callsign, PoiType poiType,
            double lat, double lon, string? remarks = null, int? argbColor = null)
        {
            var uid = Guid.NewGuid().ToString();
            string cotXml;

            if (poiType == PoiType.Spotmap)
            {
                cotXml = CotEditor.CreateSpotMarker(
                    uid:             uid,
                    callsign:        callsign,
                    lat:             lat,
                    lon:             lon,
                    hae:             0,
                    missionDest:     missionUid,
                    colorArgb:       (argbColor ?? -1).ToString(),
                    remarks:         remarks,
                    staleAfter:      TimeSpan.FromDays(365),
                    creatorUid:      CreatorUid,
                    creatorCallsign: "TAKSuiteServer");
            }
            else
            {
                var doc = ATAKHelper.CreatePoiCoT(uid, callsign, poiType, lat, lon, remarks, argbColor);
                ATAKHelper.CleanFlowTags(doc);
                cotXml = XmlHelper.FormatXmlPlain(doc);
            }

            await TakClient.SendCotAsync(cotXml);
            await Task.Delay(200);

            try
            {
                await TakClient.AddUidsToMissionAsync(missionUid, new[] { uid }, CreatorUid);
                return uid;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore AddUidsToMission: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> ChangeCotColor(string missionUID, string uid, int newColor)
        {
            try
            {
                var xml = await TakClient.GetCotByUidAsync(uid);

                var modified = CotEditor.Modify(xml,
                    newColor:    newColor.ToString(),
                    missionDest: string.IsNullOrEmpty(missionUID) ? null : missionUID,
                    staleAfter:  TimeSpan.FromDays(365));

                await TakClient.SendCotAsync(modified);
                await Task.Delay(300);

                if (!string.IsNullOrEmpty(missionUID))
                    await TakClient.AddUidsToMissionAsync(missionUID, new[] { uid }, CreatorUid);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore ChangeCotColor: {ex.Message}");
                return false;
            }
        }
    }
}
