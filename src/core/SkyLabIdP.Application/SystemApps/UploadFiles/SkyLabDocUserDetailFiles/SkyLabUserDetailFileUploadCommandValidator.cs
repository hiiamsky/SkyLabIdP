
using SkyLabIdP.Domain.Enums;
using FluentValidation;

namespace SkyLabIdP.Application.SystemApps.UploadFiles.SkyLabDocUserDetailFiles
{
    public class SkyLabUserDetailFileUploadCommandValidator : AbstractValidator<SkyLabUserDetailFileUploadCommand>
    {
        public SkyLabUserDetailFileUploadCommandValidator()
        {
            RuleFor(x => x.FileUploadDto.FileSystemType)
                .NotNull().WithMessage("檔案類型必填")
                .Matches(SystemFileType.SkyLabDocUserDetailDocument.ToString()).WithMessage("檔案類型錯誤");
        }
    }
}


