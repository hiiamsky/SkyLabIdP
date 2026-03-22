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

    public async Task<IEnumerable<FunctionGroup>> GetAllWithFunctionsAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT fg.GroupID, fg.GroupIcon, fg.GroupTitle, fg.GroupEnglishDescription,
                   fg.GroupChineseDescription, fg.TargetRoute, fg.IsDisabled, fg.IsOpenFunctionList, fg.GroupOrder,
                   f.GroupID, f.FunctionID, f.FunctionIcon, f.FunctionEnglishDescription,
                   f.FunctionChineseDescription, f.TargetRoute, f.IsDisabled, f.IsDisplayInMenu, f.FunctionOrder
            FROM [FunctionGroup] fg
            LEFT JOIN [Function] f ON f.GroupID = fg.GroupID
            WHERE fg.IsDisabled = 0
            ORDER BY fg.GroupOrder, f.FunctionOrder
            """;

        var groupDict = new Dictionary<string, FunctionGroup>();

        await _connection.QueryAsync<FunctionGroup, Function, FunctionGroup>(
            sql,
            (group, function) =>
            {
                if (!groupDict.TryGetValue(group.GroupID, out var existingGroup))
                {
                    existingGroup = group;
                    existingGroup.Functions = new List<Function>();
                    groupDict[group.GroupID] = existingGroup;
                }

                if (function != null && !string.IsNullOrEmpty(function.FunctionID))
                {
                    ((List<Function>)existingGroup.Functions).Add(function);
                }

                return existingGroup;
            },
            transaction: _transaction,
            splitOn: "GroupID");

        return groupDict.Values;
    }

    public async Task<IEnumerable<FunctionGroup>> GetFilteredWithFunctionsAsync(string? groupId = null, string? functionId = null, CancellationToken cancellationToken = default)
    {
        var whereClauses = new List<string>();
        var parameters = new DynamicParameters();

        if (!string.IsNullOrEmpty(groupId))
        {
            whereClauses.Add("fg.GroupID = @GroupID");
            parameters.Add("GroupID", groupId);
        }
        if (!string.IsNullOrEmpty(functionId))
        {
            whereClauses.Add("f.FunctionID = @FunctionID");
            parameters.Add("FunctionID", functionId);
        }

        var whereClause = whereClauses.Count > 0
            ? "WHERE " + string.Join(" AND ", whereClauses)
            : "";

        var sql = $"""
            SELECT fg.GroupID, fg.GroupIcon, fg.GroupTitle, fg.GroupEnglishDescription,
                   fg.GroupChineseDescription, fg.TargetRoute, fg.IsDisabled, fg.IsOpenFunctionList, fg.GroupOrder,
                   f.GroupID, f.FunctionID, f.FunctionIcon, f.FunctionEnglishDescription,
                   f.FunctionChineseDescription, f.TargetRoute, f.IsDisabled, f.IsDisplayInMenu, f.FunctionOrder
            FROM [FunctionGroup] fg
            LEFT JOIN [Function] f ON f.GroupID = fg.GroupID
            {whereClause}
            ORDER BY fg.GroupOrder, f.FunctionOrder
            """;

        var groupDict = new Dictionary<string, FunctionGroup>();

        await _connection.QueryAsync<FunctionGroup, Function, FunctionGroup>(
            sql,
            (group, function) =>
            {
                if (!groupDict.TryGetValue(group.GroupID, out var existingGroup))
                {
                    existingGroup = group;
                    existingGroup.Functions = new List<Function>();
                    groupDict[group.GroupID] = existingGroup;
                }

                if (function != null && !string.IsNullOrEmpty(function.FunctionID) && function.IsDisplayInMenu)
                {
                    ((List<Function>)existingGroup.Functions).Add(function);
                }

                return existingGroup;
            },
            param: parameters,
            transaction: _transaction,
            splitOn: "GroupID");

        return groupDict.Values;
    }
}
