using TAKSuite.Data.ModelsTak;
using TAKSuite.TAK.CoT;
using System.Text.Json;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using TAKSuite.TAK.Helper;
using System.Xml;
using System.Text.Json.Nodes;

namespace TAKSuite.Data.ServicesTak
{
    public class MedevacService
    {
        private readonly MartiApiClient _client;
        private readonly ApplicationDbContext _context;
        private readonly CoTManager _cotManager;
        public MedevacService(ApplicationDbContext context, MartiApiClient client, CoTManager manager)
        {

            _client = client;
            _context = context;
            _cotManager = manager;
        }
        public async Task<List<Medevac>> GetAllAsync(string missionUid)
        {
            // le missioni devono essere tutte quelle che sono assegnate al team
            var mission = await _client.GetMissionDataAsync(missionUid);
            return await Task.Run(() => GetMedevacs(mission));
        }


        protected async Task<List<Medevac>> GetMedevacs(string data)
        {
            List<Medevac> uidMission = new();


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

                        // Se il tipo è b-r-f-h-c: MEDEVAC
                        if (type == "b-r-f-h-c")
                        {
                            var item = Medevac.Parse(uid);
                            uidMission.Add(item);

                            var xmlString = await _client.GetInfoAsync(cotUid);
                            item.IntegrateInformationFromXml(xmlString);

                            var xmlString2 = await _client.GetInfoAsync(item.CreatorUid);
                            if (xmlString2 != null)
                            {
                                item.IntegrateUserInformationFromXml(xmlString2);
                            }

                            // 	

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
            //        JoinPhotoToMedevac("TEST FEED2", wp, _);
            //    }
            //});

            return uidMission;
        }


        public Medevac? FindAnyCloserWP(List<Medevac> wpt, ATAKPhoto photo, double threshold = 10.0)
        {
            var wpClosest = wpt.Where(x => GeoUtils.ArePointsClose(x, photo, threshold)).ToList();
            if (wpClosest!=null && wpClosest.Count()>0)
            {
                var closestWP = wpClosest.First();
                return closestWP;
            }
            return null;    
        }


        public async Task<bool> JoinPhotoToMedevac(string missionUid, Medevac wpt, ATAKPhoto photo)
        {
            var response = await _client.GetInfoAsync(wpt.Uid);

            if (response != null)
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(response);


                photo.AttachmentUidList.ToList().ForEach(x => AttachmentService.AddAttachment(doc, x));
                var res = await _cotManager.UpdateCoTMission(missionUid, wpt.Uid, doc);
                return res;
            }
            return false;
        }

    }
}
