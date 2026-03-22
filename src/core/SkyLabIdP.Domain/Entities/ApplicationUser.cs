using Microsoft.AspNetCore.Identity;
namespace SkyLabIdP.Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public virtual ICollection<ApplicationUserClaim>? Claims { get; set; } = new List<ApplicationUserClaim>();
        public virtual ICollection<ApplicationUserLogin>? Logins { get; set; } = new List<ApplicationUserLogin>();
        public virtual ICollection<ApplicationUserToken>? Tokens { get; set; } = new List<ApplicationUserToken>();
        public virtual ICollection<ApplicationUserRole>? UserRoles { get; set; } = new List<ApplicationUserRole>();
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
        public bool IsApproved { get; set; } = false; // 標記是否為審核通過的帳號
        public bool IsActive { get; set; } = true;          // 標記帳號是否啟用
        public bool IsMigrated { get; set; } = false;      // 標記是否為從舊系統轉移過來的帳號
        public bool IsMigratedAndReSetPWed { get; set; } = false; // 標記是否舊系統轉移過來的帳號且已經重設密碼

        public override bool LockoutEnabled { get; set; } = false; // 標記帳號是否鎖定
        // 外部登入相關屬性
        public string? ExternalId { get; set; }  // 外部提供者的用戶 ID
        public string? ExternalProvider { get; set; }  // 外部提供者名稱 (例如 "Google", "Facebook")
        public bool IsExternalAccount { get; set; } = false;  // 標記是否為外部帳號
        public bool HasCompletedRegistration { get; set; } = false;  // 是否已完成資料補充
        public ICollection<PasswordHistory> PasswordHistories { get; set; } = [];
        public virtual ICollection<UserTenant> UserTenants { get; set; } = [];
        
        // 暫時註解掉，因為資料庫中不存在這些欄位
        // public string? SerialNo { get; set; }  // 外鍵名稱修正為 SerialNo 而非 SkyLabDocUserDetailSerialNo
        // public virtual SkyLabDocUserDetail SkyLabDocUserDetail { get; set; }
    }
}
