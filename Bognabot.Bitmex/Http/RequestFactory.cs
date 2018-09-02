using System;
using Bognabot.Bitmex.Core;
using Bognabot.Bitmex.Http.Requests;
using Bognabot.Config;
using Bognabot.Data.Models.Exchange;
using Bognabot.Domain.Entities.Instruments;

namespace Bognabot.Bitmex.Http
{
    public static class RequestFactory
    {
        public static TradeCommandRequest GetTradeRequest(InstrumentType instrument, TimePeriod candleSize, DateTimeOffset startTime, DateTimeOffset endTime, int startAt = 1, int count = 750)
        {
            return new TradeCommandRequest
            {
                IsAuth = true,
                Path = Cfg.Exchange.App.Bitmex.CandlePath,
                Symbol = BitmexUtils.ToSymbol(instrument),
                StartAt = startAt,
                Count = count,
                TimeInterval = candleSize.ToBitmexTimePeriod(),
                StartTime = startTime.ToUtcTimeString(),
                EndTime = endTime.ToUtcTimeString(),
            };
        }
    }
}