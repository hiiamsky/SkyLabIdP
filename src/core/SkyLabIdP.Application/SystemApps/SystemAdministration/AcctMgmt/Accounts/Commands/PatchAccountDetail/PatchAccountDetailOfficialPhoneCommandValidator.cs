using FluentValidation;

namespace SkyLabIdP.Application.SystemApps.SystemAdministration.AcctMgmt.Accounts.Commands.PatchAccountDetail
{

    public class PatchAccountDetailOfficialPhoneCommandValidator : AbstractValidator<PatchAccountDetailOfficialPhoneCommand>
    {

        public PatchAccountDetailOfficialPhoneCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("沒有提供用戶ID。");
            RuleFor(x => x.OfficialPhone)
                .NotEmpty().WithMessage("沒有提供電話。");
        }
    }
}
