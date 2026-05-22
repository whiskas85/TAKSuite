using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using TAKSuite.Data.Models;

namespace TAKSuite.Data.Services
{
    public class DocumentTypeService : DataServiceAbstract<DocumentType>
    {
        public DocumentTypeService(IDbContextFactory<ApplicationDbContext> factory, IMemoryCache cache)
            : base(factory, ctx => ctx.DocumentTypes, cache) { }

        public async Task<List<DocumentType>> GetByTargetAsync(DocumentTypeTarget target)
        {
            using var ctx = _factory.CreateDbContext();
            return await ctx.DocumentTypes
                .Where(t => t.Target == target)
                .OrderBy(t => t.Order)
                .ThenBy(t => t.Description)
                .ToListAsync();
        }

        public async Task ReorderAsync(List<Guid> orderedIds)
        {
            using var ctx = _factory.CreateDbContext();
            var types = await ctx.DocumentTypes.Where(t => orderedIds.Contains(t.Id)).ToListAsync();
            for (int i = 0; i < orderedIds.Count; i++)
            {
                var t = types.FirstOrDefault(x => x.Id == orderedIds[i]);
                if (t != null) t.Order = i;
            }
            await ctx.SaveChangesAsync();
            _cache.Remove(_cacheKey);
        }
    }
}
