namespace BlogMaster.Shared.Models
{
    public class CommentDto
    {
        public int Id { get; set; }
        public string Author { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime PostedDate { get; set; }
        public int BlogPostId { get; set; } 
    }
}
