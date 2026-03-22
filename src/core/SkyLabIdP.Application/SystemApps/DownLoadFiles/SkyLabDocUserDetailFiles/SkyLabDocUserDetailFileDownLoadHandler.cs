using SkyLabIdP.Application.Common.Interfaces;
using SkyLabIdP.Application.Dtos;

using SkyLabIdP.Domain.Enums;
using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SkyLabIdP.Application.SystemApps.DownLoadFiles.SkyLabDocUserDetailFiles
{
    public class SkyLabDocUserDetailFileDownLoadHandler : IRequestHandler<SkyLabDocUserDetailFileDownLoad, FileContentVM>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly IDataProtectionService _dataprotectionservice;
        private readonly IFileService _fileService;
        private readonly ILogger<SkyLabDocUserDetailFileDownLoadHandler> _logger;

        public SkyLabDocUserDetailFileDownLoadHandler(IUnitOfWork unitOfWork, IConfiguration configuration, IDataProtectionService dataprotectionservice, IFileService fileService, ILogger<SkyLabDocUserDetailFileDownLoadHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _dataprotectionservice =dataprotectionservice;
            _fileService = fileService;
            _logger = logger;
        }

        public async ValueTask<FileContentVM> Handle(SkyLabDocUserDetailFileDownLoad request, CancellationToken cancellationToken)
        {
            var decryptedUserId = _dataprotectionservice.Unprotect(request.UserId);
                var decryptedFileId = _dataprotectionservice.Unprotect(request.FileId);
                _logger.LogInformation("Handling file download for User: {UserId} and File: {FileId}", decryptedUserId, decryptedFileId);

                var userDetailExists = await _unitOfWork.SkyLabDocUserDetails
                    .ExistsByUserIdAndFileIdAsync(decryptedUserId, decryptedFileId, cancellationToken);

                if (!userDetailExists)
                {
                    _logger.LogWarning("No matching user detail record found.");
                    return new FileContentVM
                    {
                        OperationResult = new OperationResult(false, "沒有所要下載的檔案資訊", StatusCodes.Status404NotFound)
                    };
                }

                var file = await _unitOfWork.FileUploads
                    .GetByFileIdAndSystemTypeAsync(decryptedFileId, SystemFileType.SkyLabDocUserDetailDocument.ToString(), cancellationToken);

                if (file == null)
                {
                    _logger.LogWarning("No matching file record found.");
                    return new FileContentVM
                    {
                        OperationResult = new OperationResult(false, "沒有要下載的檔案資訊", StatusCodes.Status404NotFound)
                    };
                }

                var filePathAndName = Path.Combine(_configuration["FileUploadSettings:SkyLabDocUserDetailFilesFloderPath"] ?? string.Empty, $"{file.FileId}.{file.FileExtension}");
                _logger.LogInformation("Fetching file from path: {FilePathAndName}", filePathAndName);

                using var fileStream = await _fileService.GetFileAsync(filePathAndName);
                if (fileStream == null)
                {
                    _logger.LogError("File not found at path: {FilePathAndName}", filePathAndName);
                    return new FileContentVM
                    {
                        OperationResult = new OperationResult(false, "檔案不存在於預期的位置", StatusCodes.Status404NotFound)
                    };
                }

                using var memoryStream = new MemoryStream();
                await fileStream.CopyToAsync(memoryStream, cancellationToken);
                var fileContent = memoryStream.ToArray();

                return new FileContentVM
                {
                    Content = fileContent,
                    ContentType = "application/pdf",
                    FileName = file.OriginalFileName,
                    OperationResult = new OperationResult(true, "下載檔案成功", StatusCodes.Status200OK)
                };

        }
    }
}
