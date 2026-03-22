using SkyLabIdP.Application.Dtos;
using SkyLabIdP.Application.Dtos.BranchArea;

namespace SkyLabIdP.Application.SystemApps.SystemInfos.Areas.Queries
{
    public class BranchAreaQueryVM
    {
        public List<BranchAreaDto> BranchAreas { get; set; } = [];

        public OperationResult OperationResult { get; set; } = new OperationResult(true, "");
    }


}


