using TAKSuite.Data.Models;
using Microsoft.Extensions.Caching.Memory;

namespace TAKSuite.Data.Services
{
    public class PhoneContactService : DataServiceAbstract<PhoneContact>
    {
        public PhoneContactService(ApplicationDbContext context, IMemoryCache cache)
            : base(context.PhoneContacts, context, cache)
        {
        }
    }
}
