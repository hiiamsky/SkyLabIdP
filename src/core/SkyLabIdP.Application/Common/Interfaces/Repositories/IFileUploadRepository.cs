using SkyLabIdP.Domain.Entities;

namespace SkyLabIdP.Application.Common.Interfaces.Repositories;

public interface IFileUploadRepository
{
    Task<FileUpload?> GetByFileIdAndSystemTypeAsync(string fileId, string fileSystemType, CancellationToken cancellationToken = default);
    Task<bool> ExistsByFileIdAndSystemTypeAsync(string fileId, string fileSystemType, CancellationToken cancellationToken = default);
    Task AddAsync(FileUpload entity, CancellationToken cancellationToken = default);
}
