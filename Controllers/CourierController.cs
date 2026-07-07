using Microsoft.AspNetCore.Mvc;
using MyDataBase;
using MyAPIC_Suhi.Models;
using MyAPIC_Suhi.models;


namespace MyAPIC_Suhi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CourierController : ControllerBase
    {
        private readonly DataBase _database;

        public CourierController(DataBase database)
        {
            _database = database;
        }

        [HttpGet("orders/get")]
        [RoleAuthorize("admin", "godmod", "courier")]
        public IActionResult GetOrders()
        {
            try
            {
                string sqlOrders = @"SELECT 
                        uo.id, 
                        od.cancelled_time as CancelledTime,
                        od.total_amount as TotalAmount,
                        ua.city, 
                        ua.street, 
                        ua.house 
                    FROM users_orders uo
                        JOIN orders_detail od ON od.order_id = uo.id 
                        JOIN users_address ua ON ua.user_id = uo.user_id
                    WHERE od.status = 'on_the_way' and uo.type = 'delivery'";

                var ordersInfo = _database.QueryList<CourierGetOrders>(sqlOrders);

                if (ordersInfo == null && ordersInfo.Any())
                {
                    return Ok(new { message = "Готово", orders = ordersInfo });
                }

                return StatusCode(200, new { message = "Заказов нет" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
