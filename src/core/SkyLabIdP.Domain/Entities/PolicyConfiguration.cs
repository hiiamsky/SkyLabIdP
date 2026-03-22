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
        public string PolicyId { get; set; }

        public string PolicyDescription { get; set; }

        [Required]
        public string ClaimType { get; set; }

        [Required]
        public string ClaimValue { get; set; }

        // 外鍵指向 Functions 表
        public string FunctionID { get; set; }

        // 導航屬性
        [ForeignKey("FunctionID")]
        public Function Function { get; set; }
    }
}

