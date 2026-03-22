using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkyLabIdP.Domain.Entities
{
    [Table("PasswordHistory")]
    public class PasswordHistory
    {
        [Key]
        public int SerialNo { get; set; }
        public string UserId { get; set; }
        public string HashedPassword { get; set; }
        public string PasswordSalt { get; set; }
        public DateTime PasswordChangeDate { get; set; }

        public ApplicationUser User { get; set; }
    }
}
