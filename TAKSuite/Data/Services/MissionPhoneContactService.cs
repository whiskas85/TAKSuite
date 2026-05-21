using TAKSuite.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace TAKSuite.Data.Services
{
    public class MissionPhoneContactService : DataServiceAbstract<MissionPhoneContact>
    {
        public MissionPhoneContactService(IDbContextFactory<ApplicationDbContext> factory, IMemoryCache cache)
            : base(factory, ctx => ctx.MissionPhoneContacts, cache)
        {
            Includes = [_ => _.PhoneContact];
        }

        public async Task<List<MissionPhoneContact>> GetByMissionAsync(Guid missionId)
        {
            using var ctx = _factory.CreateDbContext();
            return await ctx.MissionPhoneContacts
                .Include(_ => _.PhoneContact)
                .Where(_ => _.MissionId == missionId)
                .OrderBy(_ => _.Role)
                .ThenBy(_ => _.PhoneContact.Cognome)
                .ToListAsync();
        }
    }
}
