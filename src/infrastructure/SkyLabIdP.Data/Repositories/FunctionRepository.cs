using System.Data;
using Dapper;
using SkyLabIdP.Application.Common.Interfaces.Repositories;
using SkyLabIdP.Domain.Entities;

namespace SkyLabIdP.Data.Repositories;

public class FunctionRepository : IFunctionRepository
{
    private readonly IDbConnection _connection;
    private readonly IDbTransaction? _transaction;

    public FunctionRepository(IDbConnection connection, IDbTransaction? transaction)
    {
        _connection = connection;
        _transaction = transaction;
    }

    public async Task<IEnumerable<Function>> GetByGroupIdsAsync(IEnumerable<string> groupIds, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT GroupID, FunctionID, FunctionIcon, FunctionEnglishDescription,
                   FunctionChineseDescription, TargetRoute, IsDisabled, IsDisplayInMenu, FunctionOrder
            FROM [Function]
            WHERE GroupID IN @GroupIds
            ORDER BY FunctionOrder
            """;

        return await _connection.QueryAsync<Function>(sql, new { GroupIds = groupIds }, _transaction);
    }

    public async Task<Function?> GetByIdAsync(string functionId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT GroupID, FunctionID, FunctionIcon, FunctionEnglishDescription,
                   FunctionChineseDescription, TargetRoute, IsDisabled, IsDisplayInMenu, FunctionOrder
            FROM [Function]
            WHERE FunctionID = @FunctionID
            """;

        return await _connection.QueryFirstOrDefaultAsync<Function>(sql, new { FunctionID = functionId }, _transaction);
    }

    public async Task<IEnumerable<Function>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT GroupID, FunctionID, FunctionIcon, FunctionEnglishDescription,
                   FunctionChineseDescription, TargetRoute, IsDisabled, IsDisplayInMenu, FunctionOrder
            FROM [Function]
            ORDER BY GroupID, FunctionOrder
            """;

        return await _connection.QueryAsync<Function>(sql, transaction: _transaction);
    }
}
