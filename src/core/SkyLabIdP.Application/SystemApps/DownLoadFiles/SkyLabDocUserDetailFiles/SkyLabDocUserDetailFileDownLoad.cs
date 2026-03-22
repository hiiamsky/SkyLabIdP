
using SkyLabIdP.Application.Dtos;
using Mediator;

namespace SkyLabIdP.Application.SystemApps.DownLoadFiles.SkyLabDocUserDetailFiles
{
    public class SkyLabDocUserDetailFileDownLoad : IRequest<FileContentVM>
    {
        public string UserId { get; set; } = "";
        public string FileId { get; set; } = "";

        public string LoginUserId { get; set; } = "";

        public OperationResult operationResult { get; set; } = new OperationResult(false, "", 400);

    }
}


