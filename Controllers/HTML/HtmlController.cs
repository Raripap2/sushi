using Microsoft.AspNetCore.Mvc;

namespace MyAPIC_Suhi.Controllers.HTML
{
    public class HtmlController : Controller
    {
        [HttpGet]
        [Route("login")]
        public IActionResult Login()
        {
            return File("~/login.html", "text/html");
        }

        [HttpGet]
        [Route("profile")]
        public IActionResult Profile()
        {
            return File("~/profile.html", "text/html");
        }

        [HttpGet]
        [Route("sushi")]
        public IActionResult Sushi()
        {
            return File("~/sushi.html", "text/html");
        }

        [HttpGet]
        [Route("cart")]
        public IActionResult Cart()
        {
            return File("~/Cart.html", "text/html");
        }

        [HttpGet]
        [Route("current-sushi")]
        public IActionResult CurrentSushi()
        {
            return File("~/CurrentSushi.html", "text/html");
        }
    }
}