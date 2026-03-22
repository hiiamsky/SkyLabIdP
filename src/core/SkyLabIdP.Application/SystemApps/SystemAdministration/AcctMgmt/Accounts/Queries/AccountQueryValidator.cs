using FluentValidation;

namespace SkyLabIdP.Application.SystemApps.SystemAdministration.AcctMgmt.Accounts.Queries
{
    public class AccountQueryValidator : AbstractValidator<AccountQuery>
    {
        public AccountQueryValidator()
        {

            RuleFor(x => x.PageNumber)
                .GreaterThan(0).WithMessage("頁碼必須大於0。");

            RuleFor(x => x.PageSize)
                .GreaterThan(0).WithMessage("頁面大小必須大於0。");
        }
    }
}
