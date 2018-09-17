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
    public class CandleService
    {
        private readonly RepositoryService _repoService;
        private readonly List<IExchangeService> _exchanges;
        private readonly ILogger _logger;
        private readonly IStreamSubscription _candleSubscription;
        private readonly IStreamSubscription _tradeSubscription;
        private readonly TimePeriod[] _timePeriods;
        private readonly Dictionary<TimePeriod, Dictionary<string, CandleModel>> _currentCandles;

        public CandleService(RepositoryService repoService, IEnumerable<IExchangeService> exchanges, ILogger logger)
        {
            _repoService = repoService;
            _exchanges = exchanges.ToList();
            _logger = logger;

            _candleSubscription = new StreamSubscription<CandleModel>(InsertCandles);
            _tradeSubscription = new StreamSubscription<TradeModel>(OnNewTrade);
            
            _timePeriods = Enum.GetValues(typeof(TimePeriod)).Cast<TimePeriod>().ToArray();
            _currentCandles = _timePeriods.ToDictionary(x => x, 
                    y => _exchanges.ToDictionary(xx => xx.ExchangeConfig.ExchangeName, 
                            yy => new CandleModel { Period = y, ExchangeName = yy.ExchangeConfig.ExchangeName }));
        }

        public async Task StartAsync()
        {
            await Catchup();

            foreach (var exchange in _exchanges)
            {
                await exchange.SubscribeToStreamAsync<CandleModel>(ExchangeChannel.Candle, _candleSubscription);
                await exchange.SubscribeToStreamAsync<TradeModel>(ExchangeChannel.Trade, _tradeSubscription);
            }
        }

        public CandleModel GetLatestCandle(TimePeriod period, string exchangeName)
        {
            var periodCandles = _currentCandles[period];

            return periodCandles.ContainsKey(exchangeName)
                ? periodCandles[exchangeName]
                : null;
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
            
            var lastModel = candleModels.Last();

            var latestCandle = _currentCandles[lastModel.Period][lastModel.ExchangeName];

            latestCandle.Open = lastModel.Close;
            latestCandle.High = lastModel.Close;
            latestCandle.Low = lastModel.Close;
            latestCandle.Trades = 0;
            latestCandle.Volume = 0;
            latestCandle.Timestamp = GetTimeOffsetFromDataPoints(lastModel.Period, latestCandle.Timestamp, -1);

            var candleRepo = await _repoService.GetCandleRepositoryAsync(lastModel.ExchangeName, lastModel.Instrument, lastModel.Period);
            
            var last = await candleRepo.GetLastEntryAsync();

            var candles = candleModels.Where(x => last == null || x.Timestamp > last.TimestampOffset).Select(Mapper.Map<Candle>).ToList();

            if (!candles.Any())
                return;

            _logger.Log(LogLevel.Debug, $"Inserting {candles.Count} candle records");

            await candleRepo.CreateAsync(candles);

            _logger.Log(LogLevel.Info, $"{lastModel.ExchangeName} {lastModel.Period} {lastModel.Instrument} candles have been updated");
        }
        
        private Task OnNewTrade(TradeModel[] arg)
        {
            if (arg == null || !arg.Any())
                return Task.CompletedTask;

            var last = arg.Last();

            foreach (var period in _timePeriods)
            {
                var candle = _currentCandles[period][last.ExchangeName];

                candle.Trades += arg.Length;
                candle.Volume += arg.Sum(x => x.Size);

                if (last.Price > candle.High)
                    candle.High = last.Price;

                if (last.Price < candle.Low)
                    candle.Low = last.Price;

                candle.Close = last.Price;
            }

            return Task.CompletedTask;
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
