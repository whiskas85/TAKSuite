using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using TAKSuite.Data.Models;


namespace TAKSuite.Data.Services
{
    public class UserServiceAtak : DataServiceAbstract<UserAtak>
    {
        public UserServiceAtak(IDbContextFactory<ApplicationDbContext> factory, IMemoryCache cache) : base(factory, ctx => ctx.UsersAtak, cache)
        {
            Includes = [_ => _.Team];
        }
    }
}
