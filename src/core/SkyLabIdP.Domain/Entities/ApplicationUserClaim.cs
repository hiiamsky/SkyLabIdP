using Microsoft.AspNetCore.Identity;

namespace SkyLabIdP.Domain.Entities;

public class ApplicationUserClaim : IdentityUserClaim<string>
{
    public virtual ApplicationUser User { get; set; }
}
