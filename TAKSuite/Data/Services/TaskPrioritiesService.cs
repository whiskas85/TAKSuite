using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using TAKSuite.Data.Models;

namespace TAKSuite.Data.Services
{
    public class EventEntityService : DataServiceAbstract<EventEntity>
    {
        public EventEntityService(IDbContextFactory<ApplicationDbContext> factory, IMemoryCache cache) : base(factory, ctx => ctx.EventEntities, cache)
        {
        }
    }
}
