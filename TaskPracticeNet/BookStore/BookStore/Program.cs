using Microsoft.EntityFrameworkCore;
using BookStore.Data;
using BookStore.Models;
using BookStore.Repositories;
using BookStore.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ������������ Serilog ��� ��������� � ���� �� �������
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("logs/bookstore.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog(); // ������������ Serilog ��� �����

// ������ �������� ���������� � ������� ������������
builder.Services.AddControllersWithViews();

// ������������ ��������� ����� ILogger
builder.Services.AddLogging(logging =>
{
    logging.AddConsole(); // ���� ���������� � �������
    logging.SetMinimumLevel(LogLevel.Debug); // ������������ ��������� ����� ��������� (Debug ��� ����������)
});

// ������������ ��������� ���� ����� � InMemoryDatabase
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseInMemoryDatabase("BookStore")
           .LogTo(Console.WriteLine, LogLevel.Information); // ��������� EF Core
});

// ��������� ����������
builder.Services.AddScoped<IAuthorRepository, AuthorRepository>();
builder.Services.AddScoped<IBookRepository, BookRepository>();

var app = builder.Build();

// ������������ ������ ��� �������
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Application starting...");

// ������ �������� ��� ��� InMemoryDatabase
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (!dbContext.Authors.Any())
    {
        logger.LogInformation("Initializing sample data...");
        var author = new Author
        {
            FirstName = "����",
            LastName = "�����",
            BirthDate = DateTime.Now
        };
        dbContext.Authors.Add(author);

        dbContext.Books.Add(new Book
        {
            Title = "������� �����",
            PageCount = 200,
            Genre = GenreEnum.Fiction,
            AuthorId = author.Id
        });
        dbContext.SaveChanges();
        logger.LogInformation("Sample data initialized successfully.");
    }
}

// ������������ pipeline
app.UseExceptionHandler("/Home/Error");
app.UseStaticFiles();
app.UseRouting();

// ������������ ��������
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