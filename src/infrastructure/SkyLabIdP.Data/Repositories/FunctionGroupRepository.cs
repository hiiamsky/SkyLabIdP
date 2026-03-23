using System.Data;
using Dapper;
using SkyLabIdP.Application.Common.Interfaces.Repositories;
using SkyLabIdP.Domain.Entities;

namespace SkyLabIdP.Data.Repositories;

public class FunctionGroupRepository : IFunctionGroupRepository
{
    private readonly IDbConnection _connection;
    private readonly IDbTransaction? _transaction;

    public FunctionGroupRepository(IDbConnection connection, IDbTransaction? transaction)
    {
        _connection = connection;
        _transaction = transaction;
    }

    public async Task<IEnumerable<FunctionGroup>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT GroupID, GroupIcon, GroupTitle, GroupEnglishDescription,
                   GroupChineseDescription, TargetRoute, IsDisabled, IsOpenFunctionList, GroupOrder
            FROM [FunctionGroup]
            WHERE IsDisabled = 0
            ORDER BY GroupOrder
            """;

        return await _connection.QueryAsync<FunctionGroup>(sql, transaction: _transaction);
    }

    public async Task<IEnumerable<FunctionGroup>> GetFilteredAsync(string? groupId = null, CancellationToken cancellationToken = default)
    {
        var whereClauses = new List<string>();
        var parameters = new DynamicParameters();

        if (!string.IsNullOrEmpty(groupId))
        {
            whereClauses.Add("GroupID = @GroupID");
            parameters.Add("GroupID", groupId);
        }

        var whereClause = whereClauses.Count > 0
            ? "WHERE " + string.Join(" AND ", whereClauses)
            : "";

        var sql = $"""
            SELECT GroupID, GroupIcon, GroupTitle, GroupEnglishDescription,
                   GroupChineseDescription, TargetRoute, IsDisabled, IsOpenFunctionList, GroupOrder
            FROM [FunctionGroup]
            {whereClause}
            ORDER BY GroupOrder
            """;

        return await _connection.QueryAsync<FunctionGroup>(sql, parameters, _transaction);
    }
}
