using System.Threading.Tasks;

namespace Bognabot.Storage.Core
{
    public interface IStorageIO<T>
    {
        Task WriteAsync(string filePath, T content);
        Task<T> ReadAsync(string filePath);
    }
}