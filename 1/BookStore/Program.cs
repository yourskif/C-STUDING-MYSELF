using BookStore.Data;
using BookStore.Models;
using BookStore.Repositories.Interfaces;
using BookStore.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Додаємо підтримку контролерів з видачею представлень
builder.Services.AddControllersWithViews();

// Налаштування контексту бази даних з InMemoryDatabase
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseInMemoryDatabase("BookStore"));

// Реєстрація репозиторіїв
builder.Services.AddScoped<IAuthorRepository, AuthorRepository>();
builder.Services.AddScoped<IBookRepository, BookRepository>();

var app = builder.Build();

// Додаємо початкові дані для InMemoryDatabase
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (!dbContext.Authors.Any())
    {
        var author = new Author
        {
            FirstName = "Тест",
            LastName = "Автор",
            BirthDate = DateTime.Now
        };
        dbContext.Authors.Add(author);

        dbContext.Books.Add(new Book
        {
            Title = "Тестова книга",
            PageCount = 200,
            Genre = GenreEnum.Fiction,
            AuthorId = author.Id
        });
        dbContext.SaveChanges();
    }
}

// Налаштування pipeline
app.UseExceptionHandler("/Home/Error");
app.UseStaticFiles();
app.UseRouting();

// Налаштування маршрутів
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Authors}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "booksByGenre",
    pattern: "Books/ByGenre/{genre}",
    defaults: new { controller = "Books", action = "ByGenre" });

app.MapControllerRoute(
    name: "error",
    pattern: "Home/Error",
    defaults: new { controller = "Home", action = "Error" });

app.Run();