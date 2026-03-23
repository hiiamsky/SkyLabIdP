using SkyLabIdP.Application.Common.Interfaces;
using SkyLabIdP.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace SkyLabIdP.Application.SystemApps.Services.ServiceSettings;

public class ExternalLoginServiceSettings
{
    public required UserManager<ApplicationUser> UserManager { get; set; }
    public required IApplicationDbContext Context { get; set; }
    public required IJwtService JwtService { get; set; }
    public required ITenantUserServiceFactory TenantUserServiceFactory { get; set; }
    public required IDataProtectionService DataProtectionService { get; set; }
    public required ILoginNotificationService LoginNotificationService { get; set; }
    public required ITokenStorageService TokenStorageService { get; set; }
    public required ILogger<ExternalLoginServiceSettings> Logger { get; set; }
}
