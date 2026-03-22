using SkyLabIdP.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace SkyLabIdP.Identity.Services
{
    /// <summary>
    /// 中央密鑰存儲服務，用於存儲和管理JWT簽名密鑰
    /// </summary>
    public interface IKeyStoreService
    {
        /// <summary>
        /// 獲取當前活動的RSA密鑰
        /// </summary>
        RsaSecurityKey GetCurrentSigningKey();
        
        /// <summary>
        /// 獲取當前密鑰的ID (Kid)
        /// </summary>
        string GetCurrentKeyId();
        
        /// <summary>
        /// 獲取所有可用於驗證的密鑰
        /// </summary>
        IEnumerable<SecurityKey> GetAllValidationKeys();
        
        /// <summary>
        /// 添加密鑰到密鑰存儲
        /// </summary>
        void AddKey(RsaSecurityKey key, string kid);
        
        /// <summary>
        /// 刷新密鑰 - 用於金鑰輪換和熱更新
        /// </summary>
        Task RefreshKeysAsync();
    }
    
    /// <summary>
    /// 實現基於內存的密鑰存儲服務
    /// </summary>
    public class InMemoryKeyStoreService : IKeyStoreService
    {
        private static readonly object _lock = new object();
        private static RsaSecurityKey? _currentKey;
        private static string? _currentKid;
        private static readonly Dictionary<string, RsaSecurityKey> _allKeys = new Dictionary<string, RsaSecurityKey>();
        private readonly ILogger<InMemoryKeyStoreService> _logger;
        
        public InMemoryKeyStoreService(ILogger<InMemoryKeyStoreService> logger, IConfiguration configuration)
        {
            _logger = logger;
            
            // 如果密鑰尚未初始化，則初始化它
            lock (_lock)
            {
                if (_currentKey == null)
                {
                    InitializeKey(configuration);
                }
            }
        }
        
        /// <summary>
        /// 初始化RSA密鑰
        /// </summary>
        private void InitializeKey(IConfiguration configuration)
        {
            var rsa = RSA.Create(2048);
            
            // 嘗試從配置中加載RSA密鑰參數
            var rsaParams = configuration["JwtSettings:RsaPrivateKey"];
            if (!string.IsNullOrEmpty(rsaParams))
            {
                try
                {
                    // 從配置中載入RSA參數
                    var privateKeyBytes = Convert.FromBase64String(rsaParams);
                    rsa.ImportPkcs8PrivateKey(privateKeyBytes, out _);
                    _logger.LogInformation("成功從配置中載入RSA私鑰");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "無法載入RSA私鑰，將使用動態生成的密鑰。這在生產環境中不是好的做法。");
                }
            }
            else
            {
                _logger.LogWarning("未提供RSA私鑰配置，將使用動態生成的密鑰。這在生產環境中不是好的做法。");
            }
            
            _currentKey = new RsaSecurityKey(rsa);
            _currentKid = Guid.NewGuid().ToString("N");
            
            // 添加到密鑰存儲
            _allKeys[_currentKid] = _currentKey;
            
            _logger.LogInformation("已初始化RSA密鑰，KeyId: {KeyId}", _currentKid);
        }
        
        /// <summary>
        /// 獲取當前活動的RSA密鑰
        /// </summary>
        public RsaSecurityKey GetCurrentSigningKey()
        {
            return _currentKey ?? throw new InvalidOperationException("RSA密鑰尚未初始化");
        }
        
        /// <summary>
        /// 獲取當前密鑰的ID
        /// </summary>
        public string GetCurrentKeyId()
        {
            return _currentKid ?? throw new InvalidOperationException("RSA密鑰ID尚未初始化");
        }
        
        /// <summary>
        /// 獲取所有可用於驗證的密鑰
        /// </summary>
        public IEnumerable<SecurityKey> GetAllValidationKeys()
        {
            return _allKeys.Values.Cast<SecurityKey>();
        }
        
        /// <summary>
        /// 添加密鑰到密鑰存儲
        /// </summary>
        public void AddKey(RsaSecurityKey key, string kid)
        {
            lock (_lock)
            {
                if (!_allKeys.ContainsKey(kid))
                {
                    _allKeys[kid] = key;
                    _logger.LogInformation("添加新密鑰到存儲，KeyId: {KeyId}", kid);
                }
            }
        }
        
        /// <summary>
        /// 刷新密鑰 - 用於金鑰輪換和熱更新
        /// </summary>
        public Task RefreshKeysAsync()
        {
            lock (_lock)
            {
                // 這裡可以實現更複雜的金鑰更新邏輯
                // 例如從外部服務獲取新密鑰，或者生成新的密鑰
                _logger.LogInformation("刷新密鑰快取，當前共有 {Count} 個密鑰", _allKeys.Count);
                
                // 目前只是記錄信息，實際的密鑰刷新邏輯可以根據需求實現
                // 例如：檢查密鑰是否過期，從配置服務重新載入密鑰等
            }
            
            return Task.CompletedTask;
        }
    }
}