using Microsoft.EntityFrameworkCore;
using BookStore.Data;
using BookStore.Models;
using BookStore.Repositories;
using BookStore.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Налаштування Serilog для логування у файл та консоль
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("logs/bookstore.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog(); // Налаштування Serilog для хосту

// Додаємо підтримку контролерів з видачею представлень
builder.Services.AddControllersWithViews();

// Налаштування логування через ILogger
builder.Services.AddLogging(logging =>
{
    logging.AddConsole(); // Логи виводяться в консоль
    logging.SetMinimumLevel(LogLevel.Debug); // Встановлюємо мінімальний рівень логування (Debug для детальності)
});

// Налаштування контексту бази даних з InMemoryDatabase
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseInMemoryDatabase("BookStore")
           .LogTo(Console.WriteLine, LogLevel.Information); // Логування EF Core
});

// Реєстрація репозиторіїв
builder.Services.AddScoped<IAuthorRepository, AuthorRepository>();
builder.Services.AddScoped<IBookRepository, BookRepository>();

var app = builder.Build();

// Налаштування логера для додатка
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Application starting...");

// Додаємо початкові дані для InMemoryDatabase
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (!dbContext.Authors.Any())
    {
        logger.LogInformation("Initializing sample data...");
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
        logger.LogInformation("Sample data initialized successfully.");
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