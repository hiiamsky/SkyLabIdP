using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkyLabIdP.Domain.Entities
{

    public class SysCode
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("SerialNo")]
        public long SerialNo { get; set; }

        [Required]
        [Column("Type")]
        [StringLength(100)]
        public string Type { get; set; } = "";

        [Required]
        [Column("Code")]
        [StringLength(100)]
        public string Code { get; set; } = "";

        [Required]
        [Column("Desc")]
        [StringLength(100)]
        public string Description { get; set; } = "";

        [Column("Item1")]
        [StringLength(100)]
        public string Item1 { get; set; } = "";

        [Column("Item2")]
        [StringLength(100)]
        public string Item2 { get; set; } = "";

        [Column("Item3")]
        [StringLength(100)]
        public string Item3 { get; set; } = "";

        [Column("Item4")]
        [StringLength(100)]
        public string Item4 { get; set; } = "";

        [Column("Item5")]
        [StringLength(100)]
        public string Item5 { get; set; } = "";

        [Column("Item6")]
        [StringLength(100)]
        public string Item6 { get; set; } = "";

        [Column("Item7")]
        [StringLength(100)]
        public string Item7 { get; set; } = "";

        [Column("Item8")]
        [StringLength(100)]
        public string Item8 { get; set; } = "";

        [Column("Item9")]
        [StringLength(100)]
        public string Item9 { get; set; } = "";

        [Column("Item10")]
        [StringLength(100)]
        public string Item10 { get; set; } = "";

        [Column("Item11")]
        [StringLength(100)]
        public string Item11 { get; set; } = "";

        [Column("Item12")]
        [StringLength(100)]
        public string Item12 { get; set; } = "";

        [Column("Item13")]
        [StringLength(100)]
        public string Item13 { get; set; } = "";

        [Column("Item14")]
        [StringLength(100)]
        public string Item14 { get; set; } = "";

        [Column("Item15")]
        [StringLength(100)]
        public string Item15 { get; set; } = "";

        [Column("Item16")]
        [StringLength(100)]
        public string Item16 { get; set; } = "";

        [Column("Item17")]
        [StringLength(100)]
        public string Item17 { get; set; } = "";

        [Column("Item18")]
        [StringLength(100)]
        public string Item18 { get; set; } = "";

        [Column("Item19")]
        [StringLength(100)]
        public string Item19 { get; set; } = "";

        [Column("Item20")]
        [StringLength(100)]
        public string Item20 { get; set; } = "";

        [Required]
        [Column("StopTag")]
        public bool StopTag { get; set; } = false;

        [Required]
        [Column("Ord")]
        [StringLength(50)]
        public string Ord { get; set; } = "";

        [Column("createBy")]
        [StringLength(450)]
        public string CreateBy { get; set; } = "";

        [Column("createDate")]
        public DateTime? CreateDate { get; set; } = DateTime.Now;

        [Column("LastUpdateBy")]
        [StringLength(450)]
        public string LastUpdateBy { get; set; } = "";

        [Column("LastUpdateDate")]
        public DateTime? LastUpdateDate { get; set; } = DateTime.Now;
    }
}

