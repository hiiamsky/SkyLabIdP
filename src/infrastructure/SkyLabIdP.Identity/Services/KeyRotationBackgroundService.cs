using SkyLabIdP.Application.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SkyLabIdP.Identity.Services
{
    /// <summary>
    /// 背景服務用於定期更新 JWT 金鑰
    /// </summary>
    public class KeyRotationBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<KeyRotationBackgroundService> _logger;
        private readonly TimeSpan _refreshInterval = TimeSpan.FromHours(1); // 每小時檢查一次
        
        public KeyRotationBackgroundService(
            IServiceScopeFactory serviceScopeFactory,
            ILogger<KeyRotationBackgroundService> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("JWT 金鑰輪換背景服務已啟動");
            
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceScopeFactory.CreateScope();
                    var keyStoreService = scope.ServiceProvider.GetRequiredService<IKeyStoreService>();
                    
                    await keyStoreService.RefreshKeysAsync();
                    _logger.LogDebug("已成功更新 JWT 金鑰快取");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "JWT 金鑰更新失敗");
                }
                
                try
                {
                    await Task.Delay(_refreshInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // 正常的取消操作，不需要記錄錯誤
                    break;
                }
            }
            
            _logger.LogInformation("JWT 金鑰輪換背景服務已停止");
        }
    }
}
