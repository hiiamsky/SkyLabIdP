using Asp.Versioning;
using SkyLabIdP.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SkyLabIdP.WebApi.Controllers.v1
{
    /// <summary>
    /// 提供JWT公鑰的JWKS端點
    /// </summary>
    [ApiVersion("1.0")]
    [Route("skylabidp/api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class JwksController(IJwtService jwtService, ILogger<JwksController> logger) : ControllerBase
    {
        private readonly IJwtService _jwtService = jwtService;
        private readonly ILogger<JwksController> _logger = logger;

        /// <summary>
        /// 提供JWT公鑰的JWKS端點
        /// </summary>
        /// <returns>JWKS格式的公鑰</returns>
        [HttpGet]
        [Route(".well-known/jwks.json")]
        [AllowAnonymous]
        public IActionResult GetJwks()
        {
            var jwks = _jwtService.GetJsonWebKeySet();
            _logger.LogInformation("成功提供JWKS，包含 {KeyCount} 個密鑰", jwks.Keys.Count);
            return Ok(jwks);
        }
    }
}