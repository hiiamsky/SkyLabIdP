using Microsoft.AspNetCore.Identity;

namespace SkyLabIdP.Domain.Entities;

public class ApplicationUserRole : IdentityUserRole<string>
{
    public virtual ApplicationUser? User { get; set; }
    public virtual ApplicationRoles? Role { get; set; }
}
