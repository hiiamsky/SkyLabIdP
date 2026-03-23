using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkyLabIdP.Domain.Entities
{
    [Table("PasswordHistory")]
    public class PasswordHistory
    {
        [Key]
        public int SerialNo { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string HashedPassword { get; set; } = string.Empty;
        public string PasswordSalt { get; set; } = string.Empty;
        public DateTime PasswordChangeDate { get; set; }

        public ApplicationUser User { get; set; } = null!;
    }
}
