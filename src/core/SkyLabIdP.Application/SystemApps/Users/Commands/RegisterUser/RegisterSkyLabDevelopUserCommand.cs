using System;
using SkyLabIdP.Application.Dtos.User.Registration;
using Mediator;

namespace SkyLabIdP.Application.SystemApps.Users.Commands.RegisterUser;

public class RegisterSkyLabDevelopUserCommand : IRequest<SkyLabDevelopUserRegistrationResponse>
{
    public SkyLabDevelopUserRegistrationRequest SkyLabDevelopUserRegistrationRequest { get; set; } = new SkyLabDevelopUserRegistrationRequest();

    public string TenantId { get; set; } = string.Empty;
}
