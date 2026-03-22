using FluentValidation;

namespace SkyLabIdP.Application.SystemApps.SystemInfos.Areas.Queries
{
    public class BranchAreaQueryValidator : AbstractValidator<BranchAreaQuery>
    {
        public BranchAreaQueryValidator()
        {
            RuleFor(x => x.AreaID)
                .MaximumLength(4).WithMessage("行政區編號長度不得超過4個字元。")
                .When(x => !string.IsNullOrEmpty(x.AreaID));

            RuleFor(x => x.AreaName)
                .MaximumLength(10).WithMessage("行政區名稱長度不得超過10個字元。")
                .When(x => !string.IsNullOrEmpty(x.AreaName));

            RuleFor(x => x.DstCode)
                .NotEmpty().WithMessage("分部區域碼是必填項。")
                .MaximumLength(2).WithMessage("分部區域碼長度不得超過2個字元。");

            RuleFor(x => x.CityCode)
                .MaximumLength(1).WithMessage("分部簡碼長度不得超過1個字元。")
                .When(x => !string.IsNullOrEmpty(x.CityCode));
        }
    }
}