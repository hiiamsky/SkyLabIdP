using Serilog;

namespace SkyLabIdP.WebApi.Extensions
{
    /// <summary>
    /// 配置相關的擴展方法
    /// </summary>
    public static class ConfigurationExtensions
    {
        /// <summary>
        /// 添加環境變數配置
        /// </summary>
        /// <param name="builder">WebApplicationBuilder</param>
        /// <returns>更新後的 builder</returns>
        public static WebApplicationBuilder AddEnvironmentConfiguration(this WebApplicationBuilder builder)
        {
            // 讀取環境變數
            var connectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING")
                ?? throw new InvalidOperationException("DATABASE_CONNECTION_STRING environment variable is not set.");
            
            var jwtSecretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") 
                ?? throw new InvalidOperationException("JWT_SECRET_KEY environment variable is not set.");

            // 允許 JWT_RSA_PRIVATE_KEY 為空，這種情況下會動態生成臨時密鑰
            var jwtRsaPrivateKey = Environment.GetEnvironmentVariable("JWT_RSA_PRIVATE_KEY");
            if (!string.IsNullOrEmpty(jwtRsaPrivateKey))
            {
                try
                {
                    // 確保環境變數包含有效的 Base64 字符串
                    var privateKeyBytes = Convert.FromBase64String(jwtRsaPrivateKey);
                    Log.Information("成功讀取 RSA 私鑰環境變數");
                }
                catch (FormatException ex)
                {
                    Log.Error(ex, "JWT_RSA_PRIVATE_KEY 環境變數不是有效的 Base64 字符串，將使用動態生成密鑰");
                    jwtRsaPrivateKey = null; // 設為 null 以使用動態生成密鑰
                }
            }
            else
            {
                Log.Warning("未提供 JWT_RSA_PRIVATE_KEY 環境變數，將使用動態生成密鑰");
            }

            // 讀取 API Key 環境變數
            var apiKey = Environment.GetEnvironmentVariable("SKYLABIDP_APIKEY");
            if (!string.IsNullOrEmpty(apiKey))
            {
                builder.Configuration["ApiKey"] = apiKey;
                Log.Information("成功讀取 API Key 環境變數");
            }

            // 讀取 Redis 環境變數
            var redisHost = Environment.GetEnvironmentVariable("REDIS__HOST");
            var redisPassword = Environment.GetEnvironmentVariable("REDIS__PASSWORD");
            if (!string.IsNullOrEmpty(redisHost))
            {
                builder.Configuration["Redis:Host"] = redisHost;
                if (!string.IsNullOrEmpty(redisPassword))
                {
                    builder.Configuration["Redis:Password"] = redisPassword;
                }
                Log.Information("成功讀取 Redis 環境變數");
            }

            // 🔧 讀取 OAuth 環境變數
            LoadOAuthConfiguration(builder);

            // 更新配置中的連接字符串
            builder.Configuration["ConnectionStrings:DefaultConnection"] = connectionString;
            builder.Configuration["JwtSettings:SecretKey"] = jwtSecretKey;
            if (!string.IsNullOrEmpty(jwtRsaPrivateKey))
            {
                builder.Configuration["JwtSettings:RsaPrivateKey"] = jwtRsaPrivateKey;
            }

            return builder;
        }

        /// <summary>
        /// 載入 OAuth 配置從環境變數
        /// </summary>
        /// <param name="builder">WebApplicationBuilder</param>
        private static void LoadOAuthConfiguration(WebApplicationBuilder builder)
        {
            Log.Information("開始載入 OAuth 環境變數配置");

            // 只檢查預設的 Google OAuth 配置
            var defaultClientId = Environment.GetEnvironmentVariable("OAUTH_GOOGLE_DEFAULT_CLIENT_ID");
            var defaultClientSecret = Environment.GetEnvironmentVariable("OAUTH_GOOGLE_DEFAULT_CLIENT_SECRET");

            if (!string.IsNullOrEmpty(defaultClientId) && !string.IsNullOrEmpty(defaultClientSecret))
            {
                builder.Configuration["ExternalLogin:Google:Default:ClientId"] = defaultClientId;
                builder.Configuration["ExternalLogin:Google:Default:ClientSecret"] = defaultClientSecret;
                
                Log.Information("成功載入 Google OAuth 預設配置");
            }
            else
            {
                Log.Warning("缺少 Google OAuth 環境變數: OAUTH_GOOGLE_DEFAULT_CLIENT_ID 或 OAUTH_GOOGLE_DEFAULT_CLIENT_SECRET，外部登入功能可能無法正常使用");
            }
        }

        /// <summary>
        /// 添加 Serilog 配置
        /// </summary>
        /// <param name="builder">WebApplicationBuilder</param>
        /// <returns>更新後的 builder</returns>
        public static WebApplicationBuilder AddSerilogConfiguration(this WebApplicationBuilder builder)
        {
            builder.WebHost.UseDefaultServiceProvider(configure => configure.ValidateOnBuild = true);
            builder.Services.AddSerilog(options =>
            {
                // 從配置中讀取 Serilog 設定
                options.ReadFrom.Configuration(builder.Configuration);
            });

            return builder;
        }
    }
}
