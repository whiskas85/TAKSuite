using TAKSuite.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace TAKSuite.Data.Services
{
    public class MissionPhoneContactService : DataServiceAbstract<MissionPhoneContact>
    {
        public MissionPhoneContactService(ApplicationDbContext context, IMemoryCache cache)
            : base(context.MissionPhoneContacts, context, cache)
        {
            Includes = [_ => _.PhoneContact];
        }

        public async Task<List<MissionPhoneContact>> GetByMissionAsync(Guid missionId)
        {
            return await _context.MissionPhoneContacts
                .Include(_ => _.PhoneContact)
                .Where(_ => _.MissionId == missionId)
                .OrderBy(_ => _.Role)
                .ThenBy(_ => _.PhoneContact.Cognome)
                .ToListAsync();
        }
    }
}
