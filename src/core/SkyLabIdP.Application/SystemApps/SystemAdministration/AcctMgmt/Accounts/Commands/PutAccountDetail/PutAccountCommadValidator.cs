
using SkyLabIdP.Application.Common.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Configuration;


namespace SkyLabIdP.Application.SystemApps.SystemAdministration.AcctMgmt.Accounts.Commands.PutAccountDetail
{
    public class PutAccountCommadValidator : AbstractValidator<PutAccountCommad>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDataProtectionService _dataprotectionservice ;
        public PutAccountCommadValidator(IUnitOfWork unitOfWork, IDataProtectionService dataprotectionservice , IConfiguration configuration)
        {
            _dataprotectionservice  = dataprotectionservice;
            _unitOfWork = unitOfWork;
            RuleFor(x => x.SkyLabDocUserDetailDto.UserId)
                .NotEmpty().WithMessage("沒有使用者ID。");

            RuleFor(x => x.SkyLabDocUserDetailDto.OfficialPhone)
                .NotEmpty().WithMessage("公務電話是必填的。");

            RuleFor(x => x.SkyLabDocUserDetailDto.FullName)
                .NotEmpty().WithMessage("姓名是必填的。");

            RuleFor(x => x.SkyLabDocUserDetailDto.BranchCode)
                .MaximumLength(10).WithMessage("服務機構的名稱不能超過10個字符。")
                .NotEmpty().WithMessage("服務機構是必填的。");

            RuleFor(x => x.SkyLabDocUserDetailDto.OfficialEmail)
            .NotEmpty().WithMessage("信箱是必填的。")
            .EmailAddress().WithMessage("需要有效的信箱。")
            .MustAsync((cmd, email, cancellation) => BeUniqueOfficialEmail(email, _dataprotectionservice .Unprotect(cmd.SkyLabDocUserDetailDto.UserId), cancellation))
            .WithMessage("信箱已存在。");
        }

        public async Task<bool> BeUniqueOfficialEmail(string eMail, string currentUserId, CancellationToken cancellationToken)
        {
            var exists = await _unitOfWork.SkyLabDocUserDetails
                .ExistsByOfficialEmailExcludingUserAsync(eMail, currentUserId, cancellationToken);
            return !exists;
        }

    }
}