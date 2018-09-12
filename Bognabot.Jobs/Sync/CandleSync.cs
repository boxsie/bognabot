using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Bognabot.Data.Exchange;
using Bognabot.Data.Exchange.Contracts;
using Bognabot.Data.Exchange.Enums;
using Bognabot.Data.Exchange.Models;
using Bognabot.Data.Models.Exchange;
using Bognabot.Data.Repository;
using Bognabot.Domain.Entities.Instruments;
using Bognabot.Jobs.Core;
using Bognabot.Services;
using Bognabot.Services.Repository;
using NLog;

namespace Bognabot.Jobs.Sync
{
    public class CandleSync : SyncJob
    {
        private readonly RepositoryService _repoService;
        private readonly IEnumerable<IExchangeService> _exchangeServices;

        public CandleSync(ILogger logger, RepositoryService repoService,
            IEnumerable<IExchangeService> exchangeServices) : base(logger, 5)
        {
            _repoService = repoService;
            _exchangeServices = exchangeServices;
        }

        protected override async Task ExecuteAsync()
        {
            var instruments = Enum.GetValues(typeof(Instrument)).Cast<Instrument>();
            var periods = Enum.GetValues(typeof(TimePeriod)).Cast<TimePeriod>();

            foreach (var instrument in instruments)
            {
                foreach (var exchange in _exchangeServices)
                {
                    var supportedPeriods = exchange.ExchangeConfig.SupportedTimePeriods;
                    var maxPoints = exchange.ExchangeConfig.UserConfig.MaxDataPoints;

                    foreach (var period in supportedPeriods)
                    {
                        var candleRepo = await _repoService.GetCandleRepositoryAsync(exchange.ExchangeConfig.ExchangeName, instrument, period.Key);
                        var lastEntry = await candleRepo.GetLastEntryAsync();
                        var now = exchange.Now;
                        
                        var dataPoints = lastEntry != null
                            ? GetDataPointsFromTimeSpan(period.Key, now - lastEntry.TimestampOffset)
                            : maxPoints;

                        if (dataPoints <= 0)
                            continue;

                        if (dataPoints > maxPoints)
                            dataPoints = maxPoints;

                        var lastEntryLogText = (lastEntry != null ? $"was last seen at {lastEntry.TimestampOffset}" : "has no pevious records");
                        Logger.Log(LogLevel.Info, $"{exchange.ExchangeConfig.ExchangeName} {instrument} {period.Key} {lastEntryLogText}");

                        var syncStatusText = (dataPoints > 0 ? $"{dataPoints} data points behind" : "up to date");
                        Logger.Log(LogLevel.Info, $"{exchange.ExchangeConfig.ExchangeName} {instrument} {period.Key} is {syncStatusText}");

                        await exchange.GetCandlesAsync(
                            instrument,
                            period.Key,
                            GetTimeOffsetFromDataPoints(period.Key, now, dataPoints),
                            exchange.Now,
                            OnRecieve);
                    }
                }
            }
        }

        private async Task OnRecieve(CandleModel[] arg)
        {
            if (arg == null || !arg.Any())
                return;

            var first = arg.First();

            var candleRepo = await _repoService.GetCandleRepositoryAsync(first.ExchangeName, first.Instrument, first.Period);

            Logger.Log(LogLevel.Debug, $"Inserting {arg.Length} candle records");

            var candles = arg.Select(Mapper.Map<Candle>);

            await candleRepo.CreateAsync(candles);

            Logger.Log(LogLevel.Info, "Candles are up to date");
        }

        private DateTimeOffset GetTimeOffsetFromDataPoints(TimePeriod period, DateTimeOffset start, int dataPoints)
        {
            switch (period)
            {
                case TimePeriod.OneMinute:
                    return start.AddMinutes(-dataPoints);
                case TimePeriod.FiveMinutes:
                    return start.AddMinutes(-dataPoints * 5);
                case TimePeriod.FifteenMinutes:
                    return start.AddMinutes(-dataPoints * 15);
                case TimePeriod.OneHour:
                    return start.AddHours(-dataPoints);
                case TimePeriod.OneDay:
                    return start.AddDays(-dataPoints);
                default:
                    throw new ArgumentOutOfRangeException(nameof(period), period, null);
            }
        }

        private int GetDataPointsFromTimeSpan(TimePeriod period, TimeSpan span)
        {
            var mins = (int) span.TotalMinutes;

            switch (period)
            {
                case TimePeriod.OneMinute:
                    return mins;
                case TimePeriod.FiveMinutes:
                    return mins / 5;
                case TimePeriod.FifteenMinutes:
                    return mins / 15;
                case TimePeriod.OneHour:
                    return mins / 60;
                case TimePeriod.OneDay:
                    return mins / (24 * 60);
                default:
                    throw new ArgumentOutOfRangeException(nameof(period), period, null);
            }
        }
    }
}