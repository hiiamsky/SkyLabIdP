using StackExchange.Redis;

namespace SkyLabIdP.WebApi.Extensions
{
    /// <summary>
    /// 快取相關的擴展方法
    /// </summary>
    public static class CacheExtensions
    {
        /// <summary>
        /// 添加 Redis 分散式快取服務
        /// </summary>
        /// <param name="services">IServiceCollection</param>
        /// <returns>更新後的服務集合</returns>
        public static IServiceCollection AddRedisCache(this IServiceCollection services)
        {
            // 讀取 Redis 配置
            var redisHost = Environment.GetEnvironmentVariable("REDIS__HOST");
            var redisPort = int.Parse(Environment.GetEnvironmentVariable("REDIS__PORT") ?? "6379");
            var redisUser = "";
            var redisPassword = Environment.GetEnvironmentVariable("REDIS__PASSWORD");

            // 建立 ConfigurationOptions
            ConfigurationOptions redisOptions = new()
            {
                EndPoints = { { redisHost!, redisPort } },
                Password = redisPassword,
                AbortOnConnectFail = false
            };

            // 如果 User 不為空，則設定用戶
            if (!string.IsNullOrWhiteSpace(redisUser))
            {
                redisOptions.User = redisUser;
            }

            // 註冊 Redis
            services.AddStackExchangeRedisCache(options =>
            {
                options.ConfigurationOptions = redisOptions;
            });

            return services;
        }

        /// <summary>
        /// 添加輸出快取服務
        /// </summary>
        /// <param name="services">IServiceCollection</param>
        /// <returns>更新後的服務集合</returns>
        public static IServiceCollection AddOutputCacheServices(this IServiceCollection services)
        {
            // 如果您需要额外配置 Output Cache，可以继续使用 AddOutputCache
            services.AddOutputCache(options =>
            {
                options.AddBasePolicy(policy => policy.Expire(TimeSpan.FromMinutes(5)));
                // 其他配置
            });

            services.AddResponseCaching();

            return services;
        }
    }
}
