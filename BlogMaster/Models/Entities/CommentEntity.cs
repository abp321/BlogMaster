using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BlogMaster.Models.Entities
{
    public class CommentEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Author { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        [Required]
        public DateTime PostedDate { get; set; }

        [Required]
        [ForeignKey("Blog")]
        public int BlogPostId { get; set; }

        public BlogEntity Blog { get; set; }
    }
}
