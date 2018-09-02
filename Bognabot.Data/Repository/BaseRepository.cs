using System;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Bognabot.Data.Core;
using Dapper;

namespace Bognabot.Data.Repository
{
    public abstract class BaseRepository<T> : IRepository<T>
    {
        public abstract Task<int> Create(T entity);

        protected string ConnectionString;
        protected string TableName;

        public async Task LoadAsync(string connectionString, string tableName)
        {
            ConnectionString = connectionString;
            TableName = tableName;
            
            try
            {
                using (var con = new SQLiteConnection(ConnectionString))
                {
                    await con.OpenAsync();

                    await con.ExecuteAsync(BuildCreateSql());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task<T> GetLastEntry()
        {
            var sql = $"SELECT * FROM {TableName} WHERE ROWID = (SELECT MAX(ROWID) FROM '{TableName}');";

            try
            {
                using (var con = new SQLiteConnection(ConnectionString))
                {
                    await con.OpenAsync();

                    var result = await con.QueryAsync(sql);

                    return result.FirstOrDefault();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        protected DynamicParameters CreateParams(T entity)
        {
            var dp = new DynamicParameters();

            foreach (var prop in typeof(T).GetProperties())
                dp.Add(prop.Name, prop.GetValue(entity));

            return dp;
        }

        protected string BuildInsertSql()
        {
            var type = typeof(T);
            var props = type.GetProperties().ToList();
            var sb = new StringBuilder();
            
            sb.Append($"INSERT INTO {TableName} (");
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

            sb.Append($"CREATE TABLE IF NOT EXISTS {TableName}(");
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