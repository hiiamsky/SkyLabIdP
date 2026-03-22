using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkyLabIdP.Domain.Entities;

[Table("Branch")]
public class Branch
{
    [Key]
    [MaxLength(10)]
    public string BranchCode { get; set; } = "";

    [MaxLength(255)]
    public string BranchName { get; set; } = "";

    [MaxLength(10)]
    public string RegionCode { get; set; } = "";
}
