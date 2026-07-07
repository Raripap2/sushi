using Microsoft.AspNetCore.Mvc;

namespace MyAPIC_Suhi.Controllers
{
    [ApiController] 
    [Route("api/[controller]")] 
    public class CheckAutController : ControllerBase 
    {
        [HttpGet("check-auth")] 
        public IActionResult CheckAuth()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var isAuthenticated = HttpContext.Session.GetString("IsAuthenticated");

            if (userId.HasValue && isAuthenticated == "true")
            {
                HttpContext.Session.SetString("IsAuthenticated", "true");
                HttpContext.Session.SetInt32("UserId", userId.Value);

                return Ok(new
                {
                    authenticated = true,
                    userId = userId.Value,
                    message = "Сессия активна"
                });
            }

            return Ok(new { authenticated = false });
        }
    }
}
