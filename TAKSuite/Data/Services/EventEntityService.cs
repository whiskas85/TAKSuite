using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using TAKSuite.Data.Models;
using TAKSuite.Data.Services.BaseDataManagement;

namespace TAKSuite.Data.Services
{
    public class TaskPrioritiesService : DataServiceAbstract<TaskPriority>, IDataProvider
    {
        public TaskPrioritiesService(IDbContextFactory<ApplicationDbContext> factory, IMemoryCache cache) : base(factory, ctx => ctx.TaskPriorities, cache)
        {
        }

        public Type ProvidedItem => typeof(TaskPriority);

        public async Task<TaskPriority?> GetDefaultAsync()
        {
            using var ctx = _factory.CreateDbContext();
            return await ctx.TaskPriorities.FirstOrDefaultAsync(p => p.IsDefault);
        }

        public async Task ClearDefaultsExceptAsync(Guid exceptId)
        {
            using var ctx = _factory.CreateDbContext();
            var others = await ctx.TaskPriorities.Where(p => p.IsDefault && p.Id != exceptId).ToListAsync();
            foreach (var p in others)
                p.IsDefault = false;
            if (others.Count > 0)
            {
                await ctx.SaveChangesAsync();
                _cache.Remove(nameof(TaskPriority));
            }
        }
    }
}
