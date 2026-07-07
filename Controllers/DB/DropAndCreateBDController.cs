using Microsoft.AspNetCore.Mvc;
using MyDataBase;
using System.Data;

namespace MyAPIC_Suhi.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]

    public class DropAndCreateBDController : ControllerBase
    {
        private readonly DataBase _database;

        public DropAndCreateBDController(DataBase database)
        {
            _database = database;
        }

        [HttpGet]
        public ActionResult DropAndCreateBD()
        {
            try
            {
                _database.DropAndCreteBD();
                return StatusCode(200, "Готово");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка {ex.Message}");
            }
        }

    }
}