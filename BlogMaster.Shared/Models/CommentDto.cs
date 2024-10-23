namespace BlogMaster.Shared.Models
{
    public class CommentDto
    {
        private string _author = string.Empty;
        private string _content = string.Empty;

        public int Id { get; set; }

        public string Author
        {
            get => _author;
            set => _author = value.Length > 30 ? value[..30] : value;
        }

        public string Content
        {
            get => _content;
            set => _content = value.Length > 500 ? value[..500] : value;
        }

        public DateTime PostedDate { get; set; }
        public int BlogPostId { get; set; }
    }
}
