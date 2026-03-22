using FluentValidation;

namespace SkyLabIdP.Application.SystemApps.Users.Commands.ChangePassWord;

public class ChangePassWordCommandValidator : AbstractValidator<ChangePassWordCommand>
{
    public ChangePassWordCommandValidator()
    {
        RuleFor(x => x.ChangePassWordRequest.UserId)
            .NotEmpty().WithMessage("用戶ID是必填的。");


        RuleFor(x => x.ChangePassWordRequest.NewPassword)
            .NotEmpty().WithMessage("新密碼是必填的。");

        RuleFor(x => x.ChangePassWordRequest.NewPassword)
            .NotEmpty().WithMessage("新密碼是必填的。")
            .MinimumLength(12).WithMessage("新密碼至少需要12個字符。")
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{12,}$").WithMessage("密碼必須包含至少一個大寫字母、一個小寫字母、一個數字和一個特殊字符。")
            .Equal(x => x.ChangePassWordRequest.ConfirmPassword).WithMessage("兩次輸入的密碼不匹配。");

        RuleFor(x => x.ChangePassWordRequest.ConfirmPassword)
            .NotEmpty().WithMessage("確認密碼是必填的。");
    }
}
