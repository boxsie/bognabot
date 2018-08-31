using System;
using System.Threading.Tasks;
using Bognabot.Storage.Core;
using Newtonsoft.Json;

namespace Bognabot.Storage.Stores
{
    public class EncryptedJsonStore<T> : IStorageIO<T>, IDisposable
    {
        private readonly string _key;
        private readonly JsonSerializerSettings _settings;
        
        public EncryptedJsonStore(string key, JsonSerializerSettings settings = null)
        {
            _key = key;
            _settings = settings ?? StorageUtils.GetDefaultSerialiserSettings();
        }

        public async Task WriteAsync(string filePath, T content)
        {
            using (var ts = new EncryptedTextStore(_key))
            {
                var json = JsonConvert.SerializeObject(content, _settings);

                await ts.WriteAsync(filePath, json);
            }
        }

        public async Task<T> ReadAsync(string filePath)
        {
            using (var ts = new EncryptedTextStore(_key))
            {
                var json = await ts.ReadAsync(filePath);

                return JsonConvert.DeserializeObject<T>(json, _settings);
            }
        }

        public void Dispose() { }
    }
}