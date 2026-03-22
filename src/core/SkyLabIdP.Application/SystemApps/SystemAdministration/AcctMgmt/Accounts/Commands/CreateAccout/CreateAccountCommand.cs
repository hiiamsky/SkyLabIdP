using SkyLabIdP.Application.Dtos.SkyLabDocUserDetail;
using Mediator;

namespace SkyLabIdP.Application.SystemApps.SystemAdministration.AcctMgmt.Accounts.Commands.CreateAccout
{
    public class CreateAccoutCommand : IRequest<SkyLabDocUserDetailResponse>
    {
        public SkyLabDocUserDetailRequest skylabDocUserDetailRequest { get; set; } = new SkyLabDocUserDetailRequest();
    }
}


