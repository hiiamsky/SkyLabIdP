using System.ComponentModel.DataAnnotations;

namespace SkyLabIdP.Domain.Entities
{
    public class FunctionGroup
    {

        [Key]
        [Required]
        [MaxLength(450)]
        public string GroupID { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string GroupIcon { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string GroupTitle { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string GroupEnglishDescription { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string GroupChineseDescription { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string TargetRoute { get; set; } = string.Empty;

        [Required]
        public bool IsDisabled { get; set; }


        [Required]
        public bool IsOpenFunctionList { get; set; }

        [Required]
        public int GroupOrder { get; set; }

        public ICollection<Function> Functions { get; set; } = [];

    }
}