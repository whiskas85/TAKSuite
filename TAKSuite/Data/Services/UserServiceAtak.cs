using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using TAKSuite.Data.Models;


namespace TAKSuite.Data.Services
{
    public class UserServiceAtak : DataServiceAbstract<UserAtak>
    {
        public UserServiceAtak(ApplicationDbContext context, IMemoryCache cache) : base(context.UsersAtak, context, cache)
        {
            Includes = [_ => _.Team];
        }
    }
}
