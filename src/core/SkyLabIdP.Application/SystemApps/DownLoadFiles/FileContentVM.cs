using SkyLabIdP.Application.Dtos;

namespace SkyLabIdP.Application.SystemApps.DownLoadFiles
{
    public class FileContentVM
    {
        public byte[] Content { get; set; } = null!;
        public string ContentType { get; set; } = "";
        public string FileName { get; set; } = "";

        public OperationResult OperationResult { get; set; } = new OperationResult(false, "", 400);
    }
}



