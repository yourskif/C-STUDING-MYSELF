using BookStore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookStore.Data;
using BookStore.Models;

namespace BookStore.Controllers
{
    public class AuthorsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AuthorsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Author author)
        {
            if (ModelState.IsValid)
            {
                _context.Authors.Add(author);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(author);
        }
    }
}
