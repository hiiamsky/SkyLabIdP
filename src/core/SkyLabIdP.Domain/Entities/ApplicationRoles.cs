using Microsoft.AspNetCore.Identity;

namespace SkyLabIdP.Domain.Entities;

public class ApplicationRoles : IdentityRole
{
    public virtual ICollection<ApplicationUserRole>? UserRoles { get; set; }
    public virtual ICollection<ApplicationRoleClaim>? RoleClaims { get; set; }
}
