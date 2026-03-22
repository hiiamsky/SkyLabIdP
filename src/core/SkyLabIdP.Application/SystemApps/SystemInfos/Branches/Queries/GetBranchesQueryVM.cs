using SkyLabIdP.Application.Dtos;
using SkyLabIdP.Application.Dtos.Branch;

namespace SkyLabIdP.Application.SystemApps.SystemInfos.Branches.Queries;

public class GetBranchesQueryVM
{
    public List<BranchDto> Branches { get; set; } = [];
    public OperationResult OperationResult { get; set; } = new OperationResult(false, "", 400);
}
