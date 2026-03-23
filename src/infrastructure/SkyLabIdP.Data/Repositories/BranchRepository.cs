using System.Data;
using Dapper;
using SkyLabIdP.Application.Common.Interfaces.Repositories;
using SkyLabIdP.Domain.Entities;

namespace SkyLabIdP.Data.Repositories;

public class BranchRepository : IBranchRepository
{
    private readonly IDbConnection _connection;
    private readonly IDbTransaction? _transaction;

    public BranchRepository(IDbConnection connection, IDbTransaction? transaction)
    {
        _connection = connection;
        _transaction = transaction;
    }

    public async Task<Branch?> GetByCodeAsync(string branchCode, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT BranchCode, BranchName, RegionCode
            FROM [Branch]
            WHERE BranchCode = @BranchCode
            """;

        return await _connection.QueryFirstOrDefaultAsync<Branch>(
            sql,
            new { BranchCode = branchCode },
            _transaction);
    }

    public async Task<IEnumerable<Branch>> GetByCodesAsync(IEnumerable<string> codes, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT BranchCode, BranchName, RegionCode
            FROM [Branch]
            WHERE BranchCode IN @Codes
            """;

        return await _connection.QueryAsync<Branch>(sql, new { Codes = codes }, _transaction);
    }

    public async Task<IEnumerable<Branch>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT BranchCode, BranchName, RegionCode FROM [Branch] ORDER BY BranchCode";

        return await _connection.QueryAsync<Branch>(sql, transaction: _transaction);
    }
}
