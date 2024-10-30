namespace BlogMaster.Shared.Models
{
    public class BlogDto
    {
        private string _author = string.Empty;
        private string _content = string.Empty;
        private string _title = string.Empty;

        public int Id { get; set; }

        public string Author
        {
            get => _author;
            set => _author = value.Length > 30 ? value[..30] : value;
        }

        public string Content
        {
            get => _content;
            set => _content = value.Length > 20000 ? value[..20000] : value;
        }

        public string Title
        {
            get => _title;
            set => _title = value.Length > 100 ? value[..100] : value;
        }

        public DateTime PublishedDate { get; set; }
        public int ViewCount { get; set; }
        public List<CommentDto> Comments { get; set; } = [];

        public void AddComment(CommentDto comment)
        {
            comment.PostedDate = DateTime.UtcNow;
            Comments.Add(comment);
        }

        public override bool Equals(object? obj)
        {
            if (obj is not BlogDto other)
                return false;

            return Id == other.Id &&
                   Author == other.Author &&
                   Content == other.Content &&
                   Title == other.Title &&
                   PublishedDate == other.PublishedDate &&
                   ViewCount == other.ViewCount &&
                   Comments.SequenceEqual(other.Comments);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Author, Content, Title, PublishedDate, ViewCount, Comments);
        }

        public bool Valid()
        {
            return !string.IsNullOrWhiteSpace(Title) && !string.IsNullOrWhiteSpace(Author) && !string.IsNullOrWhiteSpace(Content);
        }
    }
}