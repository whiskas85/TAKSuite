using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using TAKSuite.Data.Models;
using TAKSuite.Data.Services.BaseDataManagement;


namespace TAKSuite.Data.Services
{
    public class MissionSuiteEntityServices : DataServiceAbstract<MissionSuite>, IDataProvider
    {
        public MissionSuiteEntityServices(ApplicationDbContext context, IMemoryCache cache) : base(context.MissionsTakSuite, context, cache)
        {
            Includes = [_ => _.Team];
        }

        public Type ProvidedItem => typeof(MissionSuite);
    }
}
