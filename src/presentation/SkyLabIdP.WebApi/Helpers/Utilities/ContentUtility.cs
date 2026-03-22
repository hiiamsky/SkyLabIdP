using System;

namespace SkyLabIdP.WebApi.Helpers.Utilities;

/// <summary>
/// 處理內容檢測和過濾的工具類，包括二進制內容檢測、敏感內容過濾等共用邏輯。
/// 主要供 AuditLoggingMiddleware 和 SerilogAuditLogService 使用。
/// </summary>
public static class ContentUtility
{
    /// <summary>
    /// 檢查內容類型是否為二進制或大型內容
    /// </summary>
    /// <param name="contentType">HTTP 請求或響應的內容類型</param>
    /// <returns>如果內容類型為二進制或大型內容，則為 true；否則為 false</returns>
    public static bool IsBinaryOrLargeContentType(string contentType)
    {
        if (string.IsNullOrEmpty(contentType))
            return false;

        contentType = contentType.ToLower().Split(';')[0].Trim();

        // 圖像文件類型
        if (contentType.StartsWith("image/"))
            return true;

        // 音頻文件類型
        if (contentType.StartsWith("audio/"))
            return true;

        // 視頻文件類型
        if (contentType.StartsWith("video/"))
            return true;

        // 文件和應用類型
        if (contentType.StartsWith("application/"))
        {
            // 允許常見的 JSON 和 XML 類型通過
            if (contentType == "application/json" ||
                contentType == "application/xml" ||
                contentType == "application/problem+json")
                return false;

            // 所有其他 application/* 類型被視為二進制或大型內容
            return true;
        }

        // 其他二進制類型
        return contentType.Contains("octet-stream") ||
               contentType.Contains("multipart/form-data") ||
               contentType.Contains("binary") ||
               contentType.Contains("pdf") ||
               contentType.Contains("zip") ||
               contentType.Contains("gzip") ||
               contentType.Contains("msgpack");
    }

    /// <summary>
    /// 檢查路徑是否可能包含二進制內容
    /// </summary>
    /// <param name="path">請求路徑</param>
    /// <returns>如果路徑可能包含二進制內容，則為 true；否則為 false</returns>
    public static bool IsBinaryContentPath(string path)
    {
        if (string.IsNullOrEmpty(path))
            return false;

        string lowerPath = path.ToLower();

        // 常見的二進制內容路徑
        return lowerPath.Contains("/download") ||
               lowerPath.Contains("/upload") ||
               lowerPath.Contains("/file") ||
               lowerPath.Contains("/image") ||
               lowerPath.Contains("/audio") ||
               lowerPath.Contains("/video") ||
               lowerPath.Contains("/media") ||
               lowerPath.Contains("/attachment") ||
               lowerPath.Contains("/document") ||
               lowerPath.EndsWith(".jpg") ||
               lowerPath.EndsWith(".jpeg") ||
               lowerPath.EndsWith(".png") ||
               lowerPath.EndsWith(".gif") ||
               lowerPath.EndsWith(".bmp") ||
               lowerPath.EndsWith(".webp") ||
               lowerPath.EndsWith(".svg") ||
               lowerPath.EndsWith(".ico") ||
               lowerPath.EndsWith(".pdf") ||
               lowerPath.EndsWith(".doc") ||
               lowerPath.EndsWith(".docx") ||
               lowerPath.EndsWith(".xls") ||
               lowerPath.EndsWith(".xlsx") ||
               lowerPath.EndsWith(".pptx") ||
               lowerPath.EndsWith(".zip") ||
               lowerPath.EndsWith(".rar") ||
               lowerPath.EndsWith(".7z") ||
               lowerPath.EndsWith(".mp3") ||
               lowerPath.EndsWith(".wav") ||
               lowerPath.EndsWith(".ogg") ||
               lowerPath.EndsWith(".flac") ||
               lowerPath.EndsWith(".aac") ||
               lowerPath.EndsWith(".m4a") ||
               lowerPath.EndsWith(".wma") ||
               lowerPath.EndsWith(".mp4") ||
               lowerPath.EndsWith(".mov") ||
               lowerPath.EndsWith(".avi") ||
               lowerPath.EndsWith(".wmv") ||
               lowerPath.EndsWith(".mkv") ||
               lowerPath.EndsWith(".webm");
    }

    /// <summary>
    /// 檢查是否為敏感URL路徑
    /// </summary>
    /// <param name="path">請求路徑</param>
    /// <returns>如果路徑包含敏感資訊，則為 true；否則為 false</returns>
    public static bool IsSensitiveUrl(string path)
    {
        if (string.IsNullOrEmpty(path))
            return false;

        string lowerPath = path.ToLower();
        return lowerPath.Contains("login") ||
               lowerPath.Contains("password") ||
               lowerPath.Contains("token") ||
               lowerPath.Contains("auth") ||
               lowerPath.Contains("secret") ||
               lowerPath.Contains("credential") ||
               lowerPath.Contains("key") ||
               lowerPath.Contains("cert");
    }

    /// <summary>
    /// 檢查內容是否包含敏感資訊
    /// </summary>
    /// <param name="content">要檢查的內容</param>
    /// <returns>如果內容包含敏感資訊，則為 true；否則為 false</returns>
    public static bool IsSensitiveContent(string content)
    {
        if (string.IsNullOrEmpty(content))
            return false;

        string lowerContent = content.ToLower();
        return lowerContent.Contains("password") ||
               lowerContent.Contains("token") ||
               lowerContent.Contains("secret") ||
               lowerContent.Contains("key") ||
               lowerContent.Contains("credential") ||
               lowerContent.Contains("auth") ||
               lowerContent.Contains("login") ||
               lowerContent.Contains("cert");
    }

    /// <summary>
    /// 判斷內容是否為二進制內容
    /// </summary>
    /// <param name="method">HTTP 請求方法</param>
    /// <param name="path">請求路徑</param>
    /// <param name="content">要檢查的內容</param>
    /// <returns>如果內容為二進制內容，則為 true；否則為 false</returns>
    public static bool IsBinaryContent(string? method, string? path, string content)
    {
        if (string.IsNullOrEmpty(content))
            return false;

        // 檢查路徑是否可能包含二進制內容
        if (!string.IsNullOrEmpty(path) && IsBinaryContentPath(path))
        {
            return true;
        }

        // 檢查內容是否為 JSON 或 XML
        if (IsJsonOrXml(content))
        {
            // JSON 或 XML 通常不是二進制內容
            return false;
        }

        // 檢查內容是否包含二進制文件的特徵標記（魔術數字）
        if (ContainsBinaryFileSignature(content))
        {
            return true;
        }

        // 判斷內容是否看起來像 base64 編碼的二進制數據
        if (content.Length > 500 && IsBase64Like(content))
        {
            return true;
        }

        // 檢查內容是否包含大量非 ASCII 字符（可能是二進制數據）
        int nonAsciiCount = 0;
        int nonPrintableCount = 0;
        int totalCount = Math.Min(content.Length, 1000); // 只檢查前 1000 個字符
        for (int i = 0; i < totalCount; i++)
        {
            char c = content[i];
            if (c < 32 || c > 126)
            {
                nonAsciiCount++;
                if (c != '\r' && c != '\n' && c != '\t')
                {
                    nonPrintableCount++;
                }
            }
        }

        // 如果非 ASCII 字符比例超過 10%，或非打印字符比例超過 5%，可能是二進制內容
        return (double)nonAsciiCount / totalCount > 0.1 || (double)nonPrintableCount / totalCount > 0.05;
    }

    /// <summary>
    /// 檢查內容是否為 JSON 或 XML 格式
    /// </summary>
    /// <param name="content">要檢查的內容</param>
    /// <returns>如果內容是 JSON 或 XML 格式，則為 true；否則為 false</returns>
    public static bool IsJsonOrXml(string content)
    {
        if (string.IsNullOrEmpty(content))
            return false;

        string trimmed = content.Trim();

        // 簡單檢查是否為 JSON
        bool isJson = (trimmed.StartsWith("{") && trimmed.EndsWith("}")) ||
                      (trimmed.StartsWith("[") && trimmed.EndsWith("]"));

        // 簡單檢查是否為 XML
        bool isXml = trimmed.StartsWith("<") && trimmed.EndsWith(">") &&
                     (trimmed.Contains("<?xml") || trimmed.Contains("<!DOCTYPE"));

        return isJson || isXml;
    }

    /// <summary>
    /// 檢查字符串是否類似 base64 編碼
    /// </summary>
    /// <param name="content">要檢查的內容</param>
    /// <returns>如果內容看起來像 base64 編碼，則為 true；否則為 false</returns>
    public static bool IsBase64Like(string content)
    {
        // Base64 通常只包含 A-Z, a-z, 0-9, +, /, =
        int validChars = 0;
        foreach (char c in content)
        {
            if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') ||
                (c >= '0' && c <= '9') || c == '+' || c == '/' || c == '=')
            {
                validChars++;
            }
        }

        // 如果有超過 95% 的字符是有效的 Base64 字符，可能是 Base64 編碼
        return (double)validChars / content.Length > 0.95;
    }

    /// <summary>
    /// 檢查內容是否包含二進制文件的特徵標記（魔術數字）
    /// </summary>
    /// <param name="content">要檢查的內容</param>
    /// <returns>如果內容包含二進制文件的特徵標記，則為 true；否則為 false</returns>
    public static bool ContainsBinaryFileSignature(string content)
    {
        if (string.IsNullOrEmpty(content) || content.Length < 8)
            return false;

        // 檢查常見的二進制文件格式標記（魔術數字）
        try
        {
            // 將字符串的開頭轉換為字節數組以檢查魔術數字
            byte[] bytes = new byte[Math.Min(content.Length, 16)];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = (byte)content[i];
            }

            // JPEG 文件: FF D8 FF
            if (bytes.Length >= 3 && bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF)
                return true;

            // PNG 文件: 89 50 4E 47 0D 0A 1A 0A
            if (bytes.Length >= 8 && bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47 &&
                bytes[4] == 0x0D && bytes[5] == 0x0A && bytes[6] == 0x1A && bytes[7] == 0x0A)
                return true;

            // MP3 文件: ID3 或 FF FB
            if (bytes.Length >= 3 && bytes[0] == 0x49 && bytes[1] == 0x44 && bytes[2] == 0x33)
                return true;
            if (bytes.Length >= 2 && bytes[0] == 0xFF && (bytes[1] == 0xFB || bytes[1] == 0xF3 || bytes[1] == 0xF2))
                return true;

            // WAV 文件: RIFF....WAVE
            if (bytes.Length >= 12 && bytes[0] == 0x52 && bytes[1] == 0x49 && bytes[2] == 0x46 && bytes[3] == 0x46 &&
                bytes[8] == 0x57 && bytes[9] == 0x41 && bytes[10] == 0x56 && bytes[11] == 0x45)
                return true;

            // PDF 文件: %PDF
            if (bytes.Length >= 4 && bytes[0] == 0x25 && bytes[1] == 0x50 && bytes[2] == 0x44 && bytes[3] == 0x46)
                return true;

            // ZIP 文件 (包括 DOCX, XLSX, PPTX 等): PK
            if (bytes.Length >= 2 && bytes[0] == 0x50 && bytes[1] == 0x4B)
                return true;

            // FLAC 文件: fLaC
            if (bytes.Length >= 4 && bytes[0] == 0x66 && bytes[1] == 0x4C && bytes[2] == 0x61 && bytes[3] == 0x43)
                return true;

            // OGG 文件: OggS
            if (bytes.Length >= 4 && bytes[0] == 0x4F && bytes[1] == 0x67 && bytes[2] == 0x67 && bytes[3] == 0x53)
                return true;
        }
        catch
        {
            // 如果有任何轉換錯誤，安全返回 false
            return false;
        }

        return false;
    }

    /// <summary>
    /// 安全截斷內容
    /// </summary>
    /// <param name="content">要截斷的內容</param>
    /// <param name="maxLength">最大長度</param>
    /// <returns>截斷後的內容</returns>
    public static string TruncateContent(string content, int maxLength = 10000)
    {
        if (string.IsNullOrEmpty(content) || content.Length <= maxLength)
            return content;

        return $"{content.Substring(0, maxLength)}... [Content truncated, total length: {content.Length}]";
    }

    /// <summary>
    /// 處理內容，應用所有過濾器和安全檢查
    /// </summary>
    /// <param name="content">原始內容</param>
    /// <param name="method">HTTP 方法</param>
    /// <param name="path">請求路徑</param>
    /// <param name="contentType">內容類型</param>
    /// <param name="maxLength">最大長度</param>
    /// <returns>處理後的內容</returns>
    public static string ProcessContent(string content, string? method, string? path, string? contentType, int maxLength = 10000)
    {
        if (string.IsNullOrEmpty(content))
            return content;

        // 檢查是否為敏感URL
        if (!string.IsNullOrEmpty(path) && IsSensitiveUrl(path))
        {
            return "[Sensitive data]";
        }

        // 檢查內容類型
        if (!string.IsNullOrEmpty(contentType) && IsBinaryOrLargeContentType(contentType))
        {
            return $"[Binary content: {contentType}]";
        }

        // 檢查路徑是否包含二進制內容
        if (!string.IsNullOrEmpty(path) && IsBinaryContentPath(path))
        {
            if (content.Length > 100)
                return $"[Binary content (path-based detection), length: {content.Length}]";
        }

        // 檢查內容是否為二進制
        if (IsBinaryContent(method, path, content))
        {
            return "[Binary content]";
        }

        // 檢查內容是否敏感
        if (IsSensitiveContent(content))
        {
            return "[Sensitive content]";
        }

        // 截斷過長內容
        return TruncateContent(content, maxLength);
    }
}
