using System.Data;
using System.Text.Json;
using Dapper;
using SkyLabIdP.Application.Common.Interfaces.Repositories;
using SkyLabIdP.Domain.Entities;

namespace SkyLabIdP.Data.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly IDbConnection _connection;
    private readonly IDbTransaction? _transaction;

    public AuditLogRepository(IDbConnection connection, IDbTransaction? transaction)
    {
        _connection = connection;
        _transaction = transaction;
    }

    public async Task AddAsync(AuditLog entity, CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO [AuditLog]
                (Id, UserId, UserName, TraceId, Timestamp, RequestMethod, RequestPath,
                 RequestQueryString, RequestBody, StatusCode, ResponseBody, ExecutionTime, IPAddress)
            VALUES
                (@Id, @UserId, @UserName, @TraceId, @Timestamp, @RequestMethod, @RequestPath,
                 @RequestQueryString, @RequestBody, @StatusCode, @ResponseBody, @ExecutionTime, @IPAddress)
            """;

        await _connection.ExecuteAsync(sql, new
        {
            entity.Id,
            entity.UserId,
            entity.UserName,
            entity.TraceId,
            entity.Timestamp,
            entity.RequestMethod,
            entity.RequestPath,
            entity.RequestQueryString,
            entity.RequestBody,
            entity.StatusCode,
            entity.ResponseBody,
            entity.ExecutionTime,
            entity.IPAddress
        }, _transaction);
    }
}
