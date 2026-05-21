using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using TAKSuite.Data.Models;
using TAKSuite.Data.Services.BaseDataManagement;


namespace TAKSuite.Data.Services
{
    public class MissionSuiteEntityServices : DataServiceAbstract<MissionSuite>, IDataProvider
    {
        public MissionSuiteEntityServices(IDbContextFactory<ApplicationDbContext> factory, IMemoryCache cache) : base(factory, ctx => ctx.MissionsTakSuite, cache)
        {
            Includes = [_ => _.Team];
        }

        public Type ProvidedItem => typeof(MissionSuite);
    }
}
