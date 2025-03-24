using Microsoft.AspNetCore.Identity;
using TAKSuite.Data.Models;

namespace TAKSuite.Data;

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
    public Guid UserId { get; set; }
    public UserAtak User { get; set; }
}

