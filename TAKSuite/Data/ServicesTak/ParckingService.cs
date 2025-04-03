using TAKSuite.Data.ModelsTak;
using TAKSuite.TAK.CoT;
using System.Text.Json;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace TAKSuite.Data.ServicesTak
{
    public class ParckingService
    {
        private readonly MartiApiClient _client;
        private readonly ApplicationDbContext _context;
        public ParckingService(ApplicationDbContext context, MartiApiClient client)
        {

            _client = client;
            _context = context;
        }
        public async Task<List<Parking>> GetAllAsync(String missionUid)
        {
            // le missioni devono essere tutte quelle che sono assegnate al team
            var mission = await _client.GetMissionDataAsync(missionUid); 
            return await Task.Run(() => GetParkings(mission));
        }


        protected List<Parking> GetParkings(string data)
        {
            List<Parking> uidMission = new();

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

                        JsonElement elem;
                        if (uid.GetProperty("details").TryGetProperty("iconsetPath", out elem))
                        {
                            var iconSet = elem.GetString();

                            // Se il tipo è b-m-p-s-m, aggiungi alla lista delle missioni di tipo b-m-p-s-m
                            if (type == "a-u-G" && iconSet.Contains("parking"))
                            {
                                uidMission.Add(Parking.Parse(uid));
                            }
                        }
                    }
                }
            }
            return uidMission;
        }
    }
}
