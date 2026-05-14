using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using TAKSuite.Data;
using TAKSuite.Data.Models;
using TAKSuite.Data.ModelsTak;
using TAKSuite.TAK.CoT;
using TAKSuite.TAK.Helper;

namespace TAKSuite.Data.ServicesTak
{
    public class PhotoAutoJoinService
    {
        private readonly CoTManager _cotManager;
        private readonly IServiceScopeFactory _scopeFactory;

        public PhotoAutoJoinService(CoTManager cotManager, IServiceScopeFactory scopeFactory)
        {
            _cotManager = cotManager;
            _cotManager.CoTMessageReceived += OnCotReceived;
            _scopeFactory = scopeFactory;
        }

        private void OnCotReceived(CoTManager.CoTMessageEventArgs e) => _ = ProcessAsync(e.Message);

        private async Task ProcessAsync(string cotXml)
        {
            try
            {
                var doc  = XDocument.Parse(cotXml);
                var root = doc.Root;
                if (root == null) return;

                var type = (string?)root.Attribute("type");
                if (type != "b-i-x-i") return;

                var uid = (string?)root.Attribute("uid");
                if (string.IsNullOrEmpty(uid)) return;

                var point = root.Element("point");
                if (point == null) return;
                if (!double.TryParse((string?)point.Attribute("lat"), System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out double lat)) return;
                if (!double.TryParse((string?)point.Attribute("lon"), System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out double lon)) return;

                var attachmentList = root.Descendants("attachment_list").FirstOrDefault();
                if (attachmentList == null) return;
                var hashes = ParseHashes((string?)attachmentList.Attribute("hashes") ?? "");
                if (!hashes.Any()) return;

                using var scope  = _scopeFactory.CreateScope();
                var context      = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var wpService    = scope.ServiceProvider.GetRequiredService<WaypointService>();
                var martiClient  = scope.ServiceProvider.GetRequiredService<MartiApiClient>();

                var configs = await context.MissionPhotoJoinConfigs
                    .Include(c => c.Mission)
                    .Include(c => c.PriorityRules)
                    .Where(c => c.Enabled && c.Mission.TakMissionUid != null)
                    .ToListAsync();

                foreach (var config in configs)
                    await ProcessMissionAsync(config, uid, lat, lon, hashes, wpService, martiClient);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PhotoAutoJoin] Errore: {ex.Message}");
            }
        }

        private async Task ProcessMissionAsync(
            MissionPhotoJoinConfig config,
            string photoUid,
            double lat, double lon,
            List<string> hashes,
            WaypointService wpService,
            MartiApiClient client)
        {
            var missionName = config.Mission.TakMissionUid!;

            var waypoints = await wpService.GetAllWaypointsAsync(missionName);

            var candidates = waypoints
                .Where(w => w.Uid != photoUid && w.Lat.HasValue && w.Lon.HasValue)
                .Select(w => (Wp: w, Dist: GeoUtils.GetDistance(lat, lon, w.Lat!.Value, w.Lon!.Value)))
                .Where(x => x.Dist <= config.RadiusMeters)
                .ToList();

            if (!candidates.Any()) return;

            if (SelectBestTarget(candidates, config.PriorityRules) is not { } target) return;

            Console.WriteLine($"[PhotoAutoJoin] {photoUid[..8]}… → '{target.Callsign}' " +
                $"(dist={GeoUtils.GetDistance(lat, lon, target.Lat!.Value, target.Lon!.Value):F0}m, missione='{missionName}')");

            await client.AddFilesToCotAsync(target.Uid, hashes, missionName);
            await client.DeleteCotAsync(photoUid, missionName);

            if (target.CotType == "b-m-p-s-m")
                await _cotManager.ChangeCotColor(missionName, target.Uid, ATAKHelper.COLOR_WAYPOINT_COMPLETED);
        }

        private static Waypoint? SelectBestTarget(
            List<(Waypoint Wp, double Dist)> candidates,
            List<CotPriorityRule> rules)
        {
            if (!rules.Any())
                return candidates.MinBy(x => x.Dist).Wp;

            var excluded = rules
                .SelectMany(r => r.ExcludedTypes)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var notExcluded = candidates
                .Where(x => !excluded.Any(e =>
                    x.Wp.CotType?.Contains(e, StringComparison.OrdinalIgnoreCase) == true))
                .ToList();

            foreach (var rule in rules.OrderBy(r => r.Priority))
            {
                var matches = notExcluded.Where(x =>
                {
                    if (!string.IsNullOrEmpty(rule.TypeContains) &&
                        x.Wp.CotType?.Contains(rule.TypeContains, StringComparison.OrdinalIgnoreCase) != true)
                        return false;
                    if (!string.IsNullOrEmpty(rule.CallSignContains) &&
                        x.Wp.Callsign?.Contains(rule.CallSignContains, StringComparison.OrdinalIgnoreCase) != true)
                        return false;
                    return true;
                }).ToList();

                if (matches.Any())
                    return matches.MinBy(x => x.Dist).Wp;
            }

            return notExcluded.Any() ? notExcluded.MinBy(x => x.Dist).Wp : null;
        }

        private static List<string> ParseHashes(string hashesAttr) =>
            hashesAttr.Trim('[', ']')
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(h => h.Trim('"'))
                .Where(h => !string.IsNullOrEmpty(h))
                .ToList();
    }
}
