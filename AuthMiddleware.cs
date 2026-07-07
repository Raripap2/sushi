public class AuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly List<string> _allowedPaths = new()
    {
        "/api/Users/aut",       // ← ИСПРАВЛЕНО! Полный путь к авторизации
        "/api/Users/logout",    // ← Добавьте для logout если будете делать
        "/api/Users/add",
        "/login",
        "/login.html",
        "/profile.html",
        "/swagger",
        "/favicon.ico",
        "/css/",
        "/js/",
        "/images/",
        "/lib/",
        "/static/",
        "/health",
        "/api/DropAndCreateBD"
    };

    private readonly List<string> _allowedPatterns = new()
    {
        "/api/aut",         // Паттерн для любых путей содержащих /api/aut
        "/api/logout"       // Паттерн для любых путей содержащих /api/logout
    };

    public AuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        // Проверяем, нужно ли пропускать запрос без авторизации
        if (ShouldSkipAuth(context))
        {
            // Если пользователь уже авторизован и пытается зайти на страницу логина - перенаправляем на профиль
            if (IsAuthenticated(context) && IsLoginPage(context.Request.Path))
            {
                context.Response.Redirect("/profile");
                return;
            }

            await _next(context);
            return;
        }

        var isAuthenticated = context.Session.GetString("IsAuthenticated");
        if (string.IsNullOrEmpty(isAuthenticated) || isAuthenticated != "true")
        {
            context.Response.StatusCode = 401;

            if (context.Request.Path.StartsWithSegments("/api"))
            {
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("{\"message\": \"Не авторизован\"}");
            }
            else
            {
                context.Response.Redirect("/login");
            }
            return;
        }

        context.Session.SetString("IsAuthenticated", "true");

        await _next(context);
    }

    private bool IsLoginPage(PathString path)
    {
        return path.StartsWithSegments("/login") ||
               path.StartsWithSegments("/login.html") ||
               path == "/";
    }

    private bool IsAuthenticated(HttpContext context)
    {
        var isAuthenticated = context.Session.GetString("IsAuthenticated");
        return !string.IsNullOrEmpty(isAuthenticated) && isAuthenticated == "true";
    }

    private bool ShouldSkipAuth(HttpContext context)
    {
        var path = context.Request.Path;

        // Пропускаем OPTIONS запросы (для CORS)
        if (context.Request.Method == "OPTIONS")
            return true;

        // Главная страница
        if (path == "/" || string.IsNullOrEmpty(path.Value))
            return true;

        // Проверяем точное совпадение с allowed paths
        if (_allowedPaths.Any(allowedPath =>
            path.StartsWithSegments(allowedPath, StringComparison.OrdinalIgnoreCase)))
            return true;

        // Проверяем по паттернам (содержит часть пути)
        if (_allowedPatterns.Any(pattern =>
            path.Value.Contains(pattern, StringComparison.OrdinalIgnoreCase)))
            return true;

        return false;
    }
}