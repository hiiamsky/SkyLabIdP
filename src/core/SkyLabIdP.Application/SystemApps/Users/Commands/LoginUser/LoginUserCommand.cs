
using SkyLabIdP.Application.Dtos.User.Authentication;
using Mediator;
using System.ComponentModel.DataAnnotations;

namespace SkyLabIdP.Application.SystemApps.Users.Commands.LoginUser
{

    public class LoginUserCommand : IRequest<AuthenticateResponse>
    {
        [Required]
        public string UserName { get; set; } = "";

        [Required]
        public string Password { get; set; } = "";

        public string CaptchaId { get; set; } = "";

        public string CaptchaCode { get; set; } = "";

        public string? TenantId { get; set; } = null;

        public string? Email { get; set; } = null;

        /// <summary>
        /// 客戶端 IP 地址，用於登入通知和安全記錄
        /// </summary>
        public string? IpAddress { get; set; } = null;
    }



}
