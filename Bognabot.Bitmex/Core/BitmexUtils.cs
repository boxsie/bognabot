using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Bognabot.Config;
using Bognabot.Config.Enums;
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

        public static Instrument? ToInstrumentType(string symbol)
        {
            var instrumentKvp = Cfg.GetExchangeConfig(SupportedExchange.Bitmex).SupportedInstruments.FirstOrDefault(x => x.Value == symbol);

            if (instrumentKvp.Value == null)
                throw new ArgumentOutOfRangeException();

            return instrumentKvp.Key;
        }

        public static string ToSymbol(Instrument instrument)
        {
            var supportedInstruments = Cfg.GetExchangeConfig(SupportedExchange.Bitmex).SupportedInstruments;

            return supportedInstruments.ContainsKey(instrument) 
                ? supportedInstruments[instrument] 
                : throw new ArgumentOutOfRangeException();
        }

        public static string ToBitmexTimePeriod(this TimePeriod period)
        {
            var supportedPeriods = Cfg.GetExchangeConfig(SupportedExchange.Bitmex).SupportedTimePeriods;

            return supportedPeriods.ContainsKey(period)
                ? supportedPeriods[period]
                : throw new ArgumentOutOfRangeException();
        }

        public static TradeType ToTradeType(string side)
        {
            return side == "Buy" ? TradeType.Buy : TradeType.Sell;
        }
    }
}