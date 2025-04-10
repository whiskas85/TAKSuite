using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using TAKSuite.Data.Models;


namespace TAKSuite.Data.Services
{
    public class EventEntityService : DataServiceAbstract<EventEntity>
    {
        public EventEntityService(ApplicationDbContext context, IMemoryCache cache) : base(context.EventEntities, context, cache)
        {
           
        }
    }
}
