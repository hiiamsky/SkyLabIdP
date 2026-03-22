using SkyLabIdP.Application.Dtos;
using Mediator;

namespace SkyLabIdP.Application.SystemApps.Users.Commands.ForgotPassword
{
    public class ForgotPasswordCommand : IRequest<OperationResult>
    {
        public string Username { get; set; } = "";
        public string Email { get; set; } = "";
        public string TenantId { get; set; } = "";

    }
}
