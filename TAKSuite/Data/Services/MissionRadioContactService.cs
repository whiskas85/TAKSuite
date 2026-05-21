using TAKSuite.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace TAKSuite.Data.Services
{
    public class MissionRadioContactService : DataServiceAbstract<MissionRadioContact>
    {
        public MissionRadioContactService(ApplicationDbContext context, IMemoryCache cache)
            : base(context.MissionRadioContacts, context, cache)
        {
            Includes = [_ => _.RadioChannel, _ => _.BackupRadioChannel!];
        }

        public async Task<List<MissionRadioContact>> GetByMissionAsync(Guid missionId)
        {
            return await _context.MissionRadioContacts
                .Include(_ => _.RadioChannel)
                .Include(_ => _.BackupRadioChannel)
                .Where(_ => _.MissionId == missionId)
                .OrderBy(_ => _.Name)
                .ToListAsync();
        }
    }
}
