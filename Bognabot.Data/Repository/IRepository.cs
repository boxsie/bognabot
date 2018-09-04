using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bognabot.Data.Repository
{
    public interface IRepository<in T>
    {
        Task LoadAsync(string connectionString, string tableName);

        Task<int> CreateAsync(T entity);
        Task<IEnumerable<int>> CreateAsync(IEnumerable<T> entities);
    }
}