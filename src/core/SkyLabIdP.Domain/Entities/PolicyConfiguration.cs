using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace SkyLabIdP.Domain.Entities
{
    public class PolicyConfiguration
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SerialNo { get; set; }

        [Required]
        public string PolicyId { get; set; } = string.Empty;

        public string PolicyDescription { get; set; } = string.Empty;

        [Required]
        public string ClaimType { get; set; } = string.Empty;

        [Required]
        public string ClaimValue { get; set; } = string.Empty;

        // 外鍵指向 Functions 表
        public string FunctionID { get; set; } = string.Empty;

        // 導航屬性
        [ForeignKey("FunctionID")]
        public Function Function { get; set; } = null!;
    }
}

