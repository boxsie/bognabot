using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Bognabot.Data.Models.Exchange;
using Newtonsoft.Json;

namespace Bognabot.Exchanges.Bitmex.Core
{
    public static class BitmexUtils
    {
        public static long NowSeconds()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        public static long Expires()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 3600;
        }

        public static string CreateSignature(string secret)
        {
            var message = $"GET/realtime{Expires()}";
            var signatureBytes = Encrypt(Encoding.UTF8.GetBytes(secret), Encoding.UTF8.GetBytes(message));

            return ByteArrayToHexString(signatureBytes);
        }

        public static string ToBitmexTimePeriod(this TimePeriod period)
        {
            switch (period)
            {
                case TimePeriod.OneMinute:
                    return "1m";
                case TimePeriod.FiveMinutes:
                    return "5m";
                case TimePeriod.FifteenMinutes:
                    throw new ArgumentOutOfRangeException(nameof(period), period, null);
                case TimePeriod.OneHour:
                    return "1h";
                case TimePeriod.OneDay:
                    return "1d";
                default:
                    throw new ArgumentOutOfRangeException(nameof(period), period, null);
            }
        }

        public static string ToUtcTimeString(this DateTimeOffset dateTime)
        {
            return dateTime.ToString("yyy-MM-ddTHH:mm:ss.fffZ");
        }
        
        public static IDictionary<string, string> AsDictionary(this object source, BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)
        {
            return source.GetType().GetProperties(bindingAttr).ToDictionary
            (
                propInfo => propInfo.GetCustomAttributes(typeof(JsonPropertyAttribute))?.Cast<JsonPropertyAttribute>().FirstOrDefault()?.PropertyName ?? propInfo.Name,
                propInfo => propInfo.GetValue(source, null).ToString()
            );
        }

        private static byte[] Encrypt(byte[] keyByte, byte[] messageBytes)
        {
            using (var hash = new HMACSHA256(keyByte))
                return hash.ComputeHash(messageBytes);
        }

        private static string ByteArrayToHexString(byte[] ba)
        {
            var hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }
    }
}