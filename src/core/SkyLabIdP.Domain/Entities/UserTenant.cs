using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkyLabIdP.Domain.Entities;
[Table("UserTenant")]
public class UserTenant
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("SerialNo")]
    [Description("流水號（自動加一）")]
    public int SerialNo { get; set; }

    [Column("TenantGuid")]
    [Description("租戶Guid")]
    public string TenantGuid { get; set; } = Guid.NewGuid().ToString();

    [Column("UserId")]
    [MaxLength(450)]
    [Description("使用者Id")]
    public string UserId { get; set; } = string.Empty;

    [Column("TenantId")]
    [MaxLength(50)]
    [Description("租戶類別編號")]
    public string TenantId { get; set; } = string.Empty;

    [Column("CreateDateTime")]
    [Description("建立日期時間")]
    public DateTime CreateDateTime { get; set; } = DateTime.Now;

    [ForeignKey("UserId")]
    public virtual ApplicationUser User { get; set; } = null!;

    public virtual SkyLabDocUserDetail? SkyLabDocUserDetail { get; set; }
    public virtual SkyLabDevelopUserDetail? SkyLabDevelopUserDetail { get; set; }
}
