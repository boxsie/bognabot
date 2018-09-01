using System;
using System.Collections.Generic;
using System.Text;

namespace Bognabot.Data.Models.Exchange
{
    public enum InstrumentType
    {
        BtcUsd
    }

    public enum TimePeriod
    {
        OneMinute,
        FiveMinutes,
        FifteenMinutes,
        OneHour,
        OneDay
    }

    public enum TradeType
    {
        Buy,
        Sell
    }

    public class TradeModel
    {
        public InstrumentType Instrument { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public TradeType Side { get; set; }
        public long Size { get; set; }
        public double Price { get; set; }
    }

    public class BookModel
    {
        public InstrumentType Instrument { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public TradeType Side { get; set; }
        public long Size { get; set; }
        public double Price { get; set; }
    }

    public class CandleModel
    {
        public InstrumentType Instrument { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public TimePeriod Period { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public double Open { get; set; }
        public double Close { get; set; }
        public double Volume { get; set; }
        public long Trades { get; set; }
    }
}
