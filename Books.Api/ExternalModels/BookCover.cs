namespace Books.Api.ExternalModels
{
    public class BookCover
    {
        public string Name { get; set; } = string.Empty;
        public byte[] Content { get; set; } = null!;
    }
}
