using TAKSuite.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace TAKSuite.Data.Services
{
    public class PhoneContactService : DataServiceAbstract<PhoneContact>
    {
        public PhoneContactService(IDbContextFactory<ApplicationDbContext> factory, IMemoryCache cache)
            : base(factory, ctx => ctx.PhoneContacts, cache)
        {
        }
    }
}
