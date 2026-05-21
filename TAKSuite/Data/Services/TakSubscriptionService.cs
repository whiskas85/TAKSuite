using Microsoft.EntityFrameworkCore;
using TAKSuite.Data;
using TAKSuite.Data.Models;

namespace TAKSuite.Data.Services
{
    public class TakSubscriptionService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _factory;

        public TakSubscriptionService(IDbContextFactory<ApplicationDbContext> factory) => _factory = factory;

        public async Task<List<TakSubscription>> GetAllAsync()
        {
            using var db = _factory.CreateDbContext();
            return await db.TakSubscriptions.OrderBy(s => s.MissionName).ToListAsync();
        }

        public async Task<bool> IsSubscribedAsync(string missionName)
        {
            using var db = _factory.CreateDbContext();
            return await db.TakSubscriptions.AnyAsync(s => s.MissionName == missionName);
        }

        public async Task AddAsync(string missionName)
        {
            using var db = _factory.CreateDbContext();
            if (await db.TakSubscriptions.AnyAsync(s => s.MissionName == missionName)) return;
            db.TakSubscriptions.Add(new TakSubscription { MissionName = missionName });
            await db.SaveChangesAsync();
        }

        public async Task RemoveAsync(string missionName)
        {
            using var db = _factory.CreateDbContext();
            var sub = await db.TakSubscriptions.FirstOrDefaultAsync(s => s.MissionName == missionName);
            if (sub == null) return;
            db.TakSubscriptions.Remove(sub);
            await db.SaveChangesAsync();
        }
    }
}
