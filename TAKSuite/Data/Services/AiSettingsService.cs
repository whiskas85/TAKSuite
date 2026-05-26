using Microsoft.EntityFrameworkCore;
using TAKSuite.Data.Models;

namespace TAKSuite.Data.Services
{
    public class AiSettingsService(IDbContextFactory<ApplicationDbContext> factory)
    {
        public async Task<AiSettings> GetOrCreateAsync()
        {
            using var ctx = factory.CreateDbContext();
            var s = await ctx.AiSettings.FindAsync(1);
            if (s != null) return s;

            s = new AiSettings { Id = 1 };
            ctx.AiSettings.Add(s);
            await ctx.SaveChangesAsync();
            return s;
        }

        public async Task SaveAsync(AiSettings settings)
        {
            using var ctx = factory.CreateDbContext();
            settings.Id = 1;
            ctx.AiSettings.Update(settings);
            await ctx.SaveChangesAsync();
        }
    }
}
