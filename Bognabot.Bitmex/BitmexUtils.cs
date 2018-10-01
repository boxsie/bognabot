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

        public static long ToUnixTimestamp(this DateTime d)
        {
            var epoch = d - new DateTime(1970, 1, 1, 0, 0, 0);

            return (long)epoch.TotalSeconds;
        }
    }
}