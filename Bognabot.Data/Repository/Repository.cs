using System;
using System.Data.SQLite;
using System.Threading.Tasks;
using Bognabot.Data.Core;
using Dapper;

namespace Bognabot.Data.Repository
{
    public class Repository<T> : BaseRepository<T>
    {
        public override async Task<int> Create(T entity)
        {
            try
            {
                var insertSql = BuildInsertSql();
                var param = CreateParams(entity);

                using (var con = new SQLiteConnection(ConnectionString))
                {
                    return await con.ExecuteAsync(insertSql, param);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}