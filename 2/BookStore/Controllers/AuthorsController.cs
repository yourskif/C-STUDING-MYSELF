using BookStore.Models;
using BookStore.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Threading.Tasks;

namespace BookStore.Controllers
{
    public class AuthorsController : Controller
    {
        private readonly IAuthorRepository _authorRepository;
        private readonly IBookRepository _bookRepository; // Додаємо залежність

        public AuthorsController(IAuthorRepository authorRepository, IBookRepository bookRepository)
        {
            _authorRepository = authorRepository;
            _bookRepository = bookRepository; // Ініціалізуємо
        }

        public async Task<IActionResult> Index(string? authorFilter = null)
        {
            IEnumerable<Author> authors = await _authorRepository.GetAllAsync();
            if (!string.IsNullOrEmpty(authorFilter))
            {
                authors = authors.Where(a => a.LastName.Contains(authorFilter, StringComparison.OrdinalIgnoreCase) ||
                                           a.FirstName.Contains(authorFilter, StringComparison.OrdinalIgnoreCase));
            }
            return View(authors);
        }

        public async Task<IActionResult> Details(int id)
        {
            var author = await _authorRepository.GetByIdAsync(id);
            if (author == null)
            {
                return NotFound();
            }
            return View(author);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Author author)
        {
            if (ModelState.IsValid)
            {
                await _authorRepository.AddAsync(author);
                return RedirectToAction("Index");
            }
            return View(author);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var author = await _authorRepository.GetByIdAsync(id);
            if (author == null)
            {
                return NotFound();
            }
            return View(author);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Author author)
        {
            if (id != author.Id)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                await _authorRepository.UpdateAsync(author);
                return RedirectToAction("Index");
            }
            return View(author);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var author = await _authorRepository.GetByIdAsync(id);
            if (author == null)
            {
                return NotFound();
            }
            return View(author);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _authorRepository.DeleteAsync(id);
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveBooks(int authorId, string tempBooks)
        {
            var author = await _authorRepository.GetByIdAsync(authorId);
            if (author == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(tempBooks))
            {
                var books = JsonSerializer.Deserialize<List<Book>>(tempBooks);
                if (books != null)
                {
                    foreach (var book in books)
                    {
                        book.AuthorId = authorId;
                        await _bookRepository.AddAsync(book); // Тепер _bookRepository доступний
                    }
                }
            }
            return RedirectToAction("Details", new { id = authorId });
        }
    }
}