using System.ComponentModel.DataAnnotations;

namespace SkyLabIdP.Application.Dtos.User.Authentication
{
    /// <summary>
    /// 外部用戶註冊資料 DTO
    /// </summary>
    public class ExternalUserRegistrationDto
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;
        
        [Required]
        public string BranchCode { get; set; } = string.Empty;
        
        [StringLength(20)]
        public string OfficialPhone { get; set; } = string.Empty;
        

        
        public string SubordinateUnit { get; set; } = string.Empty;
        
        public string JobTitle { get; set; } = string.Empty;

        public string TenantId { get; set; } = string.Empty;
    }
}