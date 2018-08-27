using System.Security.Cryptography;
using System.Text;

namespace Bognabot.Bitmex
{
    public static class BitmexAuthentication
    {
        public static string CreateSignature(string secret)
        {
            var message = $"GET/realtime{BitmexTime.Expires()}";

            var signatureBytes = HMACSHA256(Encoding.UTF8.GetBytes(secret), Encoding.UTF8.GetBytes(message));

            return ByteArrayToString(signatureBytes);
        }

        private static byte[] HMACSHA256(byte[] keyByte, byte[] messageBytes)
        {
            using (var hash = new HMACSHA256(keyByte))
                return hash.ComputeHash(messageBytes);
        }

        private static string ByteArrayToString(byte[] ba)
        {
            var hex = new StringBuilder(ba.Length * 2);

            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);

            return hex.ToString();
        }
    }
}