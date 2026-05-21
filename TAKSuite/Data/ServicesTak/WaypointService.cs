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
            // le missioni devono essere tutte quelle che sono assegnate al team
            var mission = await _client.GetMissionDataAsync(missionUid);
            return await Task.Run(() => GetWaypoints(mission));
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
                        var uidStr = uid.GetProperty("data").GetString() ?? string.Empty;
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
            // le missioni devono essere tutte quelle che sono assegnate al team
            var mission = await _client.GetMissionDataAsync(missionUid);
            return await Task.Run(() => GetMissionCot(mission));
        }

        public async Task SubscribeMissionAsync(string mission)
        {
            await _client.SubscribeMissionAsync(mission);
        }
        public async Task UnSubscribeMissionAsync(string mission)
        {
            await _client.UnsubscribeMissionAsync(mission);
        }





        private async Task<List<UidEntry>> GetMissionCot(string data)
        {
            var missionData = JsonSerializer.Deserialize<MissionsRoot>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            var uids = missionData.Data[0].Uids.Where(_ => _.Details.Type == "a-u-G").ToList();
            return uids;
            return new();
        }





        private async Task<List<Waypoint>> GetWaypoints(string data)
        {
            List<Waypoint> uidMission = new();


            // da cancellare
            //List<ATAKPhoto> uidarrach = new();

            if (data != null)
            {
                // Parso i dati JSON in un oggetto JsonDocument
                using (var doc = JsonDocument.Parse(data))
                {
                    var missionData = doc.RootElement
                                  .GetProperty("data")[0];

                    var uids = doc.RootElement
                                  .GetProperty("data")[0]
                                  .GetProperty("uids");


                    // Estrazione delle informazioni per ogni UID
                    foreach (var uid in uids.EnumerateArray())
                    {
                        var callsign = uid.GetProperty("details").GetProperty("callsign").GetString();
                        var type = uid.GetProperty("details").GetProperty("type").GetString();
                        var cotUid = uid.GetProperty("data").GetString();  // Aggiungi l'UID

                        // Se il tipo è b-m-p-s-m, aggiungi alla lista delle missioni di tipo b-m-p-s-m
                        if (type == "b-m-p-s-m")
                        {
                            var wp = Waypoint.Parse(uid);
                            uidMission.Add(wp);

                            //var point = _client.GetInfoAsync(cotUid);
                            //var ptString = point.Result.ToString();

                            //if (ptString.Contains("attach"))
                            //{
                            //    wp.ToString();
                            //    data.ToString();
                            //    ptString.ToString();
                            //}

                        }



                        //// da cancellare
                        //// Se il tipo è photo
                        //if (type == "b-i-x-i")
                        //{
                        //    var photo = ATAKPhoto.Parse(uid);

                        //    uidarrach.Add(photo);
                        //}
                        
                    }
                }
            }



            //// da cancellare
            //uidarrach.ForEach(_ =>
            //{
            //    var wp = FindAnyCloserWP(uidMission, _);
            //    if (wp != null)
            //    {
            //        JoinPhotoToWaypoint("TEST FEED2", wp, _);
            //    }
            //});

            return uidMission;
        }


        public Waypoint? FindAnyCloserWP(List<Waypoint> wpt, ATAKPhoto photo, double threshold = 10.0)
        {
            var wpClosest = wpt.Where(x => GeoUtils.ArePointsClose(x, photo, threshold)).ToList();
            if (wpClosest!=null && wpClosest.Count()>0)
            {
                var closestWP = wpClosest.First();
                return closestWP;
            }
            return null;    
        }


        public Task<bool> JoinPhotoToWaypoint(string missionUid, Waypoint wpt, ATAKPhoto photo)
            => _client.AddFilesToCotAsync(wpt.Uid, photo.AttachmentUidList, missionUid);

    }
}
