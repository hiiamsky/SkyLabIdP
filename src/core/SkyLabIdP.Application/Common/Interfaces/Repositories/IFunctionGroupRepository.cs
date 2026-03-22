using SkyLabIdP.Domain.Entities;

namespace SkyLabIdP.Application.Common.Interfaces.Repositories;

public interface IFunctionGroupRepository
{
    Task<IEnumerable<FunctionGroup>> GetAllWithFunctionsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<FunctionGroup>> GetFilteredWithFunctionsAsync(string? groupId = null, string? functionId = null, CancellationToken cancellationToken = default);
}
