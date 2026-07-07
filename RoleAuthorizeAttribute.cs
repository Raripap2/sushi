using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace MyAPIC_Suhi
{
    // Создаем кастомный атрибут
    public class RoleAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _allowedRoles;

        public RoleAuthorizeAttribute(params string[] allowedRoles)
        {
            _allowedRoles = allowedRoles;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var httpContext = context.HttpContext;
            var userStatus = httpContext.Session.GetString("Status");

            if (string.IsNullOrEmpty(userStatus) || !_allowedRoles.Contains(userStatus))
            {
                context.Result = new JsonResult(new { message = "Доступ запрещен. Недостаточно прав" })
                {
                    StatusCode = 403
                };
            }
        }
    }
}
