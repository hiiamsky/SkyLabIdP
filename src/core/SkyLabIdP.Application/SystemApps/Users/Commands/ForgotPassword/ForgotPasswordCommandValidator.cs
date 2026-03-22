using FluentValidation;
namespace SkyLabIdP.Application.SystemApps.Users.Commands.ForgotPassword
{
    public class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
    {
        public ForgotPasswordCommandValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("帳號是必填的。");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("電子郵件是必填的。")
                .EmailAddress().WithMessage("需要有效的電子郵件地址。");

        }

    }
}


