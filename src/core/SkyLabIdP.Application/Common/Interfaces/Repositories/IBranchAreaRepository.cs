using SkyLabIdP.Domain.Entities;

namespace SkyLabIdP.Application.Common.Interfaces.Repositories;

public interface IBranchAreaRepository
{
    Task<IEnumerable<BranchArea>> QueryAsync(string? areaId, string? areaName, string? dstCode, string? cityCode, CancellationToken cancellationToken = default);
}
