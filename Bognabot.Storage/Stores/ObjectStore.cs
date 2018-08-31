using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Bognabot.Storage.Core;

namespace Bognabot.Storage.Stores
{
    public class ObjectStore : IStorageIO<object>
    {
        public Task WriteAsync(string filePath, object content)
        {
            using (var f = File.Create(filePath))
            {
                var bf = new BinaryFormatter();

                bf.Serialize(f, content);
            }

            return Task.CompletedTask;
        }

        public Task<object> ReadAsync(string filePath)
        {
            if (!File.Exists(filePath))
                return null;

            using (var f = File.Open(filePath, FileMode.Open))
            {
                var bf = new BinaryFormatter();

                return Task.FromResult(bf.Deserialize(f));
            }
        }
    }
}