using TAKSuite.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace TAKSuite.Data.Services
{
    public class MissionRadioContactService : DataServiceAbstract<MissionRadioContact>
    {
        public MissionRadioContactService(IDbContextFactory<ApplicationDbContext> factory, IMemoryCache cache)
            : base(factory, ctx => ctx.MissionRadioContacts, cache)
        {
            Includes = [_ => _.RadioChannel, _ => _.BackupRadioChannel!];
        }

        public async Task<List<MissionRadioContact>> GetByMissionAsync(Guid missionId)
        {
            using var ctx = _factory.CreateDbContext();
            return await ctx.MissionRadioContacts
                .Include(_ => _.RadioChannel)
                .Include(_ => _.BackupRadioChannel)
                .Where(_ => _.MissionId == missionId)
                .OrderBy(_ => _.Name)
                .ToListAsync();
        }
    }
}
