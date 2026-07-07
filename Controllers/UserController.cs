using Microsoft.AspNetCore.Mvc;
using MyAPIC_Suhi.Models;
using MyDataBase;
using System.Data;

namespace MyAPIC_Suhi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly DataBase _database;

        public UsersController(DataBase database)
        {
            _database = database;
        }

        [HttpPost("aut")]
        public IActionResult Autification([FromBody] UserAut data)
        {
            try
            {
                if (data == null)
                {
                    return BadRequest(new { message = "Нет данных пользователя" });
                }

                string sql = @"SELECT id, password FROM users 
                    WHERE login = @login";

                var parameters = new Dictionary<string, object>
                {
                    { "@login", data.Login }
                };

                DataTable result = _database.ExecuteQuery(sql, parameters);

                if (result.Rows.Count == 0)
                    return Ok(new
                    {
                        success = false,
                        message = $"Пользователь с логином {data.Login} не найден"
                    });

                DataRow row = result.Rows[0];
                int userId = Convert.ToInt32(row["id"]);
                string storedPassword = row["password"].ToString();

                if (data.Password != storedPassword)
                    return Ok(new
                    {
                        success = false,
                        message = "Неверный пароль"
                    });

                HttpContext.Session.SetInt32("UserId", userId);
                HttpContext.Session.SetString("IsAuthenticated", "true");

                return Ok(new
                {
                    success = true,
                    message = "Авторизация успешна",
                    userId = userId
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpGet("profile")]
        public IActionResult GetUserProfile()
        {
            try
            {
                // Получаем ID из сессии
                var userId = HttpContext.Session.GetInt32("UserId");

                if (!userId.HasValue)
                {
                    return Unauthorized(new { message = "Пользователь не авторизован" });
                }

                string sql = @"SELECT u.*, ua.*, up.photo 
                        FROM users u 
                        LEFT JOIN users_address ua ON ua.user_id = u.id 
                        LEFT JOIN users_photo up ON up.user_id = u.id
                        WHERE u.id = 1;";

                var parameters = new Dictionary<string, object>
                {
                    { "@id", userId.Value }
                };

                DataTable result = _database.ExecuteQuery(sql, parameters);

                if (result.Rows.Count == 0)
                {
                    return NotFound(new { message = "Пользователь не найден" });
                }

                var user = new Dictionary<string, object>();
                DataRow row = result.Rows[0];

                foreach (DataColumn column in result.Columns)
                {
                    user[column.ColumnName] = row[column] is DBNull ? null : row[column];
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Ошибка {ex.Message}" });
            }
        }

        [HttpGet("get")]
        public IActionResult GetUsers()
        {
            try
            {
                string sql = "SELECT * FROM users";

                DataTable result = _database.ExecuteQuery(sql);

                var users = new List<Dictionary<string, object>>();

                if (result.Rows.Count == 0)
                {
                    return StatusCode(200, "Пользователей не найдено!");
                }

                foreach (DataRow row in result.Rows)
                {
                    var user = new Dictionary<string, object>();

                    foreach (DataColumn column in result.Columns)
                    {
                        user[column.ColumnName] = row[column];
                    }

                    users.Add(user);
                }

                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка при получении пользователей: {ex.Message}");
            }
        }

        [HttpPost("add")]
        public IActionResult AddUser([FromBody] User user)
        {
            try
            {
                if (user == null)
                    return BadRequest(new { message = "Нет данных пользователя" });

                if (string.IsNullOrEmpty(user.Login) || string.IsNullOrEmpty(user.Password))
                    return BadRequest(new { message = "Обязательные поля: Login и Password" });

                // Проверка существующего пользователя
                string checkSql = "SELECT COUNT(*) FROM users WHERE login = @login";
                var checkParams = new Dictionary<string, object> { { "@login", user.Login } };

                int existingCount = _database.ExecuteScalar<int>(checkSql, checkParams);
                if (existingCount > 0)
                    return StatusCode(409, new { message = "Такой логин уже занят" });

                // Вставка пользователя
                string insertSql = @"INSERT INTO users (login, email, username, password) 
                          VALUES (@login, @email, @username, @password) 
                          RETURNING id";

                var userParams = new Dictionary<string, object>
                {
                    { "@login", user.Login },
                    { "@email", user.Email ?? (object)DBNull.Value },
                    { "@username", user.Username ?? (object)DBNull.Value },
                    { "@password", user.Password }
                };

                int userId = _database.ExecuteScalar<int>(insertSql, userParams);

                // Добавляем статус пользователя
                string statusSql = "INSERT INTO users_status (user_id, status) VALUES (@user_id, 'user')";
                var statusParams = new Dictionary<string, object> { { "@user_id", userId } };

                _database.ExecuteNonQuery(statusSql, statusParams);

                string photoSql = "INSERT INTO users_photo (user_id, photo) VALUES (@user_id, 'https://yandex.ru/images/search?p=1&text=%D1%84%D0%BE%D1%82%D0%BE+%D0%BF%D0%BE%D0%BB%D1%8C%D0%B7%D0%BE%D0%B2%D0%B0%D1%82%D0%B5%D0%BB%D1%8F&pos=7&rpt=simage&img_url=https%3A%2F%2Fsteamuserimages-a.akamaihd.net%2Fugc%2F1769331484186621301%2F933D19061460B0D7B91EAE4E7317419D30D95E4D%2F%3Fimw%3D512%26amp%3Bimh%3D505%26amp%3Bima%3Dfit%26amp%3Bimpolicy%3DLetterbox%26amp%3Bimcolor%3D%2523000000%26amp%3Bletterbox%3Dtrue&from=tabbar&lr=11090')";       
                var photoParams = new Dictionary<string, object> { { "@user_id", userId } };

                _database.ExecuteNonQuery(photoSql, photoParams);

                string cartSql = "INSERT INTO users_cart (user_id) VALUES (@user_id)";

                var cartParams = new Dictionary<string, object> { {"@user_id", userId } };

                _database.ExecuteNonQuery(cartSql, cartParams);

                return Ok(new
                {
                    message = "Пользователь добавлен",
                    user_id = userId
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Ошибка добавления пользователя: {ex.Message}" });
            }
        }

        [HttpGet("{id}")]
        public IActionResult GetUserById(int id)
        {
            try
            {
                string sql = "SELECT * FROM users WHERE id = @id";

                var parametrs = new Dictionary<string, object>
                {
                    { "id", id },
                };

                DataTable result = _database.ExecuteQuery(sql, parametrs);

                var user = new Dictionary<string, object>();
                DataRow row = result.Rows[0];

                foreach (DataColumn column in result.Columns)
                {
                    user[column.ColumnName] = row[column] is DBNull ? null : row[column];
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка {ex.Message}");
            }
        }

        [HttpDelete("delete")]
        public IActionResult DeleteUserById()
        {
            try
            {
                var id = HttpContext.Session.GetInt32("UserId");

                string sql = "DELETE FROM users WHERE id = @id";

                var parameters = new Dictionary<string, object>
                {
                    { "id", id },
                };

                _database.ExecuteNonQuery(sql, parameters);

                return Ok(new { message = "Пользователь удален" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка {ex.Message}");
            }
        }

        [HttpPut("edit")]
        public IActionResult EditUser([FromBody] UserEdit user)
        {
            try
            {
                var id = HttpContext.Session.GetInt32("UserId");

                if (user == null)
                {
                    return BadRequest(new { message = "Нет данных для обновления" });
                }

                var setClauses = new List<string>();
                var parameters = new Dictionary<string, object> { { "@id", id } };

                if (!string.IsNullOrEmpty(user.Login))
                {
                    setClauses.Add("login = @login");
                    parameters.Add("@login", user.Login);
                }

                if (!string.IsNullOrEmpty(user.Email))
                {
                    setClauses.Add("email = @email");
                    parameters.Add("@email", user.Email);
                }

                if (!string.IsNullOrEmpty(user.Username))
                {
                    setClauses.Add("username = @username");
                    parameters.Add("@username", user.Username);
                }

                if (!string.IsNullOrEmpty(user.Password))
                {
                    setClauses.Add("password = @password");
                    parameters.Add("@password", user.Password);
                }

                if (setClauses.Count == 0)
                {
                    return BadRequest(new { message = "Нет данных для обновления" });
                }

                string sql = $"UPDATE users SET {string.Join(", ", setClauses)} WHERE id = @id";

                int rowsAffected = _database.ExecuteNonQuery(sql, parameters);

                if (rowsAffected == 0)
                {
                    return NotFound(new { message = "Пользователь не найден" });
                }

                return Ok(new { message = "Данные пользователя обновлены" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Ошибка при обновлении: {ex.Message}" });
            }
        }

        [HttpPost("address/add")]
        public IActionResult AddAddress([FromBody] UserAddress userAddress)
        {
            try
            {
                var UserId = HttpContext.Session.GetInt32("UserId");

                string sql = "SELECT * FROM users_address WHERE user_id = @id";

                var parameters = new Dictionary<string, object> { { "@id", UserId } };

                int Check = _database.ExecuteScalar<int>(sql, parameters);

                if (Check > 0)
                {
                    return StatusCode(200, new { message = "Адрес уже добавлен" });
                }

                string insertSql = @"
                INSERT INTO users_address 
                (user_id, city, street, house, entrance, floor, flat, intercom) 
                VALUES 
                (@user_id, @city, @street, @house, @entrance, @floor, @flat, @intercom)";

                var insertParams = new Dictionary<string, object>
                {
                    { "@user_id", UserId },
                    { "@city", userAddress.City },
                    { "@street", userAddress.Street },
                    { "@house", userAddress.House },
                    { "@entrance", userAddress.Entrance ?? (object)DBNull.Value },
                    { "@floor", userAddress.Floor ?? (object)DBNull.Value },
                    { "@flat", userAddress.Flat ?? (object)DBNull.Value },
                    { "@intercom", userAddress.Intercom }
                };

                int rowsAffect = _database.ExecuteNonQuery(insertSql, insertParams);

                if (rowsAffect > 0) { return StatusCode(200, new { message = "Адрес успешно добавлен" }); }

                else { return StatusCode(500, new { message = "Ошибка при добавлении адреса" }); }
            }
            catch (Exception ex) {return StatusCode(500, new { message = $"Возникла ошибка {ex.Message}" }); }
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            try
            {
                HttpContext.Session.Clear();
                return Ok(new { success = true, message = "Выход выполнен успешно" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPut("address/edit")]
        public IActionResult EditAddress([FromBody] UserAddress userAddress)
        {
            try
            {
                var UserId = HttpContext.Session.GetInt32("UserId");

                // Используйте COUNT(*) вместо SELECT *
                string sql = "SELECT COUNT(*) FROM users_address WHERE user_id = @id";
                var parameters = new Dictionary<string, object> { { "@id", UserId } };

                // COUNT(*) всегда возвращает число (0 или больше), никогда null
                int Check = _database.ExecuteScalar<int>(sql, parameters);

                if (Check == 0)
                {
                    return StatusCode(404, new { message = "Адрес не добавлен" });
                }

                string updateSql = @"
        UPDATE users_address 
        SET city = @city, 
            street = @street, 
            house = @house, 
            entrance = @entrance, 
            floor = @floor, 
            flat = @flat, 
            intercom = @intercom
        WHERE user_id = @user_id";

                var insertParams = new Dictionary<string, object>
        {
            { "@user_id", UserId },
            { "@city", userAddress.City ?? (object)DBNull.Value },
            { "@street", userAddress.Street ?? (object)DBNull.Value },
            { "@house", userAddress.House ?? (object)DBNull.Value },
            { "@entrance", userAddress.Entrance ?? (object)DBNull.Value },
            { "@floor", userAddress.Floor ?? (object)DBNull.Value },
            { "@flat", userAddress.Flat ?? (object)DBNull.Value },
            { "@intercom", userAddress.Intercom ?? (object)DBNull.Value }
        };

                int rowsAffect = _database.ExecuteNonQuery(updateSql, insertParams);

                if (rowsAffect > 0) { return StatusCode(200, new { message = "Адрес успешно изменен" }); }
                else { return StatusCode(400, new { message = "Ошибка изменения адреса" }); }
            }
            catch (Exception ex) { return StatusCode(500, new { message = $"Ошибка {ex.Message}" }); }
        }

        [HttpPost("add/worker")]
        public IActionResult AddWorkerUser([FromBody] User user)
        {
            try
            {
                if (HttpContext.Session.GetString("Status") != "admin" || HttpContext.Session.GetString("Status") != "godmod")
                {
                    return StatusCode(200, new { message = "Недостаточно прав" });
                }
                if (user == null)
                    return BadRequest(new { message = "Нет данных пользователя" });

                if (string.IsNullOrEmpty(user.Login) || string.IsNullOrEmpty(user.Password))
                    return BadRequest(new { message = "Обязательные поля: Login и Password" });

                // Проверка существующего пользователя
                string checkSql = "SELECT COUNT(*) FROM users WHERE login = @login";
                var checkParams = new Dictionary<string, object> { { "@login", user.Login } };

                int existingCount = _database.ExecuteScalar<int>(checkSql, checkParams);
                if (existingCount > 0)
                    return StatusCode(409, new { message = "Такой логин уже занят" });

                // Вставка пользователя
                string insertSql = @"INSERT INTO users (login, email, username, password) 
                          VALUES (@login, @email, @username, @password) 
                          RETURNING id";

                var userParams = new Dictionary<string, object>
                {
                    { "@login", user.Login },
                    { "@email", user.Email ?? (object)DBNull.Value },
                    { "@username", user.Username ?? (object)DBNull.Value },
                    { "@password", user.Password }
                };

                int userId = _database.ExecuteScalar<int>(insertSql, userParams);

                // Добавляем статус пользователя
                string statusSql = "INSERT INTO users_status (user_id, status) VALUES (@user_id, 'worker')";
                var statusParams = new Dictionary<string, object> { { "@user_id", userId } };

                _database.ExecuteNonQuery(statusSql, statusParams);

                string photoSql = "INSERT INTO users_photo (user_id, photo) VALUES (@user_id, 'https://yandex.ru/images/search?p=1&text=%D1%84%D0%BE%D1%82%D0%BE+%D0%BF%D0%BE%D0%BB%D1%8C%D0%B7%D0%BE%D0%B2%D0%B0%D1%82%D0%B5%D0%BB%D1%8F&pos=7&rpt=simage&img_url=https%3A%2F%2Fsteamuserimages-a.akamaihd.net%2Fugc%2F1769331484186621301%2F933D19061460B0D7B91EAE4E7317419D30D95E4D%2F%3Fimw%3D512%26amp%3Bimh%3D505%26amp%3Bima%3Dfit%26amp%3Bimpolicy%3DLetterbox%26amp%3Bimcolor%3D%2523000000%26amp%3Bletterbox%3Dtrue&from=tabbar&lr=11090')";
                var photoParams = new Dictionary<string, object> { { "@user_id", userId } };

                _database.ExecuteNonQuery(photoSql, photoParams);

                string cartSql = "INSERT INTO users_cart (user_id) VALUES (@user_id)";

                var cartParams = new Dictionary<string, object> { { "@user_id", userId } };

                _database.ExecuteNonQuery(cartSql, cartParams);

                HttpContext.Session.SetInt32("UserId", userId);
                HttpContext.Session.SetString("Status", "worker");
                HttpContext.Session.SetString("IsAuthenticated", "true");

                return Ok(new
                {
                    message = "Пользователь добавлен",
                    user_id = userId
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Ошибка добавления пользователя: {ex.Message}" });
            }
        }
    }
}