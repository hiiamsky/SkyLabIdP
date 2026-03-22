using SkyLabIdP.Application.Common.Interfaces;

using SkyLabIdP.Domain.Enums;
using FluentValidation;
using Microsoft.Extensions.Configuration;

namespace SkyLabIdP.Application.SystemApps.Users.Commands.RegisterUser
{
    public class RegisterSkyLabMgmUserCommandValidator : AbstractValidator<RegisterSkyLabMgmUserCommand>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDataProtectionService _dataprotectionservice ;
        public RegisterSkyLabMgmUserCommandValidator(IUnitOfWork unitOfWork, IConfiguration configuration, IDataProtectionService dataprotectionservice )
        {
            _unitOfWork = unitOfWork;
            _dataprotectionservice  = dataprotectionservice;
            RuleFor(x => x.UserRegistrationRequest.UserName)
                .NotEmpty().WithMessage("帳號是必填的。")
                .MinimumLength(3).WithMessage("帳號至少需要3個字符。")
                .MustAsync(UserNameIsUnique).WithMessage("帳號已存在。")
                .Must((request, userName) => userName != request.UserRegistrationRequest.Password)
                .WithMessage("帳號不能與密碼相同。");

            RuleFor(x => x.UserRegistrationRequest.Email)
                .NotEmpty().WithMessage("信箱是必填的。")
                .EmailAddress().WithMessage("需要有效的信箱。")
                .MustAsync(OfficialEmailIsUnique).WithMessage("信箱已存在。");

            RuleFor(x => x.UserRegistrationRequest.OfficialPhone)
                .NotEmpty().WithMessage("公務電話是必填的。");

            RuleFor(x => x.UserRegistrationRequest.Password)
                .NotEmpty().WithMessage("密碼是必填的。")
                .MinimumLength(12).WithMessage("密碼至少需要12個字符。")
                .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{12,}$").WithMessage("密碼必須包含至少一個大寫字母、一個小寫字母、一個數字和一個特殊字符。")
                .Equal(x => x.UserRegistrationRequest.ConfirmPassword).WithMessage("密碼和確認密碼不匹配。");

            RuleFor(x => x.UserRegistrationRequest.ConfirmPassword)
                .NotEmpty().WithMessage("確認密碼是必填的。");

            RuleFor(x => x.UserRegistrationRequest.FullName)
                .NotEmpty().WithMessage("姓名是必填的。");

            RuleFor(x => x.UserRegistrationRequest.BranchCode)
                .MaximumLength(10).WithMessage("服務機構的名稱不能超過10個字符。")
                .NotEmpty().WithMessage("服務機構是必填的。");

            RuleFor(x => x.UserRegistrationRequest.FileId)
                .NotEmpty().WithMessage("申請書是必填的。")
                .MustAsync(BeFileIdExistFileUploads).WithMessage("申請書ID不存在上傳申請書中。")
                .MustAsync(FileIdIsUnique).WithMessage("申請書ID已存在。");

        }
        public async Task<bool> UserNameIsUnique(string userName, CancellationToken cancellationToken)
        {
            bool exists = await _unitOfWork.SkyLabDocUserDetails
                                        .ExistsByUserNameAsync(userName, cancellationToken);
            return !exists;
        }
        public async Task<bool> OfficialEmailIsUnique(string eMail, CancellationToken cancellationToken)
        {
            bool emailExists = await _unitOfWork.SkyLabDocUserDetails
                                             .ExistsByOfficialEmailAsync(eMail, cancellationToken);
            return !emailExists;
        }
        public async Task<bool> FileIdIsUnique(string fileId, CancellationToken cancellationToken)
        {
            var fileIdDecrypted = _dataprotectionservice .Unprotect(fileId);
            var exists = await _unitOfWork.SkyLabDocUserDetails
                         .ExistsByFileIdAsync(fileIdDecrypted, cancellationToken);
            return !exists;
        }

        public async Task<bool> BeFileIdExistFileUploads(string fileId, CancellationToken cancellationToken)
        {

            var fileIdDecrypted = _dataprotectionservice .Unprotect(fileId);
            var Exists = await _unitOfWork.FileUploads
                              .ExistsByFileIdAndSystemTypeAsync(fileIdDecrypted, SystemFileType.SkyLabDocUserDetailDocument.ToString(), cancellationToken);
            return Exists;
        }
    }
}