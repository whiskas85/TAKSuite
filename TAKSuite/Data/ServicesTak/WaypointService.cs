using TAKSuite.Data.ModelsTak;
using TAKSuite.TAK.CoT;
using System.Text.Json;
using TAKSuite.TAK.Helper;

namespace TAKSuite.Data.ServicesTak
{
    public record TakPoint(string Uid, string? Callsign, double Lat, double Lon);

    public class WaypointService
    {
        private readonly MartiApiClient _client;
        public WaypointService(MartiApiClient client)
        {
            _client = client;
        }

        public async Task<List<Waypoint>> GetAllWaypointsAsync(string missionUid)
        {
            var data = await _client.GetMissionDataAsync(missionUid);
            return GetWaypoints(data);
        }

        public async Task<List<TakPoint>> GetAllMissionPointsAsync(string missionUid)
        {
            var data = await _client.GetMissionDataAsync(missionUid);
            return ParseAllPoints(data);
        }

        private List<TakPoint> ParseAllPoints(string data)
        {
            var result = new List<TakPoint>();
            if (string.IsNullOrEmpty(data)) return result;
            try
            {
                using var doc = JsonDocument.Parse(data);
                var uids = doc.RootElement.GetProperty("data")[0].GetProperty("uids");
                foreach (var uid in uids.EnumerateArray())
                {
                    try
                    {
                        var uidStr   = uid.GetProperty("data").GetString() ?? string.Empty;
                        var callsign = uid.GetProperty("details").GetProperty("callsign").GetString();
                        if (!uid.GetProperty("details").TryGetProperty("location", out var loc)) continue;
                        var lat = loc.GetProperty("lat").GetDouble();
                        var lon = loc.GetProperty("lon").GetDouble();
                        if (lat == 0 && lon == 0) continue;
                        result.Add(new TakPoint(uidStr, callsign, lat, lon));
                    }
                    catch { }
                }
            }
            catch { }
            return result;
        }

        public async Task<List<UidEntry>> GetAllMissionCotAsync(string missionUid)
        {
            var data = await _client.GetMissionDataAsync(missionUid);
            return GetMissionCot(data);
        }

        public Task SubscribeMissionAsync(string mission)   => _client.SubscribeMissionAsync(mission);
        public Task UnSubscribeMissionAsync(string mission) => _client.UnsubscribeMissionAsync(mission);

        private List<UidEntry> GetMissionCot(string data)
        {
            var missionData = JsonSerializer.Deserialize<MissionsRoot>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            return missionData?.Data[0].Uids.Where(u => u.Details.Type == "a-u-G").ToList() ?? new();
        }

        private List<Waypoint> GetWaypoints(string data)
        {
            var result = new List<Waypoint>();
            if (data == null) return result;

            using var doc  = JsonDocument.Parse(data);
            var uids = doc.RootElement.GetProperty("data")[0].GetProperty("uids");

            foreach (var uid in uids.EnumerateArray())
            {
                var type = uid.GetProperty("details").GetProperty("type").GetString();
                if (type == "b-m-p-s-m")
                    result.Add(Waypoint.Parse(uid));
            }
            return result;
        }

        public Waypoint? FindAnyCloserWP(List<Waypoint> wpt, ATAKPhoto photo, double threshold = 10.0)
        {
            return wpt.FirstOrDefault(x => GeoUtils.ArePointsClose(x, photo, threshold));
        }

        public Task<bool> JoinPhotoToWaypoint(string missionUid, Waypoint wpt, ATAKPhoto photo)
            => _client.AddFilesToCotAsync(wpt.Uid, photo.AttachmentUidList, missionUid);
    }
}
