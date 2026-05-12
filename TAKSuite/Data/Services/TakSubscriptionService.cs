using Microsoft.EntityFrameworkCore;
using TAKSuite.Data;
using TAKSuite.Data.Models;

namespace TAKSuite.Data.Services
{
    public class TakSubscriptionService
    {
        private readonly ApplicationDbContext _db;

        public TakSubscriptionService(ApplicationDbContext db) => _db = db;

        public Task<List<TakSubscription>> GetAllAsync() =>
            _db.TakSubscriptions.OrderBy(s => s.MissionName).ToListAsync();

        public async Task<bool> IsSubscribedAsync(string missionName) =>
            await _db.TakSubscriptions.AnyAsync(s => s.MissionName == missionName);

        public async Task AddAsync(string missionName)
        {
            if (await IsSubscribedAsync(missionName)) return;
            _db.TakSubscriptions.Add(new TakSubscription { MissionName = missionName });
            await _db.SaveChangesAsync();
        }

        public async Task RemoveAsync(string missionName)
        {
            var sub = await _db.TakSubscriptions.FirstOrDefaultAsync(s => s.MissionName == missionName);
            if (sub == null) return;
            _db.TakSubscriptions.Remove(sub);
            await _db.SaveChangesAsync();
        }
    }
}
