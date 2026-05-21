using TAKSuite.Data.ModelsTak;
using TAKSuite.TAK.CoT;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TAKSuite.TAK.Helper;
using System.Xml;

namespace TAKSuite.Data.ServicesTak
{
    public class MedevacService
    {
        private readonly MartiApiClient _client;
        private readonly CoTManager _cotManager;

        public MedevacService(MartiApiClient client, CoTManager manager)
        {
            _client     = client;
            _cotManager = manager;
        }

        public async Task<List<Medevac>> GetAllAsync(string missionUid)
        {
            var data = await _client.GetMissionDataAsync(missionUid);
            return await GetMedevacs(data);
        }

        private async Task<List<Medevac>> GetMedevacs(string data)
        {
            var result = new List<Medevac>();
            if (data == null) return result;

            using var doc  = JsonDocument.Parse(data);
            var uids = doc.RootElement.GetProperty("data")[0].GetProperty("uids");

            foreach (var uid in uids.EnumerateArray())
            {
                var type = uid.GetProperty("details").GetProperty("type").GetString();
                if (type != "b-r-f-h-c") continue;

                var item = Medevac.Parse(uid);
                result.Add(item);

                var xml = await _client.GetInfoAsync(item.Uid);
                item.IntegrateInformationFromXml(xml);

                var creatorXml = await _client.GetInfoAsync(item.CreatorUid);
                if (creatorXml != null)
                    item.IntegrateUserInformationFromXml(creatorXml);
            }

            return result;
        }

        public Medevac? FindAnyCloserWP(List<Medevac> wpt, ATAKPhoto photo, double threshold = 10.0)
        {
            return wpt.FirstOrDefault(x => GeoUtils.ArePointsClose(x, photo, threshold));
        }

        public async Task<bool> JoinPhotoToMedevac(string missionUid, Medevac wpt, ATAKPhoto photo)
        {
            var response = await _client.GetInfoAsync(wpt.Uid);
            if (response == null) return false;

            var doc = new XmlDocument();
            doc.LoadXml(response);
            photo.AttachmentUidList.ToList().ForEach(x => AttachmentService.AddAttachment(doc, x));
            return await _cotManager.UpdateCoTMission(missionUid, wpt.Uid, doc);
        }
    }
}
