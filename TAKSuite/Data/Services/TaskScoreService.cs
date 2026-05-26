using Microsoft.EntityFrameworkCore;
using TAKSuite.Data.Models;

namespace TAKSuite.Data.Services
{
    public class TaskScoreService(IDbContextFactory<ApplicationDbContext> factory)
    {
        public async Task<List<TaskScoreEntry>> GetByTaskAsync(Guid taskId)
        {
            using var ctx = factory.CreateDbContext();
            return await ctx.TaskScoreEntries
                .Where(e => e.TaskEntityId == taskId)
                .OrderBy(e => e.CreatedAt)
                .ToListAsync();
        }

        public async Task<TaskScoreEntry?> GetAsync(Guid id)
        {
            using var ctx = factory.CreateDbContext();
            return await ctx.TaskScoreEntries.FindAsync(id);
        }

        public async Task<TaskScoreEntry> AddAsync(TaskScoreEntry entry)
        {
            entry.Id = Guid.NewGuid();
            entry.CreatedAt = DateTime.UtcNow;
            entry.LastModified = DateTime.UtcNow;
            using var ctx = factory.CreateDbContext();
            ctx.TaskScoreEntries.Add(entry);
            await ctx.SaveChangesAsync();
            return entry;
        }

        public async Task UpdateAsync(TaskScoreEntry entry)
        {
            entry.LastModified = DateTime.UtcNow;
            using var ctx = factory.CreateDbContext();
            ctx.TaskScoreEntries.Update(entry);
            await ctx.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            using var ctx = factory.CreateDbContext();
            var e = await ctx.TaskScoreEntries.FindAsync(id);
            if (e == null) return false;
            ctx.TaskScoreEntries.Remove(e);
            await ctx.SaveChangesAsync();
            return true;
        }
    }
}
