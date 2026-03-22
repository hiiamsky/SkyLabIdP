using SkyLabIdP.Application.Dtos.FunctionGroup;

namespace SkyLabIdP.Application.Dtos.AcctMaintain
{
    public class AcctMaintainFunctionPermissionRequest
    {
        public string UserId { get; set; } = "";

        public List<FunctionGroupDto> FunctionGroups { get; set; } = [];

    }
}

