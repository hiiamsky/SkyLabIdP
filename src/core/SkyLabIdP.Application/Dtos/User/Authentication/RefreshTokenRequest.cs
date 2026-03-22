using System.ComponentModel.DataAnnotations;

namespace SkyLabIdP.Application.Dtos.User.Authentication
{
    /// <summary>
    /// 刷新令牌請求
    /// </summary>
    public class RefreshTokenRequest
    {
        /// <summary>
        /// 加密的用戶 ID，用於在 Redis 中查找對應的刷新令牌
        /// </summary>
        [Required(ErrorMessage = "UserId 為必填欄位")]
        public required string UserId { get; set; }
    }
}