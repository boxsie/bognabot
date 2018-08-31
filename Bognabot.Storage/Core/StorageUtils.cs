using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Bognabot.Storage.Core
{
    public static class StorageUtils
    {
        public static string PathCombine(string first, string second, bool trailing = false)
        {
            var pathSeperator = Path.DirectorySeparatorChar;

            var path = Path.Combine(first, second).Replace(pathSeperator == '/' ? '\\' : '/', pathSeperator);

            if (trailing)
                path += pathSeperator;

            return path;
        }
        
        public static JsonSerializerSettings GetDefaultSerialiserSettings()
        {
            return new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Utc };
        }

        public static async Task<string> EncryptTextAsync(string text, string key)
        {
            var keyBytes = Encoding.UTF8.GetBytes(key);

            using (var aesAlg = Aes.Create())
            {
                if (aesAlg == null)
                    return null;

                using (var encryptor = aesAlg.CreateEncryptor(keyBytes, aesAlg.IV))
                {
                    using (var msEncrypt = new MemoryStream())
                    {
                        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            using (var swEncrypt = new StreamWriter(csEncrypt))
                            {
                                await swEncrypt.WriteAsync(text);
                            }
                        }
                        
                        var decryptedContent = msEncrypt.ToArray();

                        var result = new byte[aesAlg.IV.Length + decryptedContent.Length];

                        Buffer.BlockCopy(aesAlg.IV, 0, result, 0, aesAlg.IV.Length);
                        Buffer.BlockCopy(decryptedContent, 0, result, aesAlg.IV.Length, decryptedContent.Length);

                        return Convert.ToBase64String(result);
                    }
                }
            }
        }

        public static async Task<string> DecryptTextAsync(string encryptedText, string key)
        {
            var fullCipher = Convert.FromBase64String(encryptedText);

            var iv = new byte[16];
            var cipher = new byte[16];

            Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
            Buffer.BlockCopy(fullCipher, iv.Length, cipher, 0, iv.Length);

            var keyBytes = Encoding.UTF8.GetBytes(key);

            using (var aesAlg = Aes.Create())
            {
                if (aesAlg == null)
                    return null;

                using (var decryptor = aesAlg.CreateDecryptor(keyBytes, iv))
                {
                    using (var msDecrypt = new MemoryStream(cipher))
                    {
                        using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (var srDecrypt = new StreamReader(csDecrypt))
                            {
                                return await srDecrypt.ReadToEndAsync();
                            }
                        }
                    }
                }
            }
        }
    }
}