using SkyLabIdP.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SkyLabIdP.Identity.Services
{
    public class ApiKeyValidation : IApiKeyValidation
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ApiKeyValidation> _logger;

        public ApiKeyValidation(IConfiguration configuration, ILogger<ApiKeyValidation> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }
        public bool IsValidApiKey(string userApiKey)
        {
            if (string.IsNullOrWhiteSpace(userApiKey))
            {
                _logger.LogWarning("API key validation failed: Invalid input");
                return false;
            }

            string? apiKey = Environment.GetEnvironmentVariable("SKYLABIDP_APIKEY");

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                _logger.LogError("API key validation failed: SKYLABIDP_APIKEY not found or empty");
                return false;
            }

            // Use constant-time comparison to prevent timing attacks
            bool isValid = ConstantTimeEquals(apiKey, userApiKey);

            if (!isValid)
            {
                _logger.LogWarning("API key validation failed: Authentication error");
            }
            else
            {
                _logger.LogDebug("API key validation successful");
            }

            return isValid;
        }

        /// <summary>
        /// Constant-time string comparison to prevent timing attacks
        /// 常數時間字串比較，防止時間攻擊
        /// </summary>
        /// <param name="expected">Expected string</param>
        /// <param name="actual">Actual string</param>
        /// <returns>True if strings are equal</returns>
        private static bool ConstantTimeEquals(string expected, string actual)
        {
            if (expected.Length != actual.Length)
            {
                return false;
            }

            var result = 0;
            for (int i = 0; i < expected.Length; i++)
            {
                result |= expected[i] ^ actual[i];
            }

            return result == 0;
        }
    }
}
