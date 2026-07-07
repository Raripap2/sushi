using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using Dapper;

namespace MyDataBase
{
    public class DataBase
    {
        protected readonly string _connectionString;
        private readonly Drop_Create _dropCreate;

        public DataBase(string connectionString)
        {
            _connectionString = connectionString;
            _dropCreate = new Drop_Create();
        }
        public void DropAndCreteBD()
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                string sql = _dropCreate.DropAndCreateSQL();
                var command = new NpgsqlCommand(sql, connection);
                command.ExecuteNonQuery();
            }
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                string sql = _dropCreate.photoSql();
                var command = new NpgsqlCommand(sql, connection);
                command.ExecuteNonQuery();
            }
        }

        public int ExecuteNonQuery(string sql, Dictionary<string, object> parameters = null)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            using (var command = new NpgsqlCommand(sql, connection))
            {
                connection.Open();

                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                    }
                }

                return command.ExecuteNonQuery();
            }
        }

        public DataTable ExecuteQuery(string sql, Dictionary<string, object> parameters = null)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            using (var command = new NpgsqlCommand(sql, connection))
            {
                connection.Open();

                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                    }
                }

                using (var render = command.ExecuteReader())
                {
                    var dataTable = new DataTable();
                    dataTable.Load(render);
                    return dataTable;
                }
            }
        }

        public T ExecuteScalar<T>(string sql, Dictionary<string, object> parameters = null)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            using (var command = new NpgsqlCommand(sql, connection))
            {
                connection.Open();

                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                    }
                }

                var result = command.ExecuteScalar();

                if (result == null || result == DBNull.Value)
                {
                    // Для nullable типов возвращаем default
                    if (default(T) == null)
                        return default(T);

                    throw new InvalidOperationException("Null object cannot be converted to a value type.");
                }

                return (T)Convert.ChangeType(result, typeof(T));
            }
        }

        public T QueryFirstOrDefault<T>(string sql, Dictionary<string, object> parameters = null)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                return connection.QueryFirstOrDefault<T>(sql, parameters);
            }
        }

        public List<T> QueryList<T>(string sql, Dictionary<string, object> parameters = null)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                return connection.Query<T>(sql, parameters).ToList();
            }
        }
    }
}
