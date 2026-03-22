using SkyLabIdP.Application.Dtos.Branch;
using SkyLabIdP.Application.Common.Interfaces;
using SkyLabIdP.Application.Common.Mappings;
using SkyLabIdP.Application.Dtos;
using Mediator;

namespace SkyLabIdP.Application.SystemApps.SystemInfos.Branches.Queries;

public class GetBranchesQueryHandler(IUnitOfWork unitOfWork, SkyLabIdPMapper mapper)
    : IRequestHandler<GetBranchesQuery, GetBranchesQueryVM>
{
    public async ValueTask<GetBranchesQueryVM> Handle(GetBranchesQuery request, CancellationToken cancellationToken)
    {
        var branchEntities = await unitOfWork.Branches.GetAllAsync(cancellationToken);
        var branches = branchEntities.Select(b => mapper.BranchToDto(b)).ToList();

        return new GetBranchesQueryVM
        {
            Branches = branches,
            OperationResult = new OperationResult(true, "成功取得分公司清單", 200)
        };
    }
}
