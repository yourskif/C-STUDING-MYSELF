using BookStore.Models;
using BookStore.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging; // Додано для логування
using System.Threading.Tasks;
using System.Linq;

namespace BookStore.Controllers
{
    public class BooksController : Controller
    {
        private readonly IBookRepository _bookRepository;
        private readonly IAuthorRepository _authorRepository;
        private readonly ILogger<BooksController> _logger; // Додано логгер

        public BooksController(IBookRepository bookRepository, IAuthorRepository authorRepository, ILogger<BooksController> logger)
        {
            _bookRepository = bookRepository;
            _authorRepository = authorRepository;
            _logger = logger; // Ініціалізуємо логгер
        }

        public async Task<IActionResult> Index(int authorId, string? genreFilter = null)
        {
            var books = await _bookRepository.GetBooksByAuthorIdAsync(authorId);
            if (!string.IsNullOrEmpty(genreFilter))
            {
                books = books.Where(b => b.Genre.ToString().Contains(genreFilter, StringComparison.OrdinalIgnoreCase));
            }
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

        public async Task<IActionResult> ByGenre(GenreEnum genre, string? authorFilter = null)
        {
            var books = await _bookRepository.GetAllAsync();
            var filteredBooks = books.Where(b => b.Genre == genre);
            if (!string.IsNullOrEmpty(authorFilter))
            {
                filteredBooks = filteredBooks.Where(b => b.Author.LastName.Contains(authorFilter, StringComparison.OrdinalIgnoreCase) ||
                                                       b.Author.FirstName.Contains(authorFilter, StringComparison.OrdinalIgnoreCase));
            }
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
            _logger.LogInformation("Attempting to create book: {@Book}", book);
            if (ModelState.IsValid)
            {
                try
                {
                    await _bookRepository.AddAsync(book);
                    _logger.LogInformation("Book created successfully with ID: {BookId}", book.Id);
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { id = book.Id, success = true });
                    }
                    return RedirectToAction("Index", new { authorId = book.AuthorId });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error adding book: {@Book}", book);
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, errors = new[] { "Помилка при додаванні книги: " + ex.Message } });
                    }
                    throw;
                }
            }
            _logger.LogWarning("ModelState is invalid for book creation: {@Book}", book);
            ViewBag.Author = await _authorRepository.GetByIdAsync(book.AuthorId);
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return Json(new { success = false, errors = errors });
            }
            return View(book);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var book = await _bookRepository.GetByIdAsync(id);
            if (book == null)
            {
                return NotFound();
            }
            ViewBag.Author = await _authorRepository.GetByIdAsync(book.AuthorId);
            return View(book);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Book book)
        {
            if (id != book.Id)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                await _bookRepository.UpdateAsync(book);
                return RedirectToAction("Index", new { authorId = book.AuthorId });
            }
            ViewBag.Author = await _authorRepository.GetByIdAsync(book.AuthorId);
            return View(book);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var book = await _bookRepository.GetByIdAsync(id);
            if (book == null)
            {
                return NotFound();
            }
            return View(book);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var book = await _bookRepository.GetByIdAsync(id);
            if (book != null)
            {
                await _bookRepository.DeleteAsync(id);
                return RedirectToAction("Index", new { authorId = book.AuthorId });
            }
            return NotFound();
        }
    }
}