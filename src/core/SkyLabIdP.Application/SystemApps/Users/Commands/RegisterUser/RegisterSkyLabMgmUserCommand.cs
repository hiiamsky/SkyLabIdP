using SkyLabIdP.Application.Dtos.User.Registration;
using Mediator;

namespace SkyLabIdP.Application.SystemApps.Users.Commands.RegisterUser
{
    public class RegisterSkyLabMgmUserCommand : IRequest<SkyLabMgmUserRegistrationResponse>
    {
        public SkyLabMgmUserRegistrationRequest UserRegistrationRequest { get; set; } = new SkyLabMgmUserRegistrationRequest();

        public string TenantId { get; set; } = string.Empty;
    }
}


