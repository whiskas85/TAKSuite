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
        public MissionSuiteService(ApplicationDbContext context, IMemoryCache cache) : base(context.MissionsTakSuite, context, cache)
        {
            
        }
    }
}
