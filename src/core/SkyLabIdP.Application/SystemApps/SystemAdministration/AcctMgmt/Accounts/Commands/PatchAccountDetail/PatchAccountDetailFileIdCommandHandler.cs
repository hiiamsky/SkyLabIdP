using SkyLabIdP.Application.Common.Interfaces;
using SkyLabIdP.Application.Dtos;
using SkyLabIdP.Application.Dtos.SkyLabDocUserDetail;
using SkyLabIdP.Domain.Entities;
using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace SkyLabIdP.Application.SystemApps.SystemAdministration.AcctMgmt.Accounts.Commands.PatchAccountDetail
{
    public class PatchAccountDetailFileIdCommandHandler : IRequestHandler<PatchAccountDetailFileIdCommand, SkyLabDocUserDetailResponse>
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly IDataProtectionService _dataprotectionservice;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly ILogger<PatchAccountDetailFileIdCommandHandler> _logger;

        public PatchAccountDetailFileIdCommandHandler(IUnitOfWork unitOfWork, IDataProtectionService dataprotectionservice, UserManager<ApplicationUser> userManager, ILogger<PatchAccountDetailFileIdCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;

            _dataprotectionservice = dataprotectionservice;

            _userManager = userManager;

            _logger = logger;
        }
        public async ValueTask<SkyLabDocUserDetailResponse> Handle(PatchAccountDetailFileIdCommand request, CancellationToken cancellationToken)
        {
            var appUser = await _userManager.FindByIdAsync(request.UserId);
            if (appUser == null || appUser.IsApproved)
            {
                return new SkyLabDocUserDetailResponse
                {
                    operationResult = new OperationResult(false, "無使用者資訊或是已經通過審核", StatusCodes.Status404NotFound)
                };
            }

            var skylabDocUserDetail = await _unitOfWork.SkyLabDocUserDetails
                .GetByUserIdAsync(request.UserId, cancellationToken);

            if (skylabDocUserDetail == null)
            {
                return new SkyLabDocUserDetailResponse
                {
                    operationResult = new OperationResult(false, "無使用者資訊或是已經通過審核", StatusCodes.Status404NotFound)
                };
            }
            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                var decryptedFileId = _dataprotectionservice.Unprotect(request.FileId);
                await _unitOfWork.SkyLabDocUserDetails.UpdateFileIdAsync(
                    skylabDocUserDetail.UserId, decryptedFileId, skylabDocUserDetail.UserId, DateTime.Now, cancellationToken);

                await _unitOfWork.CommitAsync(cancellationToken);
                return new SkyLabDocUserDetailResponse
                {
                    UserId = _dataprotectionservice.Protect(request.UserId),
                    UserName = skylabDocUserDetail.UserName,
                    FullName = skylabDocUserDetail.FullName,
                    OfficialEmail = skylabDocUserDetail.OfficialEmail,
                    BranchCode = skylabDocUserDetail.BranchCode,
                    FileId = _dataprotectionservice.Protect(decryptedFileId),
                    SystemRole = "",
                    SubordinateUnit = skylabDocUserDetail.SubordinateUnit,
                    OfficialPhone = skylabDocUserDetail.OfficialPhone,
                    JobTitle = skylabDocUserDetail.JobTitle,
                    operationResult = new OperationResult(true, "SkyLabDocUserDetail updated successfully.", StatusCodes.Status200OK)
                };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "An error occurred while updating SkyLabDocUserDetail.");
                throw;
            }
        }
    }

}
