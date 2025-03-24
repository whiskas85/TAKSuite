using Microsoft.EntityFrameworkCore;
using TAKSuite.Data.Models;


namespace TAKSuite.Data.Services
{
    public class UserServiceAtak : DataServiceAbstract<UserAtak>
    {
        public UserServiceAtak(ApplicationDbContext context) : base(context.UsersAtak, context)
        {

        }
    }
}
