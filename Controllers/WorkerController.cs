using Microsoft.AspNetCore.Mvc;
using MyDataBase;
using MyAPIC_Suhi.Models;
using MyAPIC_Suhi.models;

namespace MyAPIC_Suhi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WorkerController : ControllerBase
    {

        private readonly DataBase _database;

        public WorkerController(DataBase database)
        {
            _database = database;
        }

        [HttpPost("edit/order/{id}")]
        [RoleAuthorize("admin", "godmod", "worker")]
        public IActionResult EditCurrentOrder(int id, [FromBody] WorkerEditCurrentOrder editOrder)
        {
            try
            {
                string sql = "UPDATE orders_detail SET status = @status WHERE order_id = @order_id";
                var parameters = new Dictionary<string, object>
                {
                    { "status",  editOrder.Status},
                    { "@order_id", id }
                };

                _database.ExecuteNonQuery(sql, parameters);

                return Ok(new { message = "Готово" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}