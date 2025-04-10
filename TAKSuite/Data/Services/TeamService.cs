using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using TAKSuite.Data.Models;

namespace TAKSuite.Data.Services
{
    public class TeamService : DataServiceAbstract<Team>
    {
        public TeamService(ApplicationDbContext context, IMemoryCache cache) : base(context.Teams, context, cache)
        {
            Includes = [_ => _.TeamLeader];
        }

        public async Task<List<Team>> GetSubTeamsAsync(Team team, bool includeMyTeam = false)
        {
            try
            {
                var subTeams = await _context.Teams
                    .Where(t => t.ParentTeamId == team.Id)
                    .ToListAsync();

                // Lista totale di tutti i sotto-team (inclusi quelli annidati)
                List<Team> allSubTeams = new(subTeams);

                // Per ogni sotto-team, recupera ricorsivamente i suoi sotto-team
                foreach (var subTeam in subTeams)
                {
                    allSubTeams.AddRange(await GetSubTeamsAsync(subTeam));
                }

                if (includeMyTeam)
                {
                    // Aggiungi il team corrente alla lista se includeMyTeam è true
                    allSubTeams.Insert(0,team);
                }

                return allSubTeams;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore nella richiesta: {ex.Message}");
                return new List<Team>();
            }
        }
    }
}
