using Microsoft.EntityFrameworkCore;
using BookStore.Data;
using BookStore.Models;

var builder = WebApplication.CreateBuilder(args);

// Додаємо підтримку контролерів з видачею представлень
builder.Services.AddControllersWithViews();

// Використовуємо InMemoryDatabase замість реальної SQL бази
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseInMemoryDatabase("TestDb"));  // "TestDb" - ім'я для бази в пам'яті

var app = builder.Build();

// Використовуємо статичні файли (CSS, JS, зображення і т.д.)
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

// Налаштовуємо маршрут для контролера
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Authors}/{action=Create}/{id?}");

app.Run();





//using Microsoft.EntityFrameworkCore;
//using BookStore.Data;
//using BookStore.Models;


//var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddControllersWithViews();
//builder.Services.AddDbContext<ApplicationDbContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//var app = builder.Build();

//    app.UseStaticFiles();
//app.UseRouting();
//app.UseAuthorization();

//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Authors}/{action=Create}/{id?}");

//app.Run();


