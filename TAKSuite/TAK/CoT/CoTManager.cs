using TAKSuite.Data.ServicesTak;
using TAKSuite.Helper;
using TAKSuite.TAK.Helper;
using System.Threading.Tasks;
using System.Xml;

namespace TAKSuite.TAK.CoT
{
    public class CoTManager
    {
        private readonly MartiApiClient _martiApiClient;
        private readonly CoTApiClient _coTApiClient;
        private readonly CachedDataService _cache;
        private readonly WebSocketManagerCustom _webSocketManager;

        public CoTManager(MartiApiClient martiApiClient, CoTApiClient coTApiClient, WebSocketManagerCustom webSocketManager, CachedDataService service)
        {
            _martiApiClient = martiApiClient;
            _coTApiClient = coTApiClient;
            _webSocketManager = webSocketManager;
            _cache = service;

            _coTApiClient.ProcessMessageHandler = ProcessMessage;
            
        }

        public class CoTMessageEventArgs
        {
            public CoTMessageEventArgs(string message)
            {
                Message = message;
            }

            public string Message { get; set; }
        }
        //This delegate can be used to point to methods
        //which return void and take a string.
        public delegate void CotMessageReceivedHandler(CoTMessageEventArgs e);

        //This event can cause any method which conforms
        //to MyEventHandler to be called.
        public event CotMessageReceivedHandler CoTMessageReceived;


        //Here is some code I want to be executed
        //when SomethingHappened fires.
        private void OnMessageReceived(string message)
        {
            if (CoTMessageReceived != null)
                CoTMessageReceived(new CoTMessageEventArgs(message));
        }


        private async Task ProcessMessage(String message)
        {           
            
            
            message.ToString();
            OnMessageReceived(message);
            //uidarrach.ForEach(_ =>
            //{
            //    var wp = FindAnyCloserWP(uidMission, _);
            //    if (wp != null)
            //    {
            //        JoinPhotoToWaypoint("TEST FEED2", wp, _);
            //    }
            //});




            // ✅ Invio messaggio WebSocket ai client connessi
            //await _webSocketManager.BroadcastMessageAsync(message);
        }

        public async Task<bool> DeleteMissionUidAsync(string missionUid, string uid)
        {
            return await _martiApiClient.DeleteMissionUidAsync(missionUid, uid);
        }
        public async Task<bool> UpdateCoTMission(string missionUid, string uid, XmlDocument doc)
        {
            // ✅ Pulisco il CoT
            ATAKHelper.CleanFlowTags(doc);


            // ✅ Invio in broadcast del messaggio CoT
            var sXml = XmlHelper.FormatXmlPlain(doc);
            _coTApiClient.SendMessage(sXml);

            Thread.Sleep(100);

            // ✅ Aggiornamento della missione con il nuovo colore
            await _martiApiClient.UpdateMissionUidAsync(uid, missionUid);
            return true;
        }
        public async Task<bool> ChangeCotColor(string uid, int newColor)
        {
            try
            {
                var response = await _martiApiClient.GetInfoAsync(uid);
                if (response == null) return false;

                XmlDocument doc = new();
                doc.LoadXml(response);

                // ✅ Cambio del colore del CoT
                ATAKHelper.ChangeSpotmapColor(doc, newColor);
                await UpdateCoTMission("TEST FEED2", uid, doc);
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Errore nella modifica del colore: {ex.Message}");
                return false;
            }
        }
    }
}
