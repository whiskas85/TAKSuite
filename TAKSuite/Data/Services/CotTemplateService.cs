using Microsoft.EntityFrameworkCore;
using TAKSuite.Data.Models;

namespace TAKSuite.Data.Services
{
    public class CotTemplateService(IDbContextFactory<ApplicationDbContext> factory)
    {
        public async Task<List<CotTemplate>> GetAllAsync()
        {
            using var db = factory.CreateDbContext();
            return await db.CotTemplates.AsNoTracking().OrderBy(t => t.Title).ToListAsync();
        }

        public async Task<CotTemplate?> GetAsync(Guid id)
        {
            using var db = factory.CreateDbContext();
            return await db.CotTemplates.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<CotTemplate> AddAsync(CotTemplate template)
        {
            template.CreatedAt = DateTime.UtcNow;
            template.UpdatedAt = DateTime.UtcNow;
            using var db = factory.CreateDbContext();
            db.CotTemplates.Add(template);
            await db.SaveChangesAsync();
            return template;
        }

        public async Task UpdateAsync(CotTemplate template)
        {
            template.UpdatedAt = DateTime.UtcNow;
            using var db = factory.CreateDbContext();
            db.CotTemplates.Update(template);
            await db.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            using var db = factory.CreateDbContext();
            var t = await db.CotTemplates.FindAsync(id);
            if (t != null)
            {
                db.CotTemplates.Remove(t);
                await db.SaveChangesAsync();
            }
        }
    }
}
