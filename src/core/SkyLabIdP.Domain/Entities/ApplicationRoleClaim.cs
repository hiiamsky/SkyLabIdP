using SkyLabIdP.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace SkyLabIdP.Domain;

public class ApplicationRoleClaim : IdentityRoleClaim<string>
{
    public virtual ApplicationRoles? Role { get; set; }
}