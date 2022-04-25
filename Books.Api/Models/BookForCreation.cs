namespace Books.Api.Models
{
    public class BookForCreation
    {
        public Guid AuthorId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
    }
}
