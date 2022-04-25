using System;

namespace Books.Api.Models
{
    public class Book
    {
        public Guid Id { get; set; }

        public string Author { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

    }
}
