public class AuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly List<string> _allowedPaths = new()
    {
        "/api/Users/aut",       
        "/api/Users/logout",   
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
        "/api/aut",         
        "/api/logout"       
    };

    public AuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        if (ShouldSkipAuth(context))
        {
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

        if (context.Request.Method == "OPTIONS")
            return true;

        if (path == "/" || string.IsNullOrEmpty(path.Value))
            return true;

        if (_allowedPaths.Any(allowedPath =>
            path.StartsWithSegments(allowedPath, StringComparison.OrdinalIgnoreCase)))
            return true;

        if (_allowedPatterns.Any(pattern =>
            path.Value.Contains(pattern, StringComparison.OrdinalIgnoreCase)))
            return true;

        return false;
    }
}
