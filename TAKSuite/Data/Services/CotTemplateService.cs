using Microsoft.EntityFrameworkCore;
using TAKSuite.Data.Models;

namespace TAKSuite.Data.Services
{
    public class CotTemplateService(ApplicationDbContext db)
    {
        public async Task<List<CotTemplate>> GetAllAsync()
            => await db.CotTemplates.AsNoTracking().OrderBy(t => t.Title).ToListAsync();

        public async Task<CotTemplate?> GetAsync(Guid id)
            => await db.CotTemplates.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);

        public async Task<CotTemplate> AddAsync(CotTemplate template)
        {
            template.CreatedAt = DateTime.UtcNow;
            template.UpdatedAt = DateTime.UtcNow;
            db.CotTemplates.Add(template);
            await db.SaveChangesAsync();
            return template;
        }

        public async Task UpdateAsync(CotTemplate template)
        {
            template.UpdatedAt = DateTime.UtcNow;
            db.CotTemplates.Update(template);
            await db.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var t = await db.CotTemplates.FindAsync(id);
            if (t != null)
            {
                db.CotTemplates.Remove(t);
                await db.SaveChangesAsync();
            }
        }
    }
}
