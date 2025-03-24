using GamePortal.Data;
using GamePortal.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GamePortal.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RegistrationCodesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RegistrationCodesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Ottieni tutti i codici di registrazione
        [HttpGet]
        public async Task<IActionResult> GetRegistrationCodes()
        {
            var registrationCodes = await _context.RegistrationCodes.Include(r => r.Team).ToListAsync();
            return Ok(registrationCodes);
        }

        // Ottieni un codice di registrazione per ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRegistrationCodeById(Guid id)
        {
            var registrationCode = await _context.RegistrationCodes
                .Include(r => r.Team)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (registrationCode == null)
            {
                return NotFound();
            }

            return Ok(registrationCode);
        }

        // Crea un nuovo codice di registrazione
        [HttpPost]
        public async Task<IActionResult> CreateRegistrationCode([FromBody] RegistrationCode registrationCode)
        {
            if (registrationCode == null)
            {
                return BadRequest("RegistrationCode is null.");
            }
            registrationCode.Id = Guid.NewGuid();

            // Aggiungi il codice di registrazione al contesto
            _context.RegistrationCodes.Add(registrationCode);
            await _context.SaveChangesAsync();

            // Restituisci il codice di registrazione appena creato
            return CreatedAtAction(nameof(GetRegistrationCodeById), new { id = registrationCode.Id }, registrationCode);
        }

        // Aggiorna un codice di registrazione esistente
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRegistrationCode(Guid id, [FromBody] RegistrationCode registrationCode)
        {
            if (registrationCode == null || id != registrationCode.Id)
            {
                return BadRequest();
            }

            var existingCode = await _context.RegistrationCodes.FindAsync(id);
            if (existingCode == null)
            {
                return NotFound();
            }

            // Aggiorna i dati del codice di registrazione
            existingCode.Code = registrationCode.Code;
            existingCode.TeamId = registrationCode.TeamId;
            existingCode.ExpirationDate = registrationCode.ExpirationDate;
            existingCode.IsValid = registrationCode.IsValid;

            await _context.SaveChangesAsync();

            return NoContent(); // 204 No Content, success without data returned
        }

        // Elimina un codice di registrazione
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRegistrationCode(int id)
        {
            var registrationCode = await _context.RegistrationCodes.FindAsync(id);

            if (registrationCode == null)
            {
                return NotFound();
            }

            // Rimuovi il codice di registrazione dal contesto
            _context.RegistrationCodes.Remove(registrationCode);
            await _context.SaveChangesAsync();

            return NoContent(); // 204 No Content
        }
    }
}
