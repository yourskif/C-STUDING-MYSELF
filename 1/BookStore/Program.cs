using BookStore.Data;
using BookStore.Models;
using BookStore.Repositories.Interfaces;
using BookStore.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ������ �������� ���������� � ������� ������������
builder.Services.AddControllersWithViews();

// ������������ ��������� ���� ����� � InMemoryDatabase
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseInMemoryDatabase("BookStore"));

// ��������� ����������
builder.Services.AddScoped<IAuthorRepository, AuthorRepository>();
builder.Services.AddScoped<IBookRepository, BookRepository>();

var app = builder.Build();

// ������ �������� ��� ��� InMemoryDatabase
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (!dbContext.Authors.Any())
    {
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