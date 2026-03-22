using System;
using SkyLabIdP.Application.Common.Interfaces;
using SkyLabIdP.Domain.Enums;
using FluentValidation;
using Microsoft.Extensions.Configuration;

namespace SkyLabIdP.Application.SystemApps.Users.Commands.RegisterUser;

public class RegisterSkyLabDevelopUserCommandValidator : AbstractValidator<RegisterSkyLabDevelopUserCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDataProtectionService _dataprotectionservice ;
    public RegisterSkyLabDevelopUserCommandValidator(IUnitOfWork unitOfWork, IConfiguration configuration, IDataProtectionService dataprotectionservice )
    {
        _unitOfWork = unitOfWork;
        _dataprotectionservice  = dataprotectionservice;


        RuleFor(x => x.SkyLabDevelopUserRegistrationRequest.Email)
            .NotEmpty().WithMessage("信箱是必填的。")
            .EmailAddress().WithMessage("需要有效的信箱。")
            .MustAsync(OfficialEmailIsUnique).WithMessage("信箱已存在。");

        RuleFor(x => x.SkyLabDevelopUserRegistrationRequest.OfficialPhone)
            .NotEmpty().WithMessage("公務電話是必填的。");

        RuleFor(x => x.SkyLabDevelopUserRegistrationRequest.Password)
            .NotEmpty().WithMessage("密碼是必填的。")
            .MinimumLength(12).WithMessage("密碼至少需要12個字符。")
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{12,}$").WithMessage("密碼必須包含至少一個大寫字母、一個小寫字母、一個數字和一個特殊字符。")
            .Equal(x => x.SkyLabDevelopUserRegistrationRequest.ConfirmPassword).WithMessage("密碼和確認密碼不匹配。");

        RuleFor(x => x.SkyLabDevelopUserRegistrationRequest.ConfirmPassword)
            .NotEmpty().WithMessage("確認密碼是必填的。");

        RuleFor(x => x.SkyLabDevelopUserRegistrationRequest.FullName)
            .NotEmpty().WithMessage("姓名是必填的。");

        RuleFor(x => x.SkyLabDevelopUserRegistrationRequest.BranchCode)
            .MaximumLength(10).WithMessage("服務機構的名稱不能超過10個字符。")
            .NotEmpty().WithMessage("服務機構是必填的。");



    }
    public async Task<bool> OfficialEmailIsUnique(string eMail, CancellationToken cancellationToken)
    {
        bool emailExists = await _unitOfWork.SkyLabDevelopUserDetails
                                         .ExistsByOfficialEmailAsync(eMail, cancellationToken);
        return !emailExists;
    }
}