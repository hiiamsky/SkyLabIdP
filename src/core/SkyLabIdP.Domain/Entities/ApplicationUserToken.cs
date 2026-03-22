using Microsoft.AspNetCore.Identity;

namespace SkyLabIdP.Domain.Entities;

public class ApplicationUserToken : IdentityUserToken<string>
{
    public virtual ApplicationUser User { get; set; }
}
