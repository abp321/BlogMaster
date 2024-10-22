using System.ComponentModel.DataAnnotations;

namespace BlogMaster.Models.Entities
{
    public class BlogEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        [Required]
        public string Author { get; set; } = string.Empty;

        [Required]
        public DateTime PublishedDate { get; set; }

        [Required]
        public int ViewCount { get; set; }

        public List<CommentEntity> Comments { get; set; } = [];
    }
}
