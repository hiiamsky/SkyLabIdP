using SkyLabIdP.Application.Dtos;
using SkyLabIdP.Application.Dtos.FunctionGroup;

namespace SkyLabIdP.Application.SystemApps.SystemInfos.Menus.Queries
{
    public class MenuVM
    {
        public List<FunctionGroupDto> FunctionGroups { get; set; } = [];

        public OperationResult OperationResult { get; set; } = new OperationResult(true, "");
    }

}
