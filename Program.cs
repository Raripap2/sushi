using MyDataBase;
using Microsoft.AspNetCore.Http.Timeouts;


var builder = WebApplication.CreateBuilder(args);

// Добавляем сервисы
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Добавьте CORS сервисы
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Добавьте эту строку для поддержки MVC
builder.Services.AddControllersWithViews();

// Регистрируем вашу базу данных как сервис
builder.Services.AddSingleton<DataBase>(provider =>
{
    string connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    return new DataBase(connectionString);
});

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);

    // Настройки cookie отдельно
    options.Cookie.Name = "YourApp.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.MaxAge = TimeSpan.FromHours(8);
});

var app = builder.Build();

// ПРАВИЛЬНЫЙ ПОРЯДОК MIDDLEWARE ОЧЕНЬ ВАЖЕН!

// 1. CORS должен быть первым
app.UseCors("AllowAll");

// 2. Статические файлы
app.UseStaticFiles();

// 3. Сессия ДО вашего AuthMiddleware
app.UseSession();

// 4. Теперь ваш AuthMiddleware (после сессии)
app.UseMiddleware<AuthMiddleware>();

// 5. Остальные стандартные middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting(); // Добавьте UseRouting
app.UseAuthorization();

// 6. Маппинг endpoints
app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Login}/{id?}");

// В Program.cs (для .NET 6+)
app.Use(async (context, next) =>
{
    // Пропускаем статические файлы и некоторые маршруты
    if (context.Request.Path.StartsWithSegments("/api") &&
        !context.Request.Path.StartsWithSegments("/api/session"))
    {
        var session = context.Session;
        if (session.GetString("IsAuthenticated") == "true")
        {
            // Обновляем сессию при каждом API запросе
            session.SetInt32("UserId", session.GetInt32("UserId").Value);
            session.SetString("IsAuthenticated", "true");
            session.SetString("LastActivity", DateTime.Now.ToString());
        }
    }

    await next();
});

app.Run();