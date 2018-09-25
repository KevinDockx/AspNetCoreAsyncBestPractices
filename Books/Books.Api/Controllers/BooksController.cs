using AutoMapper;
using Books.Api.Models;
using Books.Api.Services;
using Books.Legacy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Books.Api.Controllers
{
    [Route("api/books")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly IBooksRepository _booksRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<BooksController> _logger;
        private readonly ComplicatedPageCalculator _complicatedPageCalculator;

        public BooksController(IBooksRepository booksRepository,
            IMapper mapper,  ILogger<BooksController> logger,
            ComplicatedPageCalculator complicatedPageCalculator)
        {
            _booksRepository = booksRepository 
                ?? throw new ArgumentNullException(nameof(booksRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _complicatedPageCalculator = complicatedPageCalculator ?? 
                throw new ArgumentNullException(nameof(complicatedPageCalculator)); 
        }
 
        [HttpGet]
        [Route("{id}", Name = "GetBook")]
        public async Task<IActionResult> GetBook(Guid id)
        {
            var result = _booksRepository.GetBookAsync(id).Result;

            var bookEntity = await _booksRepository.GetBookAsync(id);
            if (bookEntity == null)
            {
                return NotFound();
            }

            // calculate the book pages 
            // DON'T DO THIS, this is sample code for a bad practice!
            _logger.LogInformation($"ThreadId when entering GetBook: " +
                $"{System.Threading.Thread.CurrentThread.ManagedThreadId}");
            var bookPages = await GetBookPages(id);

            // good way to call legacy computational-bound code is by NOT offloading it to the background
            //var bookPages = _complicatedPageCalculator.CalculateBookPages(id);

            return Ok(_mapper.Map<Models.Book>(bookEntity));
        }

        /// <summary>
        /// Sample of how to call sync code "async" - don't do this on the server, this is 
        /// sample code of a bad practice!
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private Task<int> GetBookPages(Guid id)
        {
            return Task.Run(() =>
            {
                _logger.LogInformation($"ThreadId when calculating the amount of pages: " +
                    $"{System.Threading.Thread.CurrentThread.ManagedThreadId}");

                return _complicatedPageCalculator.CalculateBookPages(id);
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateBook([FromBody] BookForCreation book)
        {
            var bookEntity = _mapper.Map<Entities.Book>(book);
            _booksRepository.AddBook(bookEntity);

            await _booksRepository.SaveChangesAsync();

            // Fetch (refetch) the book from the data store, including the author
            await _booksRepository.GetBookAsync(bookEntity.Id);

            return CreatedAtRoute("GetBook",
                new { id = bookEntity.Id },
                _mapper.Map<Models.Book>(bookEntity));
        }
    }
}
