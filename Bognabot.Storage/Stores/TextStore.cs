using System;
using System.IO;
using System.Security;
using System.Threading.Tasks;
using Bognabot.Storage.Core;

namespace Bognabot.Storage.Stores
{
    public class TextStore : IStorageIO<string>, IDisposable
    {
        public virtual async Task WriteAsync(string filePath, string content)
        {
            using (var s = File.CreateText(filePath))
                await s.WriteAsync(content);
        }

        public virtual async Task<string> ReadAsync(string filePath)
        {
            if (!File.Exists(filePath))
                return null;

            using (var s = File.OpenText(filePath))
                return await s.ReadToEndAsync();
        }

        public virtual void Dispose() { }
    }
}