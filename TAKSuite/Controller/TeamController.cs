using TAKSuite.Data;
using TAKSuite.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TAKSuite.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TeamController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TeamController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Ottieni tutti i team
        [HttpGet]
        public async Task<IActionResult> GetTeams()
        {
            var teams = await _context.Teams.ToListAsync();
            return Ok(teams);
        }

        // Ottieni un team per ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTeamById(Guid id)
        {
            var team = await _context.Teams.FindAsync(id);

            if (team == null)
            {
                return NotFound();
            }

            return Ok(team);
        }

        // Crea un nuovo team
        [HttpPost]
        public async Task<IActionResult> CreateTeam([FromBody] Team team)
        {
            if (team == null)
            {
                return BadRequest("Team is null.");
            }

            // Aggiungi il team al contesto
            _context.Teams.Add(team);
            await _context.SaveChangesAsync();

            // Restituisci il team appena creato
            return CreatedAtAction(nameof(GetTeamById), new { id = team.Id }, team);
        }

        // Aggiorna un team esistente
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTeam(Guid id, [FromBody] Team team)
        {
            if (team == null || id != team.Id)
            {
                return BadRequest();
            }

            var existingTeam = await _context.Teams.FindAsync(id);
            if (existingTeam == null)
            {
                return NotFound();
            }

            // Aggiorna i dati del team
            existingTeam.Name = team.Name;
            existingTeam.Color = team.Color;
            await _context.SaveChangesAsync();

            return NoContent(); // 204 No Content, success without data returned
        }

        // Elimina un team
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTeam(Guid id)
        {
            var team = await _context.Teams.FindAsync(id);

            if (team == null)
            {
                return NotFound();
            }

            // Rimuovi il team dal contesto
            _context.Teams.Remove(team);
            await _context.SaveChangesAsync();

            return NoContent(); // 204 No Content
        }
    }
}
