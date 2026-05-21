using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using TAKSuite.Data.Models;

namespace TAKSuite.Data.Services
{
    public class TeamService : DataServiceAbstract<Team>, IDataProvider
    {
        public TeamService(IDbContextFactory<ApplicationDbContext> factory, IMemoryCache cache) : base(factory, ctx => ctx.Teams, cache)
        {
            Includes = [_ => _.TeamLeader];
        }
        public Type ProvidedItem { get => typeof(Team); }

        public async Task<List<Team>> GetSubTeamsAsync(Team team, bool includeMyTeam = false)
        {
            try
            {
                using var ctx = _factory.CreateDbContext();
                var allSubTeams = await GetSubTeamsRecursiveAsync(ctx, team.Id);
                if (includeMyTeam)
                    allSubTeams.Insert(0, team);
                return allSubTeams;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore nella richiesta: {ex.Message}");
                return new List<Team>();
            }
        }

        private async Task<List<Team>> GetSubTeamsRecursiveAsync(ApplicationDbContext ctx, Guid teamId)
        {
            var subTeams = await ctx.Teams.Where(t => t.ParentTeamId == teamId).ToListAsync();
            var result = new List<Team>(subTeams);
            foreach (var subTeam in subTeams)
                result.AddRange(await GetSubTeamsRecursiveAsync(ctx, subTeam.Id));
            return result;
        }
    }
}
