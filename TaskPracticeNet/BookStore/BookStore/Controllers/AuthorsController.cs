using BookStore.Models;
using BookStore.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging; // Додано для логування
using System.Text.Json;
using System.Threading.Tasks;

namespace BookStore.Controllers
{
    public class AuthorsController : Controller
    {
        private readonly IAuthorRepository _authorRepository;
        private readonly IBookRepository _bookRepository;
        private readonly ILogger<AuthorsController> _logger; // Додано логгер

        public AuthorsController(IAuthorRepository authorRepository, IBookRepository bookRepository, ILogger<AuthorsController> logger)
        {
            _authorRepository = authorRepository;
            _bookRepository = bookRepository;
            _logger = logger; // Ініціалізуємо логгер
        }

        public async Task<IActionResult> Index(string? authorFilter = null)
        {
            _logger.LogInformation("Fetching authors with filter: {AuthorFilter}", authorFilter);
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
            _logger.LogInformation("Fetching details for author ID: {AuthorId}", id);
            var author = await _authorRepository.GetByIdAsync(id);
            if (author == null)
            {
                _logger.LogWarning("Author with ID {AuthorId} not found", id);
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
            _logger.LogInformation("Attempting to create author: {@Author}", author);
            if (ModelState.IsValid)
            {
                await _authorRepository.AddAsync(author);
                _logger.LogInformation("Author created successfully with ID: {AuthorId}", author.Id);
                return RedirectToAction("Index");
            }
            _logger.LogWarning("ModelState is invalid for author creation: {@Author}", author);
            return View(author);
        }

        public async Task<IActionResult> Edit(int id)
        {
            _logger.LogInformation("Fetching author with ID {AuthorId} for editing", id);
            var author = await _authorRepository.GetByIdAsync(id);
            if (author == null)
            {
                _logger.LogWarning("Author with ID {AuthorId} not found for editing", id);
                return NotFound();
            }
            return View(author);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Author author)
        {
            _logger.LogInformation("Attempting to edit author with ID {AuthorId}: {@Author}", id, author);
            if (id != author.Id)
            {
                _logger.LogWarning("Author ID mismatch: {ProvidedId} vs {AuthorId}", id, author.Id);
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                await _authorRepository.UpdateAsync(author);
                _logger.LogInformation("Author with ID {AuthorId} updated successfully", author.Id);
                return RedirectToAction("Index");
            }
            _logger.LogWarning("ModelState is invalid for author editing: {@Author}", author);
            return View(author);
        }

        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation("Fetching author with ID {AuthorId} for deletion", id);
            var author = await _authorRepository.GetByIdAsync(id);
            if (author == null)
            {
                _logger.LogWarning("Author with ID {AuthorId} not found for deletion", id);
                return NotFound();
            }
            return View(author);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            _logger.LogInformation("Attempting to delete author with ID {AuthorId}", id);
            await _authorRepository.DeleteAsync(id);
            _logger.LogInformation("Author with ID {AuthorId} deleted successfully", id);
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveBooks(int authorId, string tempBooks)
        {
            _logger.LogInformation("Attempting to save books for author ID {AuthorId} with temp data: {TempBooks}", authorId, tempBooks);
            var author = await _authorRepository.GetByIdAsync(authorId);
            if (author == null)
            {
                _logger.LogWarning("Author with ID {AuthorId} not found for saving books", authorId);
                return NotFound();
            }

            if (!string.IsNullOrEmpty(tempBooks))
            {
                try
                {
                    var books = JsonSerializer.Deserialize<List<Book>>(tempBooks);
                    if (books != null)
                    {
                        foreach (var book in books)
                        {
                            book.AuthorId = authorId;
                            if (ModelState.IsValid)
                            {
                                _logger.LogInformation("Saving book: {@Book}", book);
                                await _bookRepository.AddAsync(book);
                            }
                            else
                            {
                                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                                _logger.LogError("Validation errors for book: {@Errors}", errors);
                                return Json(new { success = false, errors = errors });
                            }
                        }
                    }
                    _logger.LogInformation("Books saved successfully for author ID {AuthorId}", authorId);
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Error deserializing books for author ID {AuthorId}", authorId);
                    return Json(new { success = false, errors = new[] { "Помилка при обробці даних книг: " + ex.Message } });
                }
            }
            return RedirectToAction("Details", new { id = authorId });
        }
    }
}