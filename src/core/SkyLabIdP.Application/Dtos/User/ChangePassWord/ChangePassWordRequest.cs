using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SkyLabIdP.Application.Dtos.User.ChangePassWord
{
    public class ChangePassWordRequest
    {
        [Required]
        [Description("使用者ID")]
        public string UserId { get; set; } = "";
        [Required]
        [Description("密碼")]
        public string Password { get; set; } = "";
        [Required]
        [Description("新密碼")]
        public string NewPassword { get; set; } = "";
        [Required]
        [Description("確認新密碼")]
        public string ConfirmPassword { get; set; } = "";
    }
}
