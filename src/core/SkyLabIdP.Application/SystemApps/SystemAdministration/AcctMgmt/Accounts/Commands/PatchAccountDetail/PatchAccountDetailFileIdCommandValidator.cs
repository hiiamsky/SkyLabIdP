using SkyLabIdP.Application.Common.Interfaces;
using SkyLabIdP.Domain.Enums;
using FluentValidation;
using Microsoft.Extensions.Configuration;
namespace SkyLabIdP.Application.SystemApps.SystemAdministration.AcctMgmt.Accounts.Commands.PatchAccountDetail
{


    public class PatchAccountDetailFileIdCommandValidator : AbstractValidator<PatchAccountDetailFileIdCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly IDataProtectionService _dataprotectionservice ;
        public PatchAccountDetailFileIdCommandValidator(IUnitOfWork unitOfWork, IConfiguration configuration, IDataProtectionService dataprotectionservice )
        {
            _unitOfWork = unitOfWork;
            _dataprotectionservice  = dataprotectionservice;

            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("沒有提供用戶ID。");
            RuleFor(x => x.FileId)
                .NotEmpty().WithMessage("沒有提供文件ID。")
                .MustAsync((command, fileId, cancellation) => BeFileIdNotExistFileUploads(_dataprotectionservice .Unprotect(fileId), cancellation)).WithMessage("申請書ID不存在上傳申請書中。")
                .MustAsync((command, fileId, cancellation) => BeNotUniqueFileId(_dataprotectionservice .Unprotect(fileId), command.UserId, cancellation)).WithMessage("申請書ID已存在。");

        }
        public async Task<bool> BeNotUniqueFileId(string fileId, string userId, CancellationToken cancellationToken)
        {
            var exists = await _unitOfWork.SkyLabDocUserDetails
                .ExistsByFileIdAsync(fileId, cancellationToken);
            return !exists;
        }

        public async Task<bool> BeFileIdNotExistFileUploads(string fileId, CancellationToken cancellationToken)
        {
            var exists = await _unitOfWork.FileUploads
                .ExistsByFileIdAndSystemTypeAsync(fileId, SystemFileType.SkyLabDocUserDetailDocument.ToString(), cancellationToken);
            return exists;
        }
    }
}


