using GamePortal.Data.Models;
using GamePortal.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Query;
using System.Net.Http;
using GamePortal.Components.Pages;
using GamePortal.Data.Services.BaseDataManagement;

namespace GamePortal.Data.Services
{
    public class RegistrationCodeService : IDataService<RegistrationCode>
    {
        private readonly ApplicationDbContext _context;

        public RegistrationCodeService(ApplicationDbContext context)
        {
            _context = context;
        }


        // Ottieni tutti i codici di registrazione
        public async Task<List<RegistrationCode>> GetAllAsync()
        {
            try
            {
                var registrationCodeList = await _context.RegistrationCodes
                .Include(r => r.Team).ToListAsync();

                return registrationCodeList;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore nella richiesta: {ex.Message}");
                return new List<RegistrationCode>();
            }
        }

        // Ottieni un singolo codice di registrazione per ID
        public async Task<RegistrationCode?> GetAsync(Guid id)
        {
            try
            {
                var registrationCode = await _context.RegistrationCodes
                .Include(r => r.Team)
                .FirstOrDefaultAsync(r => r.Id == id);

                if (registrationCode == null)
                {
                    return null;
                }

                return registrationCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore nella richiesta: {ex.Message}");
                return null;
            }
        }

        // Crea un nuovo codice di registrazione
        public async Task<bool> AddAsync(RegistrationCode registrationCode)
        {
            try
            {
                if (registrationCode == null)
                {
                    return false;
                }

                // Crea il nuovo GUID per il codice di registrazione
                registrationCode.Id = Guid.NewGuid();

                // Imposta la data di scadenza al fine giornata
                var date = registrationCode.ExpirationDate.Value;
                DateTime d = new DateTime(date.Year, date.Month, date.Day);
                d = d.AddDays(1);
                d = d.AddTicks(-1);
                registrationCode.ExpirationDate = d;


                // Aggiungi il codice di registrazione al contesto
                _context.RegistrationCodes.Add(registrationCode);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore nella richiesta: {ex.Message}");
                return false;
            }
        }

        // Aggiorna un codice di registrazione esistente
        public async Task<bool> UpdateAsync(RegistrationCode item)
        {
            try
            {
                if (item == null )
                {
                    return false;
                }

                var existingItem = await _context.RegistrationCodes.FindAsync(item.Id);
                if (existingItem == null)
                {
                    return false;
                }


                EntityUpdater.UpdateEntity(existingItem, item);
                

                await _context.SaveChangesAsync();

                return true;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore nella richiesta: {ex.Message}");
                return false;
            }
        }

        // Elimina un codice di registrazione
        public async Task<bool> DeleteAsync(Guid id)
        {
            try
            {
                var registrationCode = await _context.RegistrationCodes.FindAsync(id);

                if (registrationCode == null)
                {
                    return false;
                }

                // Rimuovi il codice di registrazione dal contesto
                _context.RegistrationCodes.Remove(registrationCode);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore nella richiesta: {ex.Message}");
                return false;
            }
        }

        public async Task<RegistrationCode?> GetValidCodeAsync(string code)
        {
            return await _context.RegistrationCodes.FirstOrDefaultAsync(c =>
                c.Id.ToString() == code && c.IsValid && (c.ExpirationDate == null || c.ExpirationDate > DateTime.UtcNow));
        }

        public async Task InvalidateCodeAsync(int id)
        {
            var code = await _context.RegistrationCodes.FindAsync(id);
            if (code != null)
            {
                code.IsValid = false;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> RegisterUserWithCode(ApplicationUser user, string? registrationCode)
        {
            if (string.IsNullOrEmpty(registrationCode)) return false;

            var codeEntry = await GetValidCodeAsync(registrationCode);
            if (codeEntry == null)
            {
                return false; // Codice non valido o scaduto
            }

            user.TeamId = codeEntry.TeamId;

            return true;
        }
    }
}

