using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using TAKSuite.Data.Models;

namespace TAKSuite.Data.Services
{
    public class DocumentationService : DataServiceAbstract<Documentation>
    {
        private long maxFileSize = 1024 * 1024 * 200;

        public DocumentationService(IDbContextFactory<ApplicationDbContext> factory, IMemoryCache cache) : base(factory, ctx => ctx.Documents, cache)
        {
            Includes = [_ => _.DocumentationOwners];
        }

        public override Task<Documentation> AddAsync(Documentation element)
        {
            throw new NotSupportedException("Use AddDocumentationAsync instead");
        }

        public async Task<Documentation?> AddDocumentationAsync(IBrowserFile file)
        {
            Documentation documentation = new Documentation { Id = Guid.NewGuid() };
            var trustedFileName = documentation.Id.ToString();
            var basePath = Path.Combine("wwwroot", "documentation");
            var path = Path.Combine("wwwroot", "documentation", trustedFileName);

            if (!Directory.Exists(basePath))
                Directory.CreateDirectory(basePath);

            try
            {
                await using FileStream fs = new(path, FileMode.Create);
                await file.OpenReadStream(maxFileSize).CopyToAsync(fs);

                documentation.Path = path;
                documentation.Name = file.Name;
                documentation.Type = file.ContentType;

                using var ctx = _factory.CreateDbContext();
                ctx.Documents.Add(documentation);
                ctx.SaveChanges();
                return documentation;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        public override async Task<bool> DeleteAsync(Guid id)
        {
            using var ctx = _factory.CreateDbContext();
            var doc = await ctx.Documents.FindAsync(id);
            if (doc == null) return false;

            var linkedOwners = ctx.DocumentationOwners.Where(_ => _.DocumentationId == id);
            ctx.DocumentationOwners.RemoveRange(linkedOwners);
            ctx.Documents.Remove(doc);

            try { await ctx.SaveChangesAsync(); }
            catch (Exception e) { Console.WriteLine(e.Message); return false; }

            try { if (File.Exists(doc.Path)) File.Delete(doc.Path); }
            catch (Exception e) { Console.WriteLine(e.Message); }

            return true;
        }

        public async Task<bool> LinkDocumentAsync(Documentation doc, IGuidModel owner) =>
            await LinkDocumentAsync(doc.Id, owner);

        public async Task<bool> LinkDocumentAsync(Guid docId, IGuidModel owner)
        {
            try
            {
                var doc = await GetAsync(docId);
                if (doc == null) return false;

                var docOwner = new DocumentationOwner
                {
                    DocumentationId = doc.Id,
                    OwnerId = owner.Id,
                    OwnerType = owner.GetType().Name,
                    AddedDate = DateTime.Now
                };

                using var ctx = _factory.CreateDbContext();
                ctx.DocumentationOwners.Add(docOwner);
                await ctx.SaveChangesAsync();
                return true;
            }
            catch { return false; }
        }

        public async Task<bool> UnLinkDocumentAsync(Documentation doc, IGuidModel owner) =>
            await UnLinkDocumentAsync(doc.Id, owner);

        public async Task<bool> UnLinkDocumentAsync(Guid docId, IGuidModel owner)
        {
            try
            {
                var ownerType = owner.GetType().Name;
                using var ctx = _factory.CreateDbContext();
                var association = await ctx.DocumentationOwners
                    .FirstOrDefaultAsync(d => d.DocumentationId == docId && d.OwnerId == owner.Id && d.OwnerType == ownerType);

                if (association == null) return false;

                ctx.DocumentationOwners.Remove(association);
                await ctx.SaveChangesAsync();
                return true;
            }
            catch { return false; }
        }

        public async Task<List<Documentation>> GetAllByOwnerIdAsync(IGuidModel owner)
        {
            var ownerType = owner.GetType().Name;
            using var ctx = _factory.CreateDbContext();
            var documentIds = await ctx.DocumentationOwners
                .Where(d => d.OwnerId == owner.Id && d.OwnerType == ownerType)
                .Select(d => d.DocumentationId)
                .ToListAsync();

            return await ctx.Documents
                .Where(d => documentIds.Contains(d.Id))
                .ToListAsync();
        }

        public async Task<List<Documentation>> GetAllDocNotLinkedToOwnerIdAsync(IGuidModel owner)
        {
            var ownerId = owner.Id;
            using var ctx = _factory.CreateDbContext();
            return await ctx.Documents
                .Where(doc =>
                    !ctx.DocumentationOwners.Any(_ => _.DocumentationId == doc.Id) ||
                    !ctx.DocumentationOwners.Any(_ => _.DocumentationId == doc.Id && _.OwnerId == ownerId))
                .ToListAsync();
        }
    }
}
