using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Bognabot.Data.Exchange.Enums;
using Newtonsoft.Json;

namespace Bognabot.Bitmex
{
    public static class BitmexUtils
    {
        public static DateTimeOffset Now()
        {
            return DateTimeOffset.UtcNow;
        }

        public static long NowSeconds()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        public static long Expires()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 3600;
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

        public static TradeType ToTradeType(string side)
        {
            return side == "Buy" ? TradeType.Buy : TradeType.Sell;
        }
    }
}