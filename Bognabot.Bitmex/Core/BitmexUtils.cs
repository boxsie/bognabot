using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Bognabot.Config;
using Bognabot.Data.Models.Exchange;
using Bognabot.Domain.Entities.Instruments;
using Bognabot.Storage.Core;
using Newtonsoft.Json;

namespace Bognabot.Bitmex.Core
{
    public static class BitmexUtils
    {
        public static DateTimeOffset Now()
        {
            return DateTimeOffset.Now;
        }

        public static long NowSeconds()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        public static long Expires()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 3600;
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

        public static InstrumentType ToInstrumentType(string symbol)
        {
            var instrument = Cfg.Exchange.App.Bitmex.InstrumentNames.Where(x => x == symbol).Select((x, i) => (InstrumentType?)i).FirstOrDefault();

            if (!instrument.HasValue)
                throw new ArgumentOutOfRangeException();

            return instrument.Value;
        }

        public static string ToSymbol(InstrumentType instrument)
        {
            return Cfg.Exchange.App.Bitmex.InstrumentNames[(int)instrument];
        }

        public static TradeType ToTradeType(string side)
        {
            return side == "Buy" ? TradeType.Buy : TradeType.Sell;
        }
    }
}