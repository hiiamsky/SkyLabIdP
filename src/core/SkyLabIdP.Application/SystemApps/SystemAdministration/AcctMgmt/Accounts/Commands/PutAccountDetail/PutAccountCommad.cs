using SkyLabIdP.Application.Dtos.AcctMaintain;
using SkyLabIdP.Application.Dtos.SkyLabDocUserDetail;
using SkyLabIdP.Application.SystemApps.SystemAdministration.AcctMgmt.Accounts.Queries;
using Mediator;

namespace SkyLabIdP.Application.SystemApps.SystemAdministration.AcctMgmt.Accounts.Commands.PutAccountDetail
{
    public class PutAccountCommad : IRequest<AcctMaintainQueryVM>
    {
        public string LoginUserId { get; set; } = "";
        public SkyLabDocUserDetailDto SkyLabDocUserDetailDto { get; set; } = new SkyLabDocUserDetailDto();

        public AcctMaintainFunctionPermissionRequest AcctMaintainFunctionPermissionRequest { get; set; } = new AcctMaintainFunctionPermissionRequest();
    }
}

