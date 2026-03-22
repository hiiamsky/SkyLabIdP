using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkyLabIdP.Domain.Entities
{
    [Table("SkyLabDocUserDetail")]
    public class SkyLabDocUserDetail
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("SerialNo")]
        [Description("流水號，自動加一")]
        public int SerialNo { get; set; }

        [Column("UserId")]
        [MaxLength(450)]
        [Description("使用者Id")]
        public string UserId { get; set; } = string.Empty;

        [Column("SystemRole")]
        [MaxLength(255)]
        [Description("系統角色")]
        public string SystemRole { get; set; } = string.Empty;

        [Column("FileId")]
        [MaxLength(450)]
        [Description("檔案Id")]
        public string FileId { get; set; } = string.Empty;

        [Column("UserName")]
        [MaxLength(256)]
        [Description("使用者帳號")]
        public string UserName { get; set; } = string.Empty;

        [Column("FullName")]
        [MaxLength(255)]
        [Description("使用者姓名")]
        public string FullName { get; set; } = string.Empty;

        [Column("BranchCode")]
        [MaxLength(10)]
        [Description("分公司代碼")]
        public string BranchCode { get; set; } = string.Empty;

        [Column("RegionCode")]
        [MaxLength(10)]
        [Description("地區代碼")]
        public string RegionCode { get; set; } = string.Empty;

        [Column("DepartmentName")]
        [MaxLength(255)]
        [Description("部門名稱")]
        public string DepartmentName { get; set; } = string.Empty;

        [Column("SubordinateUnit")]
        [MaxLength(255)]
        [Description("所屬單位")]
        public string SubordinateUnit { get; set; } = string.Empty;

        [Column("JobTitle")]
        [MaxLength(255)]
        [Description("職稱")]
        public string JobTitle { get; set; } = string.Empty;

        [Column("OfficialEmail")]
        [MaxLength(256)]
        [Description("公務Email")]
        public string OfficialEmail { get; set; } = string.Empty;

        [Column("OfficialPhone")]
        [MaxLength(50)]
        [Description("公務電話")]
        public string OfficialPhone { get; set; } = string.Empty;

        [Column("LastLoginDatetime")]
        [Description("最後登入時間")]
        public DateTime LastLoginDatetime { get; set; } = DateTime.Now;

        [Column("CreateBy")]
        [MaxLength(450)]
        [Description("建立人")]
        public string CreateBy { get; set; } = string.Empty;

        [Column("CreateDatetime")]
        [Description("建立時間")]
        public DateTime CreateDatetime { get; set; } = DateTime.Now;

        [Column("LastUpdatedBy")]
        [MaxLength(450)]
        [Description("最後更新人")]
        public string LastUpdatedBy { get; set; } = string.Empty;

        [Column("LastUpdateDatetime")]
        [Description("最後更新時間")]
        public DateTime LastUpdateDatetime { get; set; } = DateTime.Now;

        [Column("MoicaCardNumber")]
        [MaxLength(450)]
        [Description("自然人憑證卡號")]
        public string MoicaCardNumber { get; set; } = string.Empty;

        [Column("ReasonsForDisapproval")]
        [Description("不同意原因")]
        public string ReasonsForDisapproval { get; set; } = string.Empty;

        [Column("UserTenantGuid")]
        public string UserTenantGuid { get; set; } = string.Empty;

        [ForeignKey("UserTenantGuid")]
        public virtual UserTenant UserTenant { get; set; } = null!;

        // 使用 UserId 與 ApplicationUser 建立關聯
        [ForeignKey("UserId")]
        public virtual ApplicationUser ApplicationUser { get; set; } = null!;
    }
}