using SkyLabIdP.Application.Dtos;
using SkyLabIdP.Application.Dtos.User.ChangePassWord;
using Mediator;

namespace SkyLabIdP.Application.SystemApps.Users.Commands.ChangePassWord
{
    public class ChangePassWordCommand : IRequest<OperationResult>
    {
        public ChangePassWordRequest ChangePassWordRequest { get; set; } = new ChangePassWordRequest();
    }
}
