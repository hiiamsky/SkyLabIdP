using System.Data;
using System.Text;
using Dapper;
using SkyLabIdP.Application.Common.Interfaces.Repositories;
using SkyLabIdP.Domain.Entities;

namespace SkyLabIdP.Data.Repositories;

public class BranchAreaRepository : IBranchAreaRepository
{
    private readonly IDbConnection _connection;
    private readonly IDbTransaction? _transaction;

    public BranchAreaRepository(IDbConnection connection, IDbTransaction? transaction)
    {
        _connection = connection;
        _transaction = transaction;
    }

    public async Task<IEnumerable<BranchArea>> QueryAsync(string? areaId, string? areaName, string? dstCode, string? cityCode, CancellationToken cancellationToken = default)
    {
        var sql = new StringBuilder("""
            SELECT AREAID AS AreaId, AREANA AS AreaName, AREAID2 AS AreaId2,
                   DstCode, ISDISPLAYED AS IsDisplayed, RELDSTCODE AS RelDstCode, CITYCODE AS CityCode
            FROM [AREA]
            WHERE 1=1
            """);

        var parameters = new DynamicParameters();

        if (!string.IsNullOrEmpty(areaId))
        {
            sql.Append(" AND AREAID = @AreaId");
            parameters.Add("AreaId", areaId);
        }

        if (!string.IsNullOrEmpty(areaName))
        {
            sql.Append(" AND AREANA LIKE @AreaName");
            parameters.Add("AreaName", $"%{areaName}%");
        }

        if (!string.IsNullOrEmpty(dstCode))
        {
            sql.Append(" AND RELDSTCODE = @DstCode");
            parameters.Add("DstCode", dstCode);
        }

        if (!string.IsNullOrEmpty(cityCode))
        {
            sql.Append(" AND CITYCODE = @CityCode");
            parameters.Add("CityCode", cityCode);
        }

        return await _connection.QueryAsync<BranchArea>(sql.ToString(), parameters, _transaction);
    }
}
