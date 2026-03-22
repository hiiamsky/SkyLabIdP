using SkyLabIdP.Application.Dtos.File;

namespace SkyLabIdP.Application.Common.Interfaces
{
    public interface IFileService
    {
        // 上傳檔案到指定路徑
        Task SaveFileAsync(FileDto fileDto);

        // 從指定路徑取得檔案
        Task<Stream> GetFileAsync(string filePath);

        // 驗證檔案大小是否符合限制
        bool ValidateFileSize(FileDto fileDto);

        // 驗證檔案副檔名是否符合限制
        bool ValidateFileExtension(FileDto fileDto);

        // 驗證檔案真實格式是否符合限制
        bool ValidateFileMime(FileDto fileDto);

        // 獲取檔案的 MIME 類型
        string GetMimeType(FileDto fileDto);

        // 獲取檔案的大小
        long GetFileSize(FileDto fileDto);

        // 獲取檔案的副檔名
        string GetFileExtension(FileDto fileDto);
    }
}
