using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
        
        public static TradeSide ToTradeType(string side)
        {
            return side == "Buy" ? TradeSide.Buy : TradeSide.Sell;
        }

        public static Dictionary<string, string> GetHttpAuthHeaders(HttpMethod httpMethod, string requestPath, string urlQuery, string key, string secret)
        {
            var sb = new StringBuilder();

            sb.Append(httpMethod.ToString());
            sb.Append("/api/v1");
            sb.Append(requestPath);

            if (httpMethod == HttpMethod.GET)
                sb.Append(urlQuery);

            sb.Append(BitmexUtils.Expires());

            if (httpMethod != HttpMethod.GET)
                sb.Append(urlQuery);

            var signatureMessage = sb.ToString();
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