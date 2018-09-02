using System.Threading.Tasks;

namespace Bognabot.Data.Repository
{
    public interface IRepository<in T>
    {
        Task LoadAsync(string connectionString, string tableName);

        Task<int> Create(T entity);
    }
}