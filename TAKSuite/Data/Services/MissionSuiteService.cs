using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Org.BouncyCastle.Tsp;
using System.Threading.Tasks;
using TAKSuite.Components.Pages;
using TAKSuite.Data.Models;

namespace TAKSuite.Data.Services
{
    public class MissionSuiteService : DataServiceAbstract<MissionSuite>
    {
        public MissionSuiteService(IDbContextFactory<ApplicationDbContext> factory, IMemoryCache cache) : base(factory, ctx => ctx.MissionsTakSuite, cache)
        {
            
        }
    }
}
