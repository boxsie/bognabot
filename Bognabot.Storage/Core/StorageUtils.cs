using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Bognabot.Storage.Stores;
using Newtonsoft.Json;

namespace Bognabot.Storage.Core
{
    public static class StorageUtils
    {
        private const string EncryptKeyBase = "E546C8DF278CD5931069B522E695D4F2";

        public static string GetDefaultUserDataPath(string appName)
        {
            var path = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? Environment.GetEnvironmentVariable("LocalAppData")
                : RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                    ? $"~/Library/Application Support/"
                    : $"Home/";

            return StorageUtils.PathCombine(path, appName, true);
        }

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
            key = PadKey(key);

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
            key = PadKey(key);

            var fullCipher = Convert.FromBase64String(encryptedText);

            var iv = new byte[16];
            var cipher = new byte[fullCipher.Length - iv.Length];

            Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
            Buffer.BlockCopy(fullCipher, iv.Length, cipher, 0, fullCipher.Length - iv.Length);

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

        public static byte[] EncryptHMACSHA256(byte[] keyByte, byte[] messageBytes)
        {
            using (var hash = new HMACSHA256(keyByte))
                return hash.ComputeHash(messageBytes);
        }

        public static string ByteArrayToHexString(byte[] ba)
        {
            var hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

        private static string PadKey(string key)
        {
            const int bitLen = 32;

            if (key.Length < bitLen)
                key = $"{key}{EncryptKeyBase.Substring(0, bitLen - key.Length)}";
            else if (key.Length > bitLen)
                key = key.Substring(0, bitLen);

            return key;
        }
    }
}