using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SkyLabIdP.Application.Common.Security
{
    /// <summary>
    /// URL 白名單驗證器 - 防範 SSRF 攻擊 (API7:2023)
    /// </summary>
    public interface IUrlWhitelistValidator
    {
        /// <summary>
        /// 驗證 URL 是否在白名單中
        /// </summary>
        bool IsUrlAllowed(string url);
        
        /// <summary>
        /// 驗證 URL 並返回詳細錯誤訊息
        /// </summary>
        (bool IsValid, string? ErrorMessage) ValidateUrl(string url);
    }

    public class UrlWhitelistValidator : IUrlWhitelistValidator
    {
        private readonly HashSet<string> _allowedDomains;
        private readonly ILogger<UrlWhitelistValidator> _logger;

        public UrlWhitelistValidator(IConfiguration configuration, ILogger<UrlWhitelistValidator> logger)
        {
            _logger = logger;
            
            // 從配置讀取允許的域名白名單
            var allowedUrls = configuration.GetSection("Security:AllowedResetPasswordDomains").Get<string[]>() 
                              ?? Array.Empty<string>();
            
            _allowedDomains = new HashSet<string>(allowedUrls, StringComparer.OrdinalIgnoreCase);
            
            _logger.LogInformation("URL 白名單已初始化，共 {Count} 個允許的域名", _allowedDomains.Count);
        }

        public bool IsUrlAllowed(string url)
        {
            var (isValid, _) = ValidateUrl(url);
            return isValid;
        }

        public (bool IsValid, string? ErrorMessage) ValidateUrl(string url)
        {
            // 1. 檢查 URL 是否為空
            if (string.IsNullOrWhiteSpace(url))
            {
                _logger.LogWarning("URL 驗證失敗：URL 為空");
                return (false, "URL 不可為空");
            }

            // 2. 嘗試解析 URL
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                _logger.LogWarning("URL 驗證失敗：無效的 URL 格式 - {Url}", url);
                return (false, "無效的 URL 格式");
            }

            // 3. 必須使用 HTTPS (生產環境強制)
            if (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp)
            {
                _logger.LogWarning("URL 驗證失敗：不支援的協議 - {Scheme}", uri.Scheme);
                return (false, "僅支援 HTTP/HTTPS 協議");
            }

            // 4. 防止內部網路 IP (SSRF 防護)
            if (IsPrivateOrLocalhost(uri))
            {
                _logger.LogWarning("URL 驗證失敗：嘗試存取內部網路 - {Host}", uri.Host);
                return (false, "不允許存取內部網路位址");
            }

            // 5. 檢查域名是否在白名單中
            var domain = uri.Host.ToLowerInvariant();
            if (!_allowedDomains.Any(allowed => IsMatchingDomain(domain, allowed)))
            {
                _logger.LogWarning("URL 驗證失敗：域名不在白名單中 - {Domain}", domain);
                return (false, $"域名 '{domain}' 不在允許的白名單中");
            }

            _logger.LogDebug("URL 驗證成功 - {Url}", url);
            return (true, null);
        }

        /// <summary>
        /// 檢查是否為私有 IP 或 localhost
        /// </summary>
        private static bool IsPrivateOrLocalhost(Uri uri)
        {
            var host = uri.Host.ToLowerInvariant();

            // localhost 檢查
            if (host == "localhost" || host == "127.0.0.1" || host == "::1")
            {
                return true;
            }

            // 內部域名檢查
            if (host.EndsWith(".local") || host.EndsWith(".internal"))
            {
                return true;
            }

            // 私有 IP 範圍檢查
            if (System.Net.IPAddress.TryParse(host, out var ip))
            {
                var bytes = ip.GetAddressBytes();
                
                // 10.0.0.0/8
                if (bytes[0] == 10)
                    return true;
                
                // 172.16.0.0/12
                if (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31)
                    return true;
                
                // 192.168.0.0/16
                if (bytes[0] == 192 && bytes[1] == 168)
                    return true;
                
                // 169.254.0.0/16 (Link-local)
                if (bytes[0] == 169 && bytes[1] == 254)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 檢查域名是否匹配白名單規則 (支援萬用字元)
        /// </summary>
        private static bool IsMatchingDomain(string domain, string allowedPattern)
        {
            allowedPattern = allowedPattern.ToLowerInvariant();

            // 完全匹配
            if (domain == allowedPattern)
                return true;

            // 萬用字元支援 (*.skylab.com.tw)
            if (allowedPattern.StartsWith("*."))
            {
                var baseDomain = allowedPattern.Substring(2);
                return domain == baseDomain || domain.EndsWith("." + baseDomain);
            }

            return false;
        }
    }
}
