using Microsoft.Identity.Client;

namespace SkyLabIdP.Application.Dtos.User.Registration
{
    public class BaseUserRegistrationRequest
    {
        public string Email { get; set; } = "";


        public string UserName { get; set; } = "";
        public string Password { get; set; } = "";
        public string ConfirmPassword { get; set; } = "";

        public string TenantId { get; set; } = ""; // 需在 DTO 中新增這欄
    }
}

