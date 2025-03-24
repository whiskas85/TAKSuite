using TAKSuite.Data.Models;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace TAKSuite.Data.Services
{
    public class TeamRadioChannelService : DataServiceAbstract<TeamRadioChannel>
    {
        public TeamRadioChannelService(ApplicationDbContext context) : base(context.TeamRadioChannels, context)
        {
            Includes = [_ => _.RadioChannel, _ => _.BackupRadioChannel];
        }

        // Ottieni tutti i team
        public async Task<List<TeamRadioChannel>> GetAllByTeamAsync(Guid teamId)
        {
            try
            {
                return await _context.TeamRadioChannels.Where(_ => _.TeamId == teamId).ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore nella richiesta: {ex.Message}");
                return new List<TeamRadioChannel>();
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
