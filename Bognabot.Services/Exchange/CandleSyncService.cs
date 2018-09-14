using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Bognabot.Data.Exchange.Enums;
using Bognabot.Data.Exchange.Models;
using Bognabot.Domain.Entities.Instruments;
using Bognabot.Services.Repository;
using NLog;

namespace Bognabot.Services.Exchange
{
    public class CandleSyncService
    {
        private readonly RepositoryService _repoService;
        private readonly IEnumerable<IExchangeService> _exchanges;
        private readonly ILogger _logger;
        private readonly IStreamSubscription _subscription;

        public CandleSyncService(RepositoryService repoService, IEnumerable<IExchangeService> exchanges, ILogger logger)
        {
            _repoService = repoService;
            _exchanges = exchanges;
            _logger = logger;

            _subscription = new StreamSubscription<CandleModel>(InsertCandles);
        }

        public async Task StartSync()
        {
            await Catchup();

            foreach (var exchange in _exchanges)
                await exchange.SubscribeToStreamAsync<CandleModel>(ExchangeChannel.Candle, _subscription);
        }

        private async Task Catchup()
        {
            var instruments = Enum.GetValues(typeof(Instrument)).Cast<Instrument>();
            var periods = Enum.GetValues(typeof(TimePeriod)).Cast<TimePeriod>();

            foreach (var instrument in instruments)
            {
                foreach (var exchange in _exchanges)
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
                        _logger.Log(LogLevel.Info, $"{exchange.ExchangeConfig.ExchangeName} {instrument} {period.Key} {lastEntryLogText}");

                        var syncStatusText = (dataPoints > 0 ? $"{dataPoints} data points behind" : "up to date");
                        _logger.Log(LogLevel.Info, $"{exchange.ExchangeConfig.ExchangeName} {instrument} {period.Key} is {syncStatusText}");

                        await InsertCandles(await exchange.GetCandlesAsync(instrument, period.Key, GetTimeOffsetFromDataPoints(period.Key, now, dataPoints), exchange.Now));
                    }
                }
            }
        }

        private async Task InsertCandles(IReadOnlyCollection<CandleModel> candleModels)
        {
            if(candleModels == null || !candleModels.Any())
                return;

            var first = candleModels.First();

            var candleRepo = await _repoService.GetCandleRepositoryAsync(first.ExchangeName, first.Instrument, first.Period);
            
            var last = await candleRepo.GetLastEntryAsync();

            var candles = candleModels.Where(x => last == null || x.Timestamp > last.TimestampOffset).Select(Mapper.Map<Candle>).ToList();

            if (!candles.Any())
                return;

            _logger.Log(LogLevel.Debug, $"Inserting {candles.Count} candle records");

            await candleRepo.CreateAsync(candles);

            _logger.Log(LogLevel.Info, $"{first.ExchangeName} {first.Period} {first.Instrument} candles have been updated");
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
            var mins = (int)span.TotalMinutes;

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
