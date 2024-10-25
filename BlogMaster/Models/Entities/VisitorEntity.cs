using System.ComponentModel.DataAnnotations;

namespace BlogMaster.Models.Entities
{
    public class VisitorEntity
    {
        [Key]
        public string IpAddress { get; set; } = string.Empty;

        [Required]
        public DateTime LastVisit { get; set; }

        [Required]
        public int VisitCount { get; set; }
    }
}
