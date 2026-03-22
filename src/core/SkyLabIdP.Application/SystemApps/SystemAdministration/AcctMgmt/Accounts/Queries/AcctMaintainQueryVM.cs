using SkyLabIdP.Application.Dtos;
using SkyLabIdP.Application.Dtos.AcctMaintain;
using SkyLabIdP.Application.Dtos.SkyLabDocUserDetail;
using Microsoft.AspNetCore.Http;

namespace SkyLabIdP.Application.SystemApps.SystemAdministration.AcctMgmt.Accounts.Queries
{
    public class AcctMaintainQueryVM
    {
        public SkyLabDocUserDetailDto SkyLabDocUserDetailDto { get; set; } = new SkyLabDocUserDetailDto();

        public AcctMaintainFunctionPermissionResponse AcctMaintainFunctionPermissionResponse { get; set; } = new AcctMaintainFunctionPermissionResponse();
        public OperationResult OperationResult { get; set; } = new OperationResult(true, "", StatusCodes.Status200OK);
    }
}


