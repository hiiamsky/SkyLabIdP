using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace SkyLabIdP.Domain.Entities;

[Table("SkyLabDevelopUserDetail")]
public class SkyLabDevelopUserDetail
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int SerialNo { get; set; }

    [Required]
    [MaxLength(450)]
    public string UserId { get; set; } = "";

    [Required]
    [MaxLength(255)]
    public string FullName { get; set; } = "";

    [MaxLength(10)]
    public string BranchCode { get; set; } = "";

    [MaxLength(10)]
    public string RegionCode { get; set; } = "";

    [MaxLength(255)]
    public string DepartmentName { get; set; } = "";

    [Required]
    [MaxLength(255)]
    public string SubordinateUnit { get; set; } = "";

    [Required]
    [MaxLength(255)]
    public string JobTitle { get; set; } = "";

    [Required]
    [MaxLength(256)]
    public string OfficialEmail { get; set; } = "";

    [Required]
    [MaxLength(50)]
    public string OfficialPhone { get; set; } = "";

    [Required]
    [MaxLength(450)]
    public string CreateBy { get; set; } = "";

    [Required]
    public DateTime CreateDatetime { get; set; } = DateTime.Now;

    [Required]
    public DateTime LastLoginDatetime { get; set; } = DateTime.Now;

    [Required]
    public DateTime LastUpdateDatetime { get; set; } = DateTime.Now;

    [Required]
    [MaxLength(450)]
    public string LastUpdatedBy { get; set; } = "";

    [Required]
    [MaxLength(450)]
    public string UserTenantGuid { get; set; } = "";

    [ForeignKey("UserTenantGuid")]
    public virtual UserTenant UserTenant { get; set; } = null!;
    // 使用 UserId 與 ApplicationUser 建立關聯
    [ForeignKey("UserId")]
    public virtual ApplicationUser ApplicationUser { get; set; } = null!;
}
