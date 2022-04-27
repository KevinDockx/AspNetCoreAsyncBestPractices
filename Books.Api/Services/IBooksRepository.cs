using Books.Api.Entities;
using Books.Api.ExternalModels; 

namespace Books.Api.Services
{
    public interface IBooksRepository
    {
        Task<Book?> GetBookAsync(Guid id);
        void AddBook(Book bookToAdd);
        Task<bool> SaveChangesAsync();
        Task<IEnumerable<BookCover>> GetBookCoversWithoutCancellationWithForEachAsync(Guid bookId);
        Task<IEnumerable<BookCover>> GetBookCoversWithoutCancellationAsync(Guid bookId);
        Task<IEnumerable<BookCover>> GetBookCoversAsync(Guid bookId);
        IAsyncEnumerable<Book> GetBooksAsAsyncEnumerable();

    }
}