using System.Data;
using Dapper;
using SkyLabIdP.Application.Common.Interfaces.Repositories;
using SkyLabIdP.Domain.Entities;

namespace SkyLabIdP.Data.Repositories;

public class FileUploadRepository : IFileUploadRepository
{
    private readonly IDbConnection _connection;
    private readonly IDbTransaction? _transaction;

    public FileUploadRepository(IDbConnection connection, IDbTransaction? transaction)
    {
        _connection = connection;
        _transaction = transaction;
    }

    public async Task<FileUpload?> GetByFileIdAndSystemTypeAsync(string fileId, string fileSystemType, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT SerialNo, FileId, OriginalFileName, FileExtension, FileSystemType,
                   FileDescription, ApacheTikaContent, IsDisabled, Comments, CreatorId, CreatedTime
            FROM [FileUploads]
            WHERE FileId = @FileId AND FileSystemType = @FileSystemType
            """;

        return await _connection.QueryFirstOrDefaultAsync<FileUpload>(
            sql,
            new { FileId = fileId, FileSystemType = fileSystemType },
            _transaction);
    }

    public async Task<IEnumerable<FileUpload>> GetByIdsAsync(IEnumerable<string> fileIds, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT SerialNo, FileId, OriginalFileName, FileExtension, FileSystemType,
                   FileDescription, ApacheTikaContent, IsDisabled, Comments, CreatorId, CreatedTime
            FROM [FileUploads]
            WHERE FileId IN @FileIds
            """;

        return await _connection.QueryAsync<FileUpload>(sql, new { FileIds = fileIds }, _transaction);
    }

    public async Task<bool> ExistsByFileIdAndSystemTypeAsync(string fileId, string fileSystemType, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM [FileUploads]
                WHERE FileId = @FileId AND FileSystemType = @FileSystemType
            ) THEN 1 ELSE 0 END
            """;

        return await _connection.ExecuteScalarAsync<bool>(
            sql,
            new { FileId = fileId, FileSystemType = fileSystemType },
            _transaction);
    }

    public async Task AddAsync(FileUpload entity, CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO [FileUploads]
                (FileId, OriginalFileName, FileExtension, FileSystemType,
                 FileDescription, ApacheTikaContent, IsDisabled, Comments, CreatorId, CreatedTime)
            VALUES
                (@FileId, @OriginalFileName, @FileExtension, @FileSystemType,
                 @FileDescription, @ApacheTikaContent, @IsDisabled, @Comments, @CreatorId, @CreatedTime)
            """;

        await _connection.ExecuteAsync(sql, new
        {
            entity.FileId,
            entity.OriginalFileName,
            entity.FileExtension,
            entity.FileSystemType,
            entity.FileDescription,
            entity.ApacheTikaContent,
            entity.IsDisabled,
            entity.Comments,
            entity.CreatorId,
            entity.CreatedTime
        }, _transaction);
    }
}
