using SkyLabIdP.Application.Dtos;
using Mediator;

namespace SkyLabIdP.Application.SystemApps.Users.Commands.ResetPassword
{
    public class ResetPasswordCommand : IRequest<OperationResult>
    {
        public string UserId { get; set; } = "";
        public string Token { get; set; } = "";
        public string NewPassword { get; set; } = "";
        public string ConfirmPassword { get; set; } = "";
    }
};

