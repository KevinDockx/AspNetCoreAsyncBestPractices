using System;
using System.Threading.Tasks;

namespace Books.Api.Services
{
    public interface IBooksRepository
    {        
        Task<Entities.Book> GetBookAsync(Guid id);

        void AddBook(Entities.Book bookToAdd);

        Task<bool> SaveChangesAsync();
    }
}
