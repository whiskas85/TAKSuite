using TAKSuite.Data.Models;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace TAKSuite.Data.Services
{
    public class RadioChannelService : DataServiceAbstract<RadioChannel>
    {

        public RadioChannelService(ApplicationDbContext context, IMemoryCache cache) : base(context.RadioChannels, context, cache)
        {

        }

        //// Ottieni tutti i team
        //public async Task<List<Team>> GetTeamsAsync()
        //{
        //    try
        //    {
        //        var teams = await _context.Teams.ToListAsync();



        //        var teams = await _httpClient.GetFromJsonAsync<List<Team>>("api/team");
        //        return teams ?? new List<Team>();
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Errore nella richiesta: {ex.Message}");
        //        return new List<Team>();
        //    }
        //}

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
