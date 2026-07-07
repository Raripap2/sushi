using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration.UserSecrets;
using MyAPIC_Suhi.models;
using MyDataBase;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;

namespace MyAPIC_Suhi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SushiController : ControllerBase
    {
        private readonly DataBase _database;

        public SushiController(DataBase database)
        {
            _database = database;
        }

        [HttpPost("get")]
        public IActionResult GetSushi([FromBody] PageSushi pageSushi)
        {
            try
            {
                string sql;
                Dictionary<string, object> parameters;

                if (pageSushi.Type == "all")
                {
                    // Запрос для всех типов с пагинацией
                    sql = @"SELECT s.*, p.photo, st.type 
                    FROM sushi s 
                    LEFT JOIN sushi_photo p ON p.sushi_id = s.id 
                    LEFT JOIN sushi_type st ON s.type_id = st.id
                    ORDER BY s.type_id
                    OFFSET @Offset ROWS 
                    FETCH NEXT @Limit ROWS ONLY";

                    parameters = new Dictionary<string, object>
                    {
                        { "Offset", (pageSushi.Page - 1) * pageSushi.Limit },
                        { "Limit", pageSushi.Limit }
                    };
                }
                else
                {
                    // Запрос для конкретного типа с пагинацией
                    sql = @"SELECT s.*, p.photo, st.type 
                    FROM sushi s 
                    JOIN sushi_photo p ON p.sushi_id = s.id 
                    LEFT JOIN sushi_type st ON s.type_id = st.id 
                    WHERE st.type = @Type 
                    ORDER BY s.type_id 
                    OFFSET @Offset ROWS 
                    FETCH NEXT @Limit ROWS ONLY";

                    parameters = new Dictionary<string, object>
            {
                { "Type", pageSushi.Type },
                { "Offset", (pageSushi.Page - 1) * pageSushi.Limit },
                { "Limit", pageSushi.Limit }
            };
                }

                DataTable result = _database.ExecuteQuery(sql, parameters);
                var sushi_s = new List<Dictionary<string, object>>();

                foreach (DataRow row in result.Rows)
                {
                    var sushi = new Dictionary<string, object>();
                    foreach (DataColumn column in result.Columns)
                    {
                        sushi[column.ColumnName] = row[column];
                    }
                    sushi_s.Add(sushi);
                }

                // Проверяем, есть ли еще данные
                bool hasMore = sushi_s.Count == pageSushi.Limit;

                return Ok(new
                {
                    sushi = sushi_s,
                    page = pageSushi.Page,
                    limit = pageSushi.Limit,
                    type = pageSushi.Type,
                    hasMore = hasMore
                });

            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message,
                });
            }
        }

        [HttpGet("current/{SushiId}")]
        public IActionResult CurrentSushi(int SushiId)
        {
            try
            {
                string sql = @"SELECT s.*, sp.photo FROM sushi s
                    JOIN sushi_photo sp ON sp.sushi_id = s.id 
                    WHERE s.id = @id";
                var parameters = new Dictionary<string, object> { { "@id", SushiId } };
                var sushi = _database.QueryFirstOrDefault<CurrentSushi>(sql, parameters);
                return Ok(new { sushi });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new {message = ex.Message});
            }
        }

        [HttpPut("cart/add")]
        public IActionResult AddSushiToCart([FromBody] CartAdd cartAdd)
        {
            try
            {
                int? userId = HttpContext.Session.GetInt32("UserId");
                string userIdsql = "SELECT id FROM users_cart WHERE user_id = @user_id";
                var userIdparams = new Dictionary<string, object> { { "@user_id", userId } };

                int cartId = _database.ExecuteScalar<int>(userIdsql, userIdparams);

                string quantitySql = @"SELECT cd.quantity FROM users_cart uc
                    JOIN cart_detail cd ON uc.id = cd.cart_id 
                    WHERE uc.user_id = @user_id AND cd.sushi_id = @sushi_id";

                var quantityParams = new Dictionary<string, object>
                {
                    { "@user_id", userId },
                    { "@sushi_id", cartAdd.Sushi_id }
                };

                // Безопасный способ проверки наличия записи
                string checkSql = @"SELECT COUNT(*) FROM cart_detail 
                          WHERE cart_id = @cart_id AND sushi_id = @sushi_id";

                var checkParams = new Dictionary<string, object>
                {
                    { "@cart_id", cartId },
                    { "@sushi_id", cartAdd.Sushi_id }
                };

                int exists = _database.ExecuteScalar<int>(checkSql, checkParams);

                if (exists > 0)
                {
                    // Обновляем количество
                    string updateSql = @"UPDATE cart_detail SET quantity = quantity + @quantity 
                       WHERE cart_id = @cart_id AND sushi_id = @sushi_id";

                    var updateParams = new Dictionary<string, object>
                    {
                        { "@cart_id", cartId },
                        { "@sushi_id", cartAdd.Sushi_id },
                        { "@quantity", cartAdd.Quantity }
                    };
                    _database.ExecuteNonQuery(updateSql, updateParams);
                }
                else
                {
                    // Добавляем новый
                    string insertSql = @"INSERT INTO cart_detail (cart_id, sushi_id, quantity) 
                       VALUES (@cart_id, @sushi_id, @quantity)";

                    var insertParams = new Dictionary<string, object>
                    {
                        { "@cart_id", cartId },
                        { "@sushi_id", cartAdd.Sushi_id },
                        { "@quantity", cartAdd.Quantity }
                    };

                    _database.ExecuteNonQuery(insertSql, insertParams);
                }

                return Ok(new { message = "Добавлено", success = true });

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("cart/get")]
        public IActionResult GetCart()
        {
            try
            {
                int? userId = HttpContext.Session.GetInt32("UserId");
                string cartIdSql = "SELECT id FROM users_cart WHERE user_id = @user_id";
                var cartIdParams = new Dictionary<string, object> { { "@user_id", userId } };
                int cartId = _database.ExecuteScalar<int>(cartIdSql, cartIdParams);

                string sql = @"SELECT cd.sushi_id, sp.photo, s.name, s.cost, cd.quantity, (s.cost * cd.quantity) as total_cost
                    FROM cart_detail cd 
                    JOIN sushi s ON cd.sushi_id = s.id 
                    JOIN sushi_photo sp ON cd.sushi_id = sp.sushi_id
                    WHERE cart_id = @cart_id";

                var parameters = new Dictionary<string, object> { { "@cart_id", cartId } };
                DataTable cartDetail = _database.ExecuteQuery(sql, parameters);

                // Если корзина пуста - возвращаем 200 OK с пустым массивом
                if (cartDetail.Rows.Count == 0)
                {
                    return Ok(new
                    {
                        message = "Корзина пуста",
                        items = new List<object>(),
                        total = 0
                    });
                }

                var cartDetailSushi = new List<Dictionary<string, object>>();
                decimal total = 0;

                foreach (DataRow row in cartDetail.Rows)
                {
                    var _cart = new Dictionary<string, object>();

                    foreach (DataColumn column in cartDetail.Columns)
                    {
                        _cart[column.ColumnName] = row[column];
                    }

                    // Считаем общую стоимость
                    if (row["total_cost"] != DBNull.Value)
                    {
                        total += Convert.ToDecimal(row["total_cost"]);
                    }

                    cartDetailSushi.Add(_cart);
                }

                return Ok(new
                {
                    items = cartDetailSushi,
                    total = total,
                    count = cartDetailSushi.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpDelete("cart/delete")]
        public IActionResult CartDeleteOne([FromBody] CartAdd cartAdd)
        {
            try
            {
                int? userId = HttpContext.Session.GetInt32("UserId");
                if (!userId.HasValue)
                {
                    return Unauthorized(new { message = "Пользователь не аутентифицирован" });
                }

                if (cartAdd == null || cartAdd.Sushi_id <= 0)
                {
                    return BadRequest(new { message = "Некорректные данные" });
                }

                // ИСПОЛЬЗУЕМ QueryFirstOrDefault вместо ExecuteScalar
                string sqlGetCartInfo = @"SELECT uc.id as Id, cd.quantity as Quantity 
                                FROM users_cart uc
                                JOIN cart_detail cd ON uc.id = cd.cart_id
                                WHERE uc.user_id = @user_id AND cd.sushi_id = @sushi_id";

                var paramsGetCartInfo = new Dictionary<string, object>
                {
                    { "@user_id", userId.Value },
                    { "@sushi_id", cartAdd.Sushi_id }
                };

                var cartInfo = _database.QueryFirstOrDefault<DeleteCartOne>(sqlGetCartInfo, paramsGetCartInfo);

                if (cartInfo == null)
                {
                    return NotFound(new { message = "Позиция не найдена в корзине" });
                }

                // Остальной код остается без изменений...
                string sql;
                var parameters = new Dictionary<string, object>();

                sql = "DELETE FROM cart_detail WHERE sushi_id = @sushi_id AND cart_id = @cart_id";
                parameters = new Dictionary<string, object>
                {
                    { "@sushi_id", cartAdd.Sushi_id },
                    { "@cart_id", cartInfo.Id }
                };
                

                int affectedRows = _database.ExecuteNonQuery(sql, parameters);

                if (affectedRows == 0)
                {
                    return StatusCode(500, new { message = "Не удалось обновить корзину" });
                }

                return Ok(new { message = "Убрано" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpDelete("cart/clear")]
        public IActionResult CartClear()
        {
            try
            {
                int? userId = HttpContext.Session.GetInt32("UserId");

                string sqlCartId = "SELECT id FROM users_cart WHERE user_id = @user_id";
                var paramsCartId = new Dictionary<string, object> { { "@user_id", userId } };

                int cartId = _database.ExecuteScalar<int>(sqlCartId, paramsCartId);

                string sql = "DELETE FROM cart_detail WHERE cart_id = @cart_id";
                var parameters = new Dictionary<string, object> { { "cart_id", cartId } };
                _database.ExecuteNonQuery(sql, parameters);

                return Ok(new { message = "Готово" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("cart/update")]
        public IActionResult UpdateCartQuantity([FromBody] CartUpdate cartUpdate)
        {
            try
            {
                int? userId = HttpContext.Session.GetInt32("UserId");
                if (!userId.HasValue)
                {
                    return Unauthorized(new { message = "Пользователь не аутентифицирован" });
                }

                // Получаем cart_id
                string cartIdSql = "SELECT id FROM users_cart WHERE user_id = @user_id";
                var cartIdParams = new Dictionary<string, object> { { "@user_id", userId.Value } };

                int cartId = _database.ExecuteScalar<int>(cartIdSql, cartIdParams);

                // Проверяем текущее количество
                string currentQuantitySql = @"SELECT quantity FROM cart_detail 
                                   WHERE cart_id = @cart_id AND sushi_id = @sushi_id";
                var currentQuantityParams = new Dictionary<string, object>
                {
                    { "@cart_id", cartId },
                    { "@sushi_id", cartUpdate.Sushi_id }
                };

                int currentQuantity = _database.ExecuteScalar<int>(currentQuantitySql, currentQuantityParams);
                int newQuantity = currentQuantity + cartUpdate.Quantity;

                if (newQuantity <= 0)
                {
                    // Удаляем запись если количество <= 0
                    string deleteSql = "DELETE FROM cart_detail WHERE cart_id = @cart_id AND sushi_id = @sushi_id";
                    _database.ExecuteNonQuery(deleteSql, currentQuantityParams);
                }
                else
                {
                    // Обновляем количество
                    string updateSql = @"UPDATE cart_detail SET quantity = @quantity 
                             WHERE cart_id = @cart_id AND sushi_id = @sushi_id";
                    var updateParams = new Dictionary<string, object>
                    {
                        { "@cart_id", cartId },
                        { "@sushi_id", cartUpdate.Sushi_id },
                        { "@quantity", newQuantity }
                    };
                    _database.ExecuteNonQuery(updateSql, updateParams);
                }

                return Ok(new { message = "Количество обновлено", success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

    }
}
