using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Bognabot.Data.Repository;
using Dapper;
using Microsoft.Extensions.Logging;

namespace Bognabot.Services.Repository
{
    public class Repository<T> : IRepository<T>
    {
        private readonly ILogger _logger;
        private readonly Stopwatch _stopwatch;

        private string _connectionString;
        private string _tableName;

        public Repository(ILogger logger)
        {
            _logger = logger;
            _stopwatch = new Stopwatch();
        }

        public async Task LoadAsync(string connectionString, string tableName)
        {
            _connectionString = connectionString;
            _tableName = tableName;

            try
            {
                using (var con = new SQLiteConnection(_connectionString))
                {
                    await con.OpenAsync();

                    await con.ExecuteAsync(BuildCreateSql());
                }
            }
            catch (Exception e)
            {
                _logger.LogCritical(e.Message);
                throw;
            }
        }

        public async Task<int> CreateAsync(T entity)
        {
            try
            {
                var insertSql = BuildInsertSql();
                var param = CreateParams(entity);

                using (var con = new SQLiteConnection(_connectionString))
                {
                    await con.OpenAsync();

                    return await con.ExecuteAsync(insertSql, param);
                }
            }
            catch (Exception e)
            {
                _logger.LogCritical(e.Message);
                throw;
            }
        }

        public async Task<IEnumerable<int>> CreateAsync(IEnumerable<T> entities)
        {
            try
            {
                using (var con = new SQLiteConnection(_connectionString))
                {
                    await con.OpenAsync();

                    using (var transaction = con.BeginTransaction())
                    {
                        var results = new List<int>();

                        foreach (var entity in entities)
                        {
                            var insertSql = BuildInsertSql();
                            var param = CreateParams(entity);

                            var result = await con.ExecuteAsync(insertSql, param, transaction);

                            results.Add(result);
                        }

                        transaction.Commit();

                        return results;
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogCritical(e.Message);
                throw;
            }
        }

        public async Task<T> GetLastEntry()
        {
            var sql = $"SELECT * FROM {_tableName} WHERE ROWID = (SELECT MAX(ROWID) FROM '{_tableName}');";

            try
            {
                using (var con = new SQLiteConnection(_connectionString))
                {
                    await con.OpenAsync();

                    var result = await con.QueryAsync(sql);

                    return result.FirstOrDefault();
                }
            }
            catch (Exception e)
            {
                _logger.LogCritical(e.Message);
                throw;
            }
        }

        private DynamicParameters CreateParams(T entity)
        {
            var dp = new DynamicParameters();

            foreach (var prop in typeof(T).GetProperties())
                dp.Add(prop.Name, prop.GetValue(entity));

            return dp;
        }

        private string BuildInsertSql()
        {
            var type = typeof(T);
            var props = type.GetProperties().ToList();
            var sb = new StringBuilder();
            
            sb.Append($"INSERT INTO {_tableName} (");
            sb.Append(string.Join(',', props.Select(x => x.Name)));
            sb.Append(") values(");
            sb.Append(string.Join(',', props.Select(x => $"@{x.Name}")));
            sb.Append(")");

            return sb.ToString();
        }

        private string BuildCreateSql()
        {
            var type = typeof(T);
            var props = type.GetProperties();
            var sb = new StringBuilder();

            sb.Append($"CREATE TABLE IF NOT EXISTS {_tableName}(");
            sb.Append($"{string.Join(',', props.Select(x => $"{x.Name} {TypeToSql(x.PropertyType)} NOT NULL"))}");
            sb.Append(")");

            return sb.ToString();
        }

        private static string TypeToSql(MemberInfo type)
        {
            switch (type.Name)
            {
                case "Int32":
                case "Int64":
                    return "int";
                case "Double":
                    return "float";
                case "Decimal":
                    return "numeric";
                case "String":
                    return "varchar(255)";
                case "DateTime":
                case "DateTimeOffset":
                    return "datetime";
                default:
                    throw new NotSupportedException();
            }
        }
    }
}