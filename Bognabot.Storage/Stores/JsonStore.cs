using System;
using System.Threading.Tasks;
using Bognabot.Storage.Core;
using Newtonsoft.Json;

namespace Bognabot.Storage.Stores
{
    public class JsonStore<T> : IStorageIO<T>, IDisposable
    {
        private readonly JsonSerializerSettings _settings;

        public JsonStore()
        {
            _settings = new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Utc };
        }

        public JsonStore(JsonSerializerSettings settings)
        {
            _settings = settings;
        }

        public async Task WriteAsync(string filePath, T content)
        {
            using (var ts = new TextStore())
            {
                var json = JsonConvert.SerializeObject(content, _settings);

                await ts.WriteAsync(filePath, json);
            }
        }

        public async Task<T> ReadAsync(string filePath)
        {
            using (var ts = new TextStore())
            {
                var json = await ts.ReadAsync(filePath);

                return JsonConvert.DeserializeObject<T>(json, _settings);
            }
        }

        public void Dispose() { }
    }
}