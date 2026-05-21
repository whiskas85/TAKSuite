using Microsoft.EntityFrameworkCore;
using TAKSuite.Data;
using TAKSuite.Data.Models;

namespace TAKSuite.Data.Services
{
    public class TakSettingsService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _factory;

        public TakSettingsService(IDbContextFactory<ApplicationDbContext> factory) => _factory = factory;

        public async Task<TakSettings> GetOrCreateAsync()
        {
            using var db = _factory.CreateDbContext();
            var s = await db.TakSettings.FindAsync(1);
            if (s != null) return s;

            s = new TakSettings { Id = 1 };
            db.TakSettings.Add(s);
            await db.SaveChangesAsync();
            return s;
        }

        public async Task SaveAsync(TakSettings settings)
        {
            settings.Id = 1;
            settings.LastModified = DateTime.UtcNow;

            using var db = _factory.CreateDbContext();
            var existing = await db.TakSettings.FindAsync(1);
            if (existing == null)
            {
                db.TakSettings.Add(settings);
            }
            else
            {
                db.Entry(existing).CurrentValues.SetValues(settings);
                if (settings.CertificateP12 != null)
                    existing.CertificateP12 = settings.CertificateP12;
            }
            await db.SaveChangesAsync();
        }

        public async Task SaveCertificateAsync(byte[] p12Bytes)
        {
            using var db = _factory.CreateDbContext();
            var s = await db.TakSettings.FindAsync(1);
            if (s == null)
            {
                s = new TakSettings { Id = 1 };
                db.TakSettings.Add(s);
            }
            s.CertificateP12 = p12Bytes;
            s.LastModified = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }
    }
}
