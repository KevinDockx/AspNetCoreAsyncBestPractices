using Books.Api.Contexts;
using Books.Api.Entities;
using Books.Api.ExternalModels;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Books.Api.Services
{
    public class BooksRepository : IBooksRepository 
    {  
        private readonly BooksContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<BooksRepository> _logger;
        private CancellationTokenSource _cancellationTokenSource = null!;

        public BooksRepository(BooksContext context, 
            IHttpClientFactory httpClientFactory, 
            ILogger<BooksRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Book?> GetBookAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(id));
            }

            return await _context.Books.Include(b => b.Author)
                .FirstOrDefaultAsync(b => b.Id == id);
        }
  
        public void AddBook(Book bookToAdd)
        {
            if (bookToAdd == null)
            {
                throw new ArgumentNullException(nameof(bookToAdd));
            }

            _context.Add(bookToAdd);
        } 
 
        public async Task<bool> SaveChangesAsync()
        {
            return (await _context.SaveChangesAsync() > 0);
        } 

        public async Task<IEnumerable<BookCover>> GetBookCoversAsync(Guid bookId)
        {
            var httpClient = _httpClientFactory.CreateClient();
            var bookCovers = new List<BookCover>();
            _cancellationTokenSource = new CancellationTokenSource();

            // create a list of fake bookcovers
            var bookCoverUrls = new[]
            {
                $"http://localhost:52644/api/bookcovers/{bookId}-dummycover1",
                $"http://localhost:52644/api/bookcovers/{bookId}-dummycover2?returnFault=true",
                $"http://localhost:52644/api/bookcovers/{bookId}-dummycover3",
                $"http://localhost:52644/api/bookcovers/{bookId}-dummycover4",
                $"http://localhost:52644/api/bookcovers/{bookId}-dummycover5"
            };

            // foreach + await will run them in order.  We prefer parallel.

            // create the tasks
            var downloadBookCoverTasksQuery =
                 from bookCoverUrl
                 in bookCoverUrls
                 select DownloadBookCoverAsync(
                     httpClient,
                     bookCoverUrl,
                     _cancellationTokenSource.Token);

            // start the tasks
            var downloadBookCoverTasks = downloadBookCoverTasksQuery.ToList();

            try
            {
                // get the results
                var results = await Task.WhenAll(downloadBookCoverTasks);
                // return results that aren't null
                return results.Where(r => r != null);
            }
            catch (OperationCanceledException operationCanceledException)
            {
                _logger.LogInformation($"{operationCanceledException.Message}");
                foreach (var task in downloadBookCoverTasks)
                {
                    _logger.LogInformation($"Task {task.Id} has status {task.Status}");
                }

                return new List<BookCover>();
            }
            catch (Exception exception)
            {
                _logger.LogError($"{exception.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<BookCover>> GetBookCoversWithoutCancellationWithForEachAsync(Guid bookId)
        {
            var httpClient = _httpClientFactory.CreateClient();
            var bookCovers = new List<BookCover>();

            // create a list of fake bookcovers
            var bookCoverUrls = new[]
            {
                $"http://localhost:52644/api/bookcovers/{bookId}-dummycover1",
                $"http://localhost:52644/api/bookcovers/{bookId}-dummycover2?returnFault=true",
                $"http://localhost:52644/api/bookcovers/{bookId}-dummycover3",
                $"http://localhost:52644/api/bookcovers/{bookId}-dummycover4",
                $"http://localhost:52644/api/bookcovers/{bookId}-dummycover5"
            };

            // foreach + await will run them in order.  
            foreach (var bookCoverUrl in bookCoverUrls)
            {
                var cover = await DownloadBookCoverWithoutCancellationSupportAsync(
                    httpClient, bookCoverUrl);

                if (cover != null)
                {
                    bookCovers.Add(cover);
                }
            }

            return bookCovers;
        }

        public async Task<IEnumerable<BookCover>> GetBookCoversWithoutCancellationAsync(Guid bookId)
        {
            var httpClient = _httpClientFactory.CreateClient();
            var bookCovers = new List<BookCover>();

            // create a list of fake bookcovers
            var bookCoverUrls = new[]
            {
                $"http://localhost:52644/api/bookcovers/{bookId}-dummycover1",
                $"http://localhost:52644/api/bookcovers/{bookId}-dummycover2?returnFault=true",
                $"http://localhost:52644/api/bookcovers/{bookId}-dummycover3",
                $"http://localhost:52644/api/bookcovers/{bookId}-dummycover4",
                $"http://localhost:52644/api/bookcovers/{bookId}-dummycover5"
            };

            // foreach + await will run them in order.  We prefer parallel.  
            // create the tasks
            var downloadBookCoverTasksQuery =
                 from bookCoverUrl
                 in bookCoverUrls
                 select DownloadBookCoverWithoutCancellationSupportAsync(
                     httpClient, bookCoverUrl);

            // start the tasks
            var downloadBookCoverTasks = downloadBookCoverTasksQuery.ToList();
            // get the results
            var results = await Task.WhenAll(downloadBookCoverTasks);
            // return results that aren't null
            return results.Where(r => r != null);
        }

        /// <summary>
        /// Download book cover without cancellation support
        /// </summary> 
        private async Task<BookCover?> DownloadBookCoverWithoutCancellationSupportAsync(
            HttpClient httpClient,
            string bookCoverUrl)
        {
            var response = await httpClient
                       .GetAsync(bookCoverUrl);

            if (response.IsSuccessStatusCode)
            {
                var bookCover = JsonSerializer.Deserialize<BookCover>(
                    await response.Content.ReadAsStringAsync(),
                    new JsonSerializerOptions()
                    { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                return bookCover;
            }

            return null;
        }

        /// <summary>
        /// Download book cover with cancellation support
        /// </summary> 
        private async Task<BookCover?> DownloadBookCoverAsync(
            HttpClient httpClient,
            string bookCoverUrl,
            CancellationToken cancellationToken)
        {
            var response = await httpClient
                       .GetAsync(bookCoverUrl, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var bookCover = JsonSerializer.Deserialize<BookCover>(
                    await response.Content.ReadAsStringAsync(cancellationToken),
                    new JsonSerializerOptions()
                    { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                return bookCover;
            }

            _cancellationTokenSource.Cancel();

            return null;
        }
    }
}
