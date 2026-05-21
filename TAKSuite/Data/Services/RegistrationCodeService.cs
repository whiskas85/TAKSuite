using TAKSuite.Data.Models;
using TAKSuite.Data;
using Microsoft.EntityFrameworkCore;
using TAKSuite.Data.Services.BaseDataManagement;
using Microsoft.Extensions.Caching.Memory;

namespace TAKSuite.Data.Services
{
    public class RegistrationCodeService : DataServiceAbstract<RegistrationCode>
    {
        public RegistrationCodeService(IDbContextFactory<ApplicationDbContext> factory, IMemoryCache cache) : base(factory, ctx => ctx.RegistrationCodes, cache)
        {
            Includes = [c => c.Team];
        }

        public Task<RegistrationCode> CreateAsync() => Task.Run(() => new RegistrationCode());

        public override Task<RegistrationCode> AddAsync(RegistrationCode element)
        {
            var date = element.ExpirationDate.Value;
            DateTime d = new DateTime(date.Year, date.Month, date.Day);
            d = d.AddDays(1).AddTicks(-1);
            element.ExpirationDate = d;
            return base.AddAsync(element);
        }

        public async Task<RegistrationCode?> GetRegistrationByTeamID(Guid teamId)
        {
            var list = await GetAllAsync();
            return list.Where(_ => _.TeamId == teamId)
                .Where(_ => _.IsValid)
                .Where(_ => _.ExpirationDate == null || _.ExpirationDate > DateTime.UtcNow)
                .FirstOrDefault();
        }

        public async Task<RegistrationCode?> GetValidCodeAsync(string code)
        {
            using var ctx = _factory.CreateDbContext();
            return await ctx.RegistrationCodes.FirstOrDefaultAsync(c =>
                c.Id.ToString() == code && c.IsValid && (c.ExpirationDate == null || c.ExpirationDate > DateTime.UtcNow));
        }

        public async Task InvalidateCodeAsync(int id)
        {
            using var ctx = _factory.CreateDbContext();
            var code = await ctx.RegistrationCodes.FindAsync(id);
            if (code != null)
            {
                code.IsValid = false;
                await ctx.SaveChangesAsync();
            }
        }

        public async Task<bool> RegisterUserWithCode(UserAtak user, string? registrationCode)
        {
            if (string.IsNullOrEmpty(registrationCode) || user == null) return false;
            var codeEntry = await GetValidCodeAsync(registrationCode);
            if (codeEntry == null) return false;
            user.TeamId = codeEntry.TeamId;
            return true;
        }
    }
}
