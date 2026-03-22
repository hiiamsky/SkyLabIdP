using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkyLabIdP.Domain.Entities
{
    public class FileUpload
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SerialNo { get; set; }
        public string FileId { get; set; } = "";
        public string OriginalFileName { get; set; } = "";
        public string FileExtension { get; set; } = "";
        public string FileSystemType { get; set; } = "";

        public string? FileDescription { get; set; }
        public string ApacheTikaContent { get; set; } = "";
        public bool IsDisabled { get; set; } = false;
        public string Comments { get; set; } = "";
        public string CreatorId { get; set; } = "";
        public DateTime CreatedTime { get; set; } = DateTime.Now;
    }
}


