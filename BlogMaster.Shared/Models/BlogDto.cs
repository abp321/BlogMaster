namespace BlogMaster.Shared.Models
{
    public class BlogDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public DateTime PublishedDate { get; set; }
        public int ViewCount { get; set; }
        public List<CommentDto> Comments { get; set; } = [];

        public void AddComment(CommentDto comment)
        {
            comment.PostedDate = DateTime.UtcNow;
            Comments.Add(comment);
        }
    }
}