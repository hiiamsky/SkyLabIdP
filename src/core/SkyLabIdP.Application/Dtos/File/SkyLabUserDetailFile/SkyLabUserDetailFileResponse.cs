

namespace SkyLabIdP.Application.Dtos.File.SkyLabUserDetailFile
{
    public class SkyLabUserDetailFileResponse
    {
        public string FileId { get; set; } = "";

        public OperationResult OperationResult { get; set; } = new OperationResult(true, "", 200);
    }
}


