using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using TAKSuite.Data.Models;
using TAKSuite.Data.Services.BaseDataManagement;


namespace TAKSuite.Data.Services
{
    public class TaskPrioritiesService : DataServiceAbstract<TaskPriority>, IDataProvider
    {
        public TaskPrioritiesService(ApplicationDbContext context, IMemoryCache cache) : base(context.TaskPriorities, context, cache)
        {

        }

        public Type ProvidedItem => typeof(TaskPriority);

        public async Task<TaskPriority?> GetDefaultAsync()
        {
            return await DBSet.FirstOrDefaultAsync(p => p.IsDefault);
        }

        public async Task ClearDefaultsExceptAsync(Guid exceptId)
        {
            var others = await DBSet.Where(p => p.IsDefault && p.Id != exceptId).ToListAsync();
            foreach (var p in others)
                p.IsDefault = false;
            if (others.Count > 0)
            {
                await _context.SaveChangesAsync();
                _cache.Remove(nameof(TaskPriority));
            }
        }
    }
}
