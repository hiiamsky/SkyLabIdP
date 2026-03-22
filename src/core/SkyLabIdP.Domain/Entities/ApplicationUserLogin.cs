using Microsoft.AspNetCore.Identity;

namespace SkyLabIdP.Domain.Entities;

public class ApplicationUserLogin : IdentityUserLogin<string>
{
    public virtual ApplicationUser User { get; set; }
}
