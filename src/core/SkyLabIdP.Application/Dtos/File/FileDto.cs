using Microsoft.AspNetCore.Http;

namespace SkyLabIdP.Application.Dtos.File
{
    public class FileDto
    {
        public IFormFile File { get; set; } = null!;

        public string FileSystemType { get; set; } = "";
        public string? ApacheTikaContent { get; set; }

        public string? FileDescription { get; set; }

        // 由於這是 DTO，我們可能需要有一個用於上傳的檔案參數
        public byte[]? FileContent { get; set; }

        public string FileId { get; set; } = "";

        public string? OriginalFileName { get; set; }

        public string? FileExtension { get; set; }

        public string? FileSavePath { get; set; }

        public long FileSizeLimit { get; set; } = 0;

        //用於檔案檢查的副檔名 List
        public List<string>? FileCheckExtensions { get; set; }

        //用於檔案檢查的MIMI List
        public List<string>? FileCheckMimes { get; set; }

        public string CreatorId { get; set; } = "";
    }
};

