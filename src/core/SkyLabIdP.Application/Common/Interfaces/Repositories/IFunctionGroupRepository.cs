using SkyLabIdP.Domain.Entities;

namespace SkyLabIdP.Application.Common.Interfaces.Repositories;

public interface IFunctionGroupRepository
{
    Task<IEnumerable<FunctionGroup>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<FunctionGroup>> GetFilteredAsync(string? groupId = null, CancellationToken cancellationToken = default);
}
