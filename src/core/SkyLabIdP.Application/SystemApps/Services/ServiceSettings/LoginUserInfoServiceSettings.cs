using System;
using SkyLabIdP.Application.Common.Interfaces;
using SkyLabIdP.Domain.Entities;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SkyLabIdP.Application.SystemApps.Services;

namespace SkyLabIdP.Application.SystemApps.Services.ServiceSettings;

public class LoginUserInfoServiceSettings
{
    public required IUnitOfWork UnitOfWork { get; set; }
    public required UserManager<ApplicationUser> UserManager { get; set; }
    public required IConfiguration Configuration { get; set; }
    public required IDataProtectionService Dataprotectionservice { get; set; }
    public required IJwtService JwtService { get; set; }
    public required ILogger<AbstractLoginUserInfoService> Logger { get; set; }
    public required IEmailService EmailService { get; set; }
    public required ILoginNotificationService LoginNotificationService { get; set; }
    public required ISaltGenerator SaltGenerator { get; set; }
    public required SkyLabIdPMapper Mapper { get; set; }
    public required ICaptchaService CaptchaService { get; set; }
    public required IDistributedCache Cache { get; set; }

    public required ITokenStorageService TokenStorageService { get; set; }
}
