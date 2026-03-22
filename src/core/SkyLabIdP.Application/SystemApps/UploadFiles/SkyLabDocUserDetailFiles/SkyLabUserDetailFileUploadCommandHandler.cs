using SkyLabIdP.Application.Common.Interfaces;
using SkyLabIdP.Application.Dtos;
using SkyLabIdP.Application.Dtos.File.SkyLabUserDetailFile;
using SkyLabIdP.Domain.Enums;
using Mediator;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SkyLabIdP.Application.SystemApps.UploadFiles.SkyLabDocUserDetailFiles
{
    public class SkyLabUserDetailFileUploadCommandHandler : IRequestHandler<SkyLabUserDetailFileUploadCommand, SkyLabUserDetailFileResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;

        private readonly IFileService _fileService;

        private readonly IDataProtectionService _dataProtectionService;

        private readonly ILogger<SkyLabUserDetailFileUploadCommandHandler> _logger;
        public SkyLabUserDetailFileUploadCommandHandler(IUnitOfWork unitOfWork, IConfiguration configuration, IFileService fileService, IDataProtectionService dataProtectionService, ILogger<SkyLabUserDetailFileUploadCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _fileService = fileService;
            _dataProtectionService = dataProtectionService;
            _logger = logger;

        }
        public async ValueTask<SkyLabUserDetailFileResponse> Handle(SkyLabUserDetailFileUploadCommand request, CancellationToken cancellationToken)
        {
            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                var fileDto = request.FileUploadDto;
                fileDto.FileId = Guid.NewGuid().ToString();
                fileDto.FileSizeLimit = 100 * 1024 * 1024;
                fileDto.FileSystemType = SystemFileType.SkyLabDocUserDetailDocument.ToString();
                fileDto.FileExtension = _fileService.GetFileExtension(fileDto);
                fileDto.FileCheckExtensions = ["pdf"];
                fileDto.FileCheckMimes = ["application/pdf"];
                fileDto.FileSavePath = _configuration["FileUploadSettings:SkyLabDocUserDetailFilesFloderPath"];
                fileDto.CreatorId = Guid.NewGuid().ToString();
                await _fileService.SaveFileAsync(fileDto);

                // Save the file details to the database
                var fileUpload = new Domain.Entities.FileUpload
                {
                    FileId = fileDto.FileId,
                    OriginalFileName = fileDto.File.FileName,
                    FileExtension = fileDto.FileExtension,
                    FileSystemType = fileDto.FileSystemType,
                    ApacheTikaContent = fileDto.ApacheTikaContent ?? string.Empty,
                    IsDisabled = false,
                    FileDescription = fileDto.FileDescription ?? string.Empty,
                    Comments = "",
                    CreatorId = fileDto.CreatorId,
                    CreatedTime = DateTime.UtcNow
                };

                await _unitOfWork.FileUploads.AddAsync(fileUpload, cancellationToken);
                await _unitOfWork.CommitAsync(cancellationToken);
                var response = new SkyLabUserDetailFileResponse()
                {
                    FileId = _dataProtectionService.Protect(fileDto.FileId),
                    OperationResult = new Dtos.OperationResult(true, "上傳檔案成功", 200)
                };

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "上傳檔案過程中發生錯誤");
                await _unitOfWork.RollbackAsync(cancellationToken);

                throw;
            }


        }
    }
}


