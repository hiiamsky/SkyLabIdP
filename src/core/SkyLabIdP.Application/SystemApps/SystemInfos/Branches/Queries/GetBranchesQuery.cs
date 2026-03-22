using SkyLabIdP.Application.Dtos.Branch;
using Mediator;

namespace SkyLabIdP.Application.SystemApps.SystemInfos.Branches.Queries;

public record GetBranchesQuery : IRequest<GetBranchesQueryVM>;
