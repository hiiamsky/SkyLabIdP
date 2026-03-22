using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkyLabIdP.Domain.Entities
{
    [Table("AREA")]
    public class BranchArea
    {
        [Key]
        [Column("AREAID")]
        [StringLength(4)]
        [Description("行政區編號")]
        public string AreaId { get; set; } = "";

        [Column("AREANA")]
        [StringLength(10)]
        [Description("行政區名稱")]
        public string AreaName { get; set; } = "";

        [Required]
        [Column("AREAID2")]
        [StringLength(4)]
        [Description("分部區域碼")]
        public string AreaId2 { get; set; } = "";

        [Required]
        [Column("DstCode")]
        [StringLength(2)]
        [Description("分部區域碼")]
        public string DstCode { get; set; } = "";

        [Column("ISDISPLAYED")]
        [Description("是否顯示")]
        public bool? IsDisplayed { get; set; } = false;

        [Column("RELDSTCODE")]
        [StringLength(2)]
        [Description("合併分部碼")]
        public string RelDstCode { get; set; } = "";

        [Column("CITYCODE")]
        [StringLength(1)]
        [Description("分部簡碼")]
        public string CityCode { get; set; } = "";
    }
}