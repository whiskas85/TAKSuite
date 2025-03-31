using Microsoft.EntityFrameworkCore;
using TAKSuite.Data.Models;

namespace TAKSuite.Data.Services
{
    public class TeamService : DataServiceAbstract<Team>
    {
        public TeamService(ApplicationDbContext context) : base(context.Teams, context)
        {
            Includes = [_ => _.TeamLeader];
        }

        public async Task<List<Team>> GetSubTeamsAsync(Team team)
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

                return allSubTeams;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore nella richiesta: {ex.Message}");
                return new List<Team>();
            }
        }

        //// Ottieni un singolo team per ID
        //public async Task<Team?> GetTeamByIdAsync(Guid id)
        //{
        //    try
        //    {
        //        return await _httpClient.GetFromJsonAsync<Team>($"api/team/{id}");
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Errore nella richiesta: {ex.Message}");
        //        return null;
        //    }
        //}

        //// Crea un nuovo team
        //public async Task<bool> CreateTeamAsync(Team team)
        //{
        //    try
        //    {
        //        var response = await _httpClient.PostAsJsonAsync("api/team", team);
        //        return response.IsSuccessStatusCode;
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Errore nella richiesta: {ex.Message}");
        //        return false;
        //    }
        //}

        //// Aggiorna un team esistente
        //public async Task<bool> UpdateTeamAsync(Team team)
        //{
        //    try
        //    {
        //        var response = await _httpClient.PutAsJsonAsync($"api/team/{team.Id}", team);
        //        return response.IsSuccessStatusCode;
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Errore nella richiesta: {ex.Message}");
        //        return false;
        //    }
        //}

        //// Elimina un team
        //public async Task<bool> DeleteTeamAsync(Guid id)
        //{
        //    try
        //    {
        //        var response = await _httpClient.DeleteAsync($"api/team/{id}");
        //        return response.IsSuccessStatusCode;
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Errore nella richiesta: {ex.Message}");
        //        return false;
        //    }
        //}
    }
}
