using Microsoft.AspNetCore.Mvc;
using System;

namespace MyAPIC_Suhi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SessionController : ControllerBase
    {
        [HttpGet("update")]
        public ActionResult UpdateSession()
        {
            try
            {
                // Проверяем, существует ли сессия и авторизован ли пользователь
                var userId = HttpContext.Session.GetInt32("UserId");
                var isAuthenticated = HttpContext.Session.GetString("IsAuthenticated");

                if (userId == null || isAuthenticated != "true")
                {
                    return Ok(new
                    {
                        success = false,
                        message = "Сессия недействительна или пользователь не авторизован",
                        isAuthenticated = false
                    });
                }

                // Обновляем время жизни сессии (ASP.NET Core делает это автоматически при обращении)
                // Просто считываем и записываем обратно те же значения для обновления сессии
                HttpContext.Session.SetInt32("UserId", userId.Value);
                HttpContext.Session.SetString("IsAuthenticated", "true");

                // Можно также добавить время последней активности
                HttpContext.Session.SetString("LastActivity", DateTime.Now.ToString());

                return Ok(new
                {
                    success = true,
                    message = "Сессия обновлена",
                    userId = userId,
                    isAuthenticated = true
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Ошибка при обновлении сессии: {ex.Message}"
                });
            }
        }

        [HttpGet("check")]
        public ActionResult CheckSession()
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                var isAuthenticated = HttpContext.Session.GetString("IsAuthenticated");

                return Ok(new
                {
                    success = true,
                    isAuthenticated = (userId != null && isAuthenticated == "true"),
                    userId = userId,
                    message = userId != null ? "Сессия активна" : "Сессия не активна"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Ошибка при проверке сессии: {ex.Message}"
                });
            }
        }
    }
}