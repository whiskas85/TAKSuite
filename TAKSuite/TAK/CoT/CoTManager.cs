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
            // TAK server must ingest the CoT via TCP before the REST uid-association call succeeds
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

        /// <summary>
        /// Genera e pubblica un punto TAK partendo da un template COT.
        /// I placeholder nel CotXml vengono sostituiti con i valori reali.
        /// </summary>
        public async Task<string?> CreateFromTemplateAsync(
            Data.Models.CotTemplate template, string missionUid,
            Dictionary<string, object> dicName)
        {
            var uid  = Guid.NewGuid().ToString();
            var now  = DateTime.UtcNow;
            var stale = now.AddYears(1);

            // Inject runtime values not provided by the caller (UID, timestamps, server identity)
            var extra = new Dictionary<string, object>
            {
                { "UID", uid },
                { "MISSION_UID", missionUid },
                { "TIME", now },
                { "STALE", stale },
                { "CREATOR_UID", CreatorUid },
            };

            var mapper = new CotTemplateMapper(dicName, extra);
            var cotXml = mapper.ApplyTemplate(template.CotXml);


            await TakClient.SendCotAsync(cotXml);
            // TAK server must ingest the CoT via TCP before the REST uid-association call succeeds
            await Task.Delay(200);

            try
            {
                await TakClient.AddUidsToMissionAsync(missionUid, new[] { uid }, CreatorUid);
                return uid;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CreateFromTemplate] AddUidsToMission: {ex.Message}");
                return null;
            }
        }

        public async Task<string?> CreateParkingAsync(string missionUid, string name, double lat, double lon, int argbColor = -1)
        {
            var uid   = Guid.NewGuid().ToString();
            var now   = DateTime.UtcNow;
            var stale = now.AddYears(1);
            var inv   = System.Globalization.CultureInfo.InvariantCulture;
            var safeName = System.Security.SecurityElement.Escape(name);

            var cotXml =
                $"<?xml version='1.0' encoding='UTF-8' standalone='yes'?>" +
                $"<event version='2.0' uid='{uid}' type='a-u-G' " +
                $"time='{now.ToString("yyyy-MM-ddTHH:mm:ssZ", inv)}' " +
                $"start='{now.ToString("yyyy-MM-ddTHH:mm:ssZ", inv)}' " +
                $"stale='{stale.ToString("yyyy-MM-ddTHH:mm:ssZ", inv)}' how='h-g-i-g-o'>" +
                $"<point lat='{lat.ToString("G17", inv)}' lon='{lon.ToString("G17", inv)}' hae='9999999.0' ce='9999999.0' le='9999999.0' />" +
                $"<detail>" +
                $"<contact callsign=\"{safeName}\"/>" +
                $"<precisionlocation geopointsrc=\"???\" altsrc=\"DTED2\"/>" +
                $"<archive/>" +
                $"<color argb=\"{argbColor}\"/>" +
                $"<usericon iconsetpath=\"6d781afb-89a6-4c07-b2b9-a89748b6a38f/Transport/parking.png\"/>" +
                $"</detail></event>";

            await TakClient.SendCotAsync(cotXml);
            // TAK server must ingest the CoT via TCP before the REST uid-association call succeeds;
            // parking uses 300 ms instead of 200 ms because the iconset lookup is slightly slower
            await Task.Delay(300);

            try
            {
                await TakClient.AddUidsToMissionAsync(missionUid, new[] { uid }, CreatorUid);
                return uid;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CreateParking] AddUidsToMission: {ex.Message}");
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
                // TAK server must ingest the updated CoT via TCP before the REST uid-association call succeeds
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
