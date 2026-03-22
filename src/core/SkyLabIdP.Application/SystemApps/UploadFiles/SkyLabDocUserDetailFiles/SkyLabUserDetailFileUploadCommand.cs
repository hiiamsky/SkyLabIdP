using SkyLabIdP.Application.Dtos.File;
using SkyLabIdP.Application.Dtos.File.SkyLabUserDetailFile;
using Mediator;

namespace SkyLabIdP.Application.SystemApps.UploadFiles.SkyLabDocUserDetailFiles
{
    public class SkyLabUserDetailFileUploadCommand : IRequest<SkyLabUserDetailFileResponse>
    {
        public FileDto FileUploadDto { get; set; } = new FileDto();

        public SkyLabUserDetailFileUploadCommand(FileDto fileUploadDto)
        {
            FileUploadDto = fileUploadDto ?? throw new ArgumentNullException(nameof(fileUploadDto));
        }
    }
}

