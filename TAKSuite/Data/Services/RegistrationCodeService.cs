using TAKSuite.Data.Models;
using TAKSuite.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Query;
using System.Net.Http;
using TAKSuite.Components.Pages;
using TAKSuite.Data.Services.BaseDataManagement;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;
using BlazorBootstrap;
using Microsoft.Extensions.Caching.Memory;

namespace TAKSuite.Data.Services
{
    public class RegistrationCodeService : DataServiceAbstract<RegistrationCode>
    {
        

        public RegistrationCodeService(ApplicationDbContext context, IMemoryCache cache) : base(context.RegistrationCodes, context, cache)
        {
           Includes = [c => c.Team];
        }

        public Task<RegistrationCode> CreateAsync()
        {
            return Task.Run(()=> new RegistrationCode());
        }
        public override Task<RegistrationCode> AddAsync(RegistrationCode element)
        {
            //Imposta la data di scadenza al fine giornata
            var date = element.ExpirationDate.Value;
            DateTime d = new DateTime(date.Year, date.Month, date.Day);
            d = d.AddDays(1);
            d = d.AddTicks(-1);
            element.ExpirationDate = d;

            return base.AddAsync(element);

        }
        public async Task<RegistrationCode?> GetRegistrationByTeamID(Guid teamId)
        {
            var list = await GetAllAsync();
            var elem = list.Where(_ => _.TeamId == teamId)
                .Where(_ => _.IsValid)
                .Where(_ => _.ExpirationDate == null || _.ExpirationDate > DateTime.UtcNow)
                .FirstOrDefault();
            return elem;
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

        public async Task<bool> RegisterUserWithCode(UserAtak user, string? registrationCode)
        {
            if (string.IsNullOrEmpty(registrationCode)) return false;

            if(user == null) return false;

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

