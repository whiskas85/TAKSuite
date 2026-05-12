using Microsoft.EntityFrameworkCore;
using TAKSuite.Data;
using TAKSuite.Data.Models;

namespace TAKSuite.Data.Services
{
    public class TakSettingsService
    {
        private readonly ApplicationDbContext _db;

        public TakSettingsService(ApplicationDbContext db) => _db = db;

        public async Task<TakSettings> GetOrCreateAsync()
        {
            var s = await _db.TakSettings.FindAsync(1);
            if (s != null) return s;

            s = new TakSettings { Id = 1 };
            _db.TakSettings.Add(s);
            await _db.SaveChangesAsync();
            return s;
        }

        public async Task SaveAsync(TakSettings settings)
        {
            settings.Id = 1;
            settings.LastModified = DateTime.UtcNow;

            var existing = await _db.TakSettings.FindAsync(1);
            if (existing == null)
            {
                _db.TakSettings.Add(settings);
            }
            else
            {
                _db.Entry(existing).CurrentValues.SetValues(settings);
                // CertificateP12 non viene copiato da SetValues se è null (non sovrascrive)
                if (settings.CertificateP12 != null)
                    existing.CertificateP12 = settings.CertificateP12;
            }
            await _db.SaveChangesAsync();
        }

        /// <summary>Salva solo i bytes del certificato senza toccare il resto.</summary>
        public async Task SaveCertificateAsync(byte[] p12Bytes)
        {
            var s = await GetOrCreateAsync();
            s.CertificateP12 = p12Bytes;
            s.LastModified   = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
    }
}
