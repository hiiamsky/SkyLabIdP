using System.Data;
using System.Text;
using Dapper;
using SkyLabIdP.Application.Common.Interfaces.Repositories;
using SkyLabIdP.Domain.Entities;

namespace SkyLabIdP.Data.Repositories;

public class SysCodeRepository : ISysCodeRepository
{
    private readonly IDbConnection _connection;
    private readonly IDbTransaction? _transaction;

    public SysCodeRepository(IDbConnection connection, IDbTransaction? transaction)
    {
        _connection = connection;
        _transaction = transaction;
    }

    public async Task<IEnumerable<SysCode>> QueryAsync(string? type, string? code, CancellationToken cancellationToken = default)
    {
        var sql = new StringBuilder("""
            SELECT SerialNo, [Type], Code, [Desc] AS Description,
                   Item1, Item2, Item3, Item4, Item5, Item6, Item7, Item8, Item9, Item10,
                   Item11, Item12, Item13, Item14, Item15, Item16, Item17, Item18, Item19, Item20,
                   StopTag, Ord, createBy AS CreateBy, createDate AS CreateDate,
                   LastUpdateBy, LastUpdateDate
            FROM [SysCodes]
            WHERE 1=1
            """);

        var parameters = new DynamicParameters();

        if (!string.IsNullOrEmpty(type))
        {
            sql.Append(" AND [Type] = @Type");
            parameters.Add("Type", type);
        }

        if (!string.IsNullOrEmpty(code))
        {
            sql.Append(" AND Code = @Code");
            parameters.Add("Code", code);
        }

        sql.Append(" ORDER BY Ord");

        return await _connection.QueryAsync<SysCode>(sql.ToString(), parameters, _transaction);
    }
}
