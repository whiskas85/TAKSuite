using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using TAKSuite.Data.Models;

namespace TAKSuite.Data.Services
{
    public class DocumentationService : DataServiceAbstract<Documentation>
    {
        private long maxFileSize = 1024 * 1024 * 200;   // un file puo essere di 200MB


        private readonly ApplicationDbContext _context;
        public DocumentationService(ApplicationDbContext contex, IMemoryCache cache) : base(contex.Documents, contex, cache)
        {
            _context = contex;
            Includes = [_ => _.DocumentationOwners];
        }

        public override Task<Documentation> AddAsync(Documentation element)
        {
            throw new NotSupportedException("Use AddDocumentationAsync instead");
        }
        public async Task<Documentation?> AddDocumentationAsync(IBrowserFile file)
        {

            // creo l'header della documentazione
            Documentation documentation = new Documentation
            {
                Id = Guid.NewGuid(),  // Assegna un nuovo ID
            };


            var trustedFileName = documentation.Id.ToString();
            var basePath = Path.Combine("wwwroot", "documentation");
            var path = Path.Combine("wwwroot", "documentation", trustedFileName);

            // se non c'è la directory la crea
            if (!Directory.Exists(basePath))
                Directory.CreateDirectory(basePath);

            try
            {

                await using FileStream fs = new(path, FileMode.Create);
                await file.OpenReadStream(maxFileSize).CopyToAsync(fs);

                documentation.Path = path;
                documentation.Name = file.Name;
                documentation.Type = file.ContentType;

                _context.Documents.Add(documentation);
                _context.SaveChanges();

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
            var doc = await _context.Documents.FindAsync(id);

            if (doc == null)
                return false;

            // Rimuove tutte le associazioni nella tabella di giunzione
            var linkedOwners = _context.DocumentationOwners.Where(_ => _.DocumentationId == id);
            _context.DocumentationOwners.RemoveRange(linkedOwners);

            var res = await base.DeleteAsync(id); // Cancella il documento dal DB

            if (res)
            {
                try
                {
                    File.Delete(doc.Path);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }

            return res;
        }
        public async Task<bool> LinkDocumentAsync(Documentation doc, IGuidModel owner)
        {
            return await LinkDocumentAsync(doc.Id, owner);
        }
        public async Task<bool> LinkDocumentAsync(Guid docId, IGuidModel owner)
        {
            try
            {
                // recupero il documento
                var doc = await GetAsync(docId);
                if (doc == null) return false;

                // creo l'associazione al docuento
                DocumentationOwner docOwner = new();
                docOwner.DocumentationId = doc.Id;
                docOwner.OwnerId = owner.Id;
                docOwner.OwnerType = owner.GetType().Name;
                docOwner.AddedDate = DateTime.Now;


                // salvo l'associazione
                _context.DocumentationOwners.Add(docOwner);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }

        }


        public async Task<bool> UnLinkDocumentAsync(Documentation doc, IGuidModel owner)
        {
            return await UnLinkDocumentAsync(doc.Id, owner);
        }
        public async Task<bool> UnLinkDocumentAsync(Guid docId, IGuidModel owner)
        {
            try
            {
                var ownerType = owner.GetType().Name; // Recupera il tipo di owner come stringa

                var association = await _context.DocumentationOwners
                    .FirstOrDefaultAsync(d => d.DocumentationId == docId && d.OwnerId == owner.Id && d.OwnerType == ownerType);

                if (association == null)
                {
                    return false; // Nessuna associazione trovata
                }

                _context.DocumentationOwners.Remove(association);
                await _context.SaveChangesAsync();

                return true; // Associazione rimossa con successo
            }
            catch
            {
                return false;
            }
        }
        public async Task<List<Documentation>> GetAllByOwnerIdAsync(IGuidModel owner)
        {
            var ownerType = owner.GetType().Name; // Ottiene il tipo dell'owner come stringa

            var documentIds = await _context.DocumentationOwners
                .Where(d => d.OwnerId == owner.Id && d.OwnerType == ownerType)
                .Select(d => d.DocumentationId)
                .ToListAsync(); // Ottiene tutti gli ID dei documenti associati

            var documents = await _context.Documents
                .Where(d => documentIds.Contains(d.Id))
                .ToListAsync(); // Recupera i documenti dal database

            return documents;
        }

        public async Task<List<Documentation>> GetAllDocNotLinkedToOwnerIdAsync(IGuidModel owner)
        {
            var ownerId = owner.Id;

            var documents = await _context.Documents
                .Where(doc =>
                    !_context.DocumentationOwners.Any(_ => _.DocumentationId == doc.Id) ||  // Nessuna associazione
                    !_context.DocumentationOwners.Any(_ => _.DocumentationId == doc.Id && _.OwnerId == ownerId) // Associazione esiste ma non per questo owner
                )
                .ToListAsync();

            return documents;
        }

    }
}
