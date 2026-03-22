using SkyLabIdP.Application.Dtos.FunctionGroup;

namespace SkyLabIdP.Application.Dtos.AcctMaintain
{
    public class AcctMaintainFunctionPermissionResponse
    {
        public string UserId { get; set; } = "";

        public List<FunctionGroupDto> FunctionGroups { get; set; } = [];

        public OperationResult OperationResult { get; set; } = new OperationResult(true, "");

    }


}
