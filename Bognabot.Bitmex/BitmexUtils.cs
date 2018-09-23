using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using Bognabot.Bitmex.Response;
using Bognabot.Data.Exchange;
using Bognabot.Data.Exchange.Enums;
using Bognabot.Storage.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using HttpMethod = Bognabot.Data.Exchange.Enums.HttpMethod;

namespace Bognabot.Bitmex
{
    public static class BitmexUtils
    {
        public static DateTime Now()
        {
            return DateTime.UtcNow;
        }

        public static string ToUtcTimeString(this DateTime dateTime)
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

        public static TradeType ToTradeType(string side)
        {
            return side == "Buy" ? TradeType.Buy : TradeType.Sell;
        }

        public static Dictionary<string, string> GetHttpAuthHeaders(string baseUrl, HttpMethod httpMethod, string requestPath, string urlQuery, string key, string secret)
        {
            var signatureMessage = $"{httpMethod.ToString()}/api/v1{requestPath}{urlQuery}{BitmexUtils.Expires()}";
            var signatureBytes = StorageUtils.EncryptHMACSHA256(Encoding.UTF8.GetBytes(secret), Encoding.UTF8.GetBytes(signatureMessage));

            return new Dictionary<string, string>
            {
                { "api-expires", BitmexUtils.Expires().ToString() },
                { "api-key", key },
                { "api-signature", StorageUtils.ByteArrayToHexString(signatureBytes) }
            };
        }

        public static string GetSocketRequest(string path, params string[] args)
        {
            if (!args.Any())
                throw new MissingFieldException();

            return $@"{{""op"": ""subscribe"", ""args"": [""{path}:{args[0]}""]}}";
        }

        public static string GetSocketRequest(IEnumerable<string> path, List<string[]> args)
        {
            if (!args.Any())
                throw new MissingFieldException();

            return $@"{{""op"": ""subscribe"", ""args"": [""{string.Join("\", \"", path.Select((x, i) => $"{x}:{args[i][0]}"))}""]}}";
        }

        public static string GetSocketAuthCommand(string key, string secret)
        {
            return $@"{{""op"": ""authKeyExpires"", ""args"": [""{key}"", {BitmexUtils.Expires()}, ""{CreateSignature(secret)}""]}}";
        }

        private static string CreateSignature(string secret)
        {
            var message = $"GET/realtime{BitmexUtils.Expires()}";
            var signatureBytes = StorageUtils.EncryptHMACSHA256(Encoding.UTF8.GetBytes(secret), Encoding.UTF8.GetBytes(message));

            return StorageUtils.ByteArrayToHexString(signatureBytes);
        }

        private static long Expires()
        {
            return DateTime.UtcNow.ToUnixTimestamp() + 3600;
        }

        private static long ToUnixTimestamp(this DateTime d)
        {
            var epoch = d - new DateTime(1970, 1, 1, 0, 0, 0);

            return (long)epoch.TotalSeconds;
        }
    }
}