using TAKSuite.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace TAKSuite.Data.Services
{
    public class TeamRadioChannelService : DataServiceAbstract<TeamRadioChannel>
    {
        public TeamRadioChannelService(IDbContextFactory<ApplicationDbContext> factory, IMemoryCache cache) : base(factory, ctx => ctx.TeamRadioChannels, cache)
        {
            Includes = [_ => _.RadioChannel, _ => _.BackupRadioChannel];
        }

        public async Task<List<TeamRadioChannel>> GetAllByTeamAsync(Guid teamId)
        {
            try
            {
                using var ctx = _factory.CreateDbContext();
                return await ctx.TeamRadioChannels.Where(_ => _.TeamId == teamId).ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore nella richiesta: {ex.Message}");
                return new List<TeamRadioChannel>();
            }
        }
    }
}
