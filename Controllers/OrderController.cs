using Microsoft.AspNetCore.Mvc;
using System.Data;
using MyDataBase;
using MyAPIC_Suhi.models;

namespace MyAPIC_Suhi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly DataBase _database;

        public OrderController(DataBase database)
        {
            _database = database;
        }

        [HttpGet("get")]
        public ActionResult GetOrders()
        {
            try
            {
                var UserId = HttpContext.Session.GetInt32("UserId");

                string sql = @"SELECT uo.id, od.status, od.total_amount FROM users_orders uo
                    JOIN orders_detail od ON od.order_id = uo.id 
                    WHERE user_id = @user_id";
                var parameters = new Dictionary<string, object>
                {
                    { "@user_id", UserId }
                };

                var result = _database.QueryFirstOrDefault<GetOrders>(sql, parameters);

                if (result == null)
                {
                    return Ok(new { message = "Заказов нет" });
                }

                return Ok(new
                {
                   orders = result,
                   message = "Готово"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("add")]
        public ActionResult AddOrder([FromBody] OrderAdd orderAdd)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                {
                    return Unauthorized(new { message = "Пользователь не авторизован" });
                }

                string sql = "INSERT INTO users_orders (user_id, order_date, type) VALUES (@user_id, @order_date, @type) RETURNING id";
                var parameters = new Dictionary<string, object>
                {
                    { "@user_id", userId },
                    { "@order_date", DateTime.Now },
                    { "@type", orderAdd.Type }
                };

                int orderId = _database.ExecuteScalar<int>(sql, parameters);

                string sqlOrderDetail = @"INSERT INTO orders_detail (order_id, status, cancelled_time, total_amount, comment) 
                    VALUES (@order_id, @status, @cancelled_time, @total_amount, @comment)";

                var paramsOrderDetail = new Dictionary<string, object>
                {
                    { "@order_id", orderId },
                    { "@status", "pending" },
                    { "@cancelled_time", DateTime.Now.AddMinutes(20) },
                    { "@total_amount", orderAdd.TotalAmmount },
                    { "@comment", orderAdd.Comment ?? string.Empty }
                };

                _database.ExecuteNonQuery(sqlOrderDetail, paramsOrderDetail);

                foreach (var sushiItem in orderAdd.Sushi)
                {
                    string sqlPrice = "SELECT cost FROM sushi WHERE id = @id";
                    var parametersPrice = new Dictionary<string, object> { { "@id", sushiItem.Key } };
                    int price = _database.ExecuteScalar<int>(sqlPrice, parametersPrice);

                    string sqlOrderItems = @"INSERT INTO orders_items (order_id, sushi_id, quantity, price) 
                        VALUES (@order_id, @sushi_id, @quantity, @price)";

                    var paramsOrderItems = new Dictionary<string, object>
                    {
                        { "@order_id", orderId },
                        { "@sushi_id", sushiItem.Key },
                        { "@quantity", sushiItem.Value },
                        { "@price", price }
                    };

                    _database.ExecuteNonQuery(sqlOrderItems, paramsOrderItems);
                }

                return Ok(new { message = "Готово", orderId = orderId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("get/order/{id}")]
        public IActionResult GetCurrentOrder(int Id)
        {
            try
            {
                string orderSql = @"SELECT uo.id, od.status, od.comment, od.total_amount 
                           FROM users_orders uo
                           LEFT JOIN orders_detail od ON od.order_id = uo.id 
                           WHERE uo.id = @id";

                var orderParameters = new Dictionary<string, object> { { "@id", Id } };

                string itemsSql = @"SELECT s.name, oi.quantity, oi.price
                           FROM orders_items oi 
                           LEFT JOIN sushi s ON s.id = oi.sushi_id 
                           LEFT JOIN sushi_photo sp ON sp.sushi_id = oi.sushi_id 
                           WHERE order_id = @orderId";

                var itemsParameters = new Dictionary<string, object> { { "@orderId", Id } };
                
                var orderInfo = _database.QueryFirstOrDefault<CurrentOrderInfo>(orderSql, orderParameters);
                var orderItems = _database.QueryList<CurrentOrderItems>(itemsSql, itemsParameters);

                int totatlCost = 0;

                foreach (var cost in orderItems)
                {
                    totatlCost += cost.Price;
                }

                var result = new
                {
                    Order = orderInfo,
                    Items = orderItems,
                    totatlCost
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("delete/{id}")]
        public IActionResult DeleteOrder(int id)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");

                string checkSql = @"SELECT od.status FROM users_orders uo 
                    JOIN orders_detail od ON od.order_id = uo.id
                    WHERE uo.user_id = @user_id AND uo.id = @id";

                var checkParams = new Dictionary<string, object>
                {
                    { "@user_id", userId },
                    { "@id", id }
                };

                string status = _database.ExecuteScalar<string>(checkSql, checkParams);

                if (status == "pending")
                {
                    string sql = @"UPDATE order_detail SET (status = cancelled) 
                        WHERE user_id = @user_id AND order_id = @order_id";
                    var parameters = new Dictionary<string, object>
                    {
                        { "@user_id", userId},
                        { "@order_id", id}
                    };
                    int rowsAffected = _database.ExecuteNonQuery(sql, parameters);

                    if (rowsAffected > 0)
                    {
                        return Ok(new { message = "Заказ успешно отменен" });
                    }
                    else
                    {
                        return BadRequest(new { message = "Не удалось отменить заказ" });
                    }
                }

                return Ok(new { message = "Заказ уже в обработке, нельзя отменить, обратитесь в поддержку" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new {message = ex.Message});
            }
        }

    }
}