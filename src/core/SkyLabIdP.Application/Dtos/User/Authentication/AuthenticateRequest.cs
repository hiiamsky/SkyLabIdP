using System.ComponentModel.DataAnnotations;

namespace SkyLabIdP.Application.Dtos.User.Authentication
{
    public class AuthenticateRequest
    {
        [Required]
        public string UserName { get; set; } = "";

        [Required]
        public string Password { get; set; } = "";
    }
}

