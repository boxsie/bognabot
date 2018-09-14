using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bognabot.Services.Repository
{
    public interface IRepository<in T>
    {
        Task CreateTable(string connectionString, string tableName);

        Task<int> CreateAsync(T entity);
        Task<IEnumerable<int>> CreateAsync(IEnumerable<T> entities);
    }
}