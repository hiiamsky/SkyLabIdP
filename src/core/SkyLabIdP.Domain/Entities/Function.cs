using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkyLabIdP.Domain.Entities
{
    public class Function
    {
        [Key]
        [Required]
        [MaxLength(450)]
        public string GroupID { get; set; } = string.Empty;

        [Required]
        [MaxLength(450)]
        public string FunctionID { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string FunctionIcon { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string FunctionEnglishDescription { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string FunctionChineseDescription { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string TargetRoute { get; set; } = "/";

        [Required]
        public bool IsDisabled { get; set; }

        [Required]
        public bool IsDisplayInMenu { get; set; }

        [Required]
        public int FunctionOrder { get; set; }

        // Navigation property back to the Group
        [ForeignKey("GroupID")]
        public FunctionGroup FunctionGroup { get; set; } = null!;

        public ICollection<PolicyConfiguration> PolicyConfigurations { get; set; } = [];
    }
}