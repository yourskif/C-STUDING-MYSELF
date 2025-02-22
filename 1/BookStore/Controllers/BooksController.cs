using BookStore.Models;
using BookStore.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BookStore.Controllers
{
    public class BooksController : Controller
    {
        private readonly IBookRepository _bookRepository;
        private readonly IAuthorRepository _authorRepository;

        public BooksController(IBookRepository bookRepository, IAuthorRepository authorRepository)
        {
            _bookRepository = bookRepository;
            _authorRepository = authorRepository;
        }

        public async Task<IActionResult> Index(int authorId)
        {
            var books = await _bookRepository.GetBooksByAuthorIdAsync(authorId);
            return View(books);
        }

        public async Task<IActionResult> Details(int id)
        {
            var book = await _bookRepository.GetByIdAsync(id);
            if (book == null)
            {
                return NotFound();
            }
            return View(book);
        }

        public async Task<IActionResult> ByGenre(GenreEnum genre)
        {
            var books = await _bookRepository.GetAllAsync();
            var filteredBooks = books.Where(b => b.Genre == genre);
            return View(filteredBooks);
        }

        public async Task<IActionResult> Create(int authorId)
        {
            ViewBag.Author = await _authorRepository.GetByIdAsync(authorId);
            return View(new Book { AuthorId = authorId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Book book)
        {
            if (ModelState.IsValid)
            {
                await _bookRepository.AddAsync(book);
                return RedirectToAction("Index", new { authorId = book.AuthorId });
            }
            ViewBag.Author = await _authorRepository.GetByIdAsync(book.AuthorId);
            return View(book);
        }
    }
}