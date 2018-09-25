using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Bognabot.Data.Exchange.Dtos;
using Bognabot.Data.Exchange.Enums;
using Bognabot.Domain.Entities.Instruments;
using Bognabot.Services.Exchange.Contracts;
using Bognabot.Services.Exchange.Factories;
using Bognabot.Services.Repository;
using Bognabot.Trader;
using NLog;

namespace Bognabot.Services.Exchange
{
    public class ExchangeCandles
    {
        public string Key => ExchangeUtils.GetCandleDataKey(_exchangeName, _instrument, _period);
        public CandleDto CurrentCandle { get; }

        private readonly ILogger _logger;
        private readonly RepositoryService _repoService;
        private readonly IExchangeService _exchange;
        private readonly IndicatorFactory _indicatorFactory;
        private readonly TimePeriod _period;
        private readonly Instrument _instrument;
        private readonly string _exchangeName;

        private List<CandleDto> _candles;

        public ExchangeCandles(ILogger logger, RepositoryService repoService, IExchangeService exchange, IndicatorFactory indicatorFactory, TimePeriod period, Instrument instrument)
        {
            _logger = logger;
            _repoService = repoService;
            _exchange = exchange;
            _indicatorFactory = indicatorFactory;
            _period = period;
            _instrument = instrument;
            _exchangeName = _exchange.ExchangeConfig.ExchangeName;

            CurrentCandle = new CandleDto { ExchangeName = _exchangeName, Period = _period, Instrument = _instrument };
        }
        
        public double[] Indicate<T>(int dataPoints, bool includeNow = true) where T : IIndicator
        {
            var indicator = _indicatorFactory.Get<T>();
            var candles = GetCandles();

            return indicator?.Calculate(candles.ToArray(), dataPoints);
        }

        public async Task LoadAsync()
        {
            var candleRepo = await _repoService.GetCandleRepositoryAsync(_exchangeName, _instrument, _period);

            if (candleRepo == null)
            {
                _logger.Log(LogLevel.Error, $"{_exchangeName} {_instrument} {_period} repository not found");
                return;
            }

            var dbCandles = await candleRepo.GetLastEntriesAsync();

            _candles = dbCandles.Select(Mapper.Map<CandleDto>).Select(x =>
            {
                x.Instrument = _instrument;
                x.ExchangeName = _exchangeName;
                x.Period = _period;

                return x;
            }).ToList();

            if (!_candles.Any())
                _logger.Log(LogLevel.Warn, $"{_exchangeName} {_instrument} {_period} could not load any candles");
            else
                _logger.Log(LogLevel.Debug, $"{_exchangeName} {_instrument} {_period} candle load complete");

            await CatchupAsync(await candleRepo.GetLastEntryAsync());
        }

        public List<CandleDto> GetCandles()
        {
            var candles = new List<CandleDto> {CurrentCandle};

            candles.AddRange(_candles);

            return candles;
        }

        public async Task InsertCandlesAsync(CandleDto[] candleDtos)
        {
            if (candleDtos == null || !candleDtos.Any())
                return;

            candleDtos = candleDtos.OrderByDescending(x => x.Timestamp).ToArray();

            ResetCurrentCandle(candleDtos.First());

            var candleRepo = await _repoService.GetCandleRepositoryAsync(_exchangeName, _instrument, _period);

            var last = await candleRepo.GetLastEntryAsync();

            var candles = candleDtos
                .Where(x => last == null || x.Timestamp.ToUniversalTime() > last.Timestamp.ToUniversalTime())
                .Select(Mapper.Map<Candle>)
                .ToList();

            if (!candles.Any())
                return;

            _logger.Log(LogLevel.Debug, $"Inserting {candles.Count} candle records");

            await candleRepo.CreateAsync(candles);

            _candles.InsertRange(0, candleDtos);

            _logger.Log(LogLevel.Info, $"{_exchangeName} {_instrument} {_period} candles have been updated");
        }

        public void UpdateCurrentCandle(double price, int trades, double volume)
        {
            CurrentCandle.Trades += trades;
            CurrentCandle.Volume += volume;

            if (price > CurrentCandle.High)
                CurrentCandle.High = price;

            if (price < CurrentCandle.Low)
                CurrentCandle.Low = price;

            CurrentCandle.Close = price;
        }

        private async Task CatchupAsync(Candle lastDbCandle)
        {
            var maxPoints = _exchange.ExchangeConfig.UserConfig.MaxDataPoints;
            var now = _exchange.Now;

            var dataPoints = lastDbCandle != null
                ? ExchangeUtils.GetDataPointsFromTimeSpan(_period, now - lastDbCandle.Timestamp.ToUniversalTime())
                : maxPoints;

            if (dataPoints <= 0)
                return;

            if (dataPoints > maxPoints)
                dataPoints = maxPoints;

            var candles = await _exchange.GetCandlesAsync(_instrument, _period, ExchangeUtils.GetTimeOffsetFromDataPoints(_period, now, dataPoints), now);

            if (candles != null)
                await InsertCandlesAsync(candles.ToArray());

            var lastEntryLogText = (lastDbCandle != null ? $"was last seen at {lastDbCandle.Timestamp.ToUniversalTime()}" : "has no pevious records");
            _logger.Log(LogLevel.Info, $"{_exchangeName} {_instrument} {_period} {lastEntryLogText}");

            var syncStatusText = (dataPoints > 0 ? $"{dataPoints} data points behind" : "up to date");
            _logger.Log(LogLevel.Info, $"{_exchangeName} {_instrument} {_period} is {syncStatusText}");
        }

        private void ResetCurrentCandle(CandleDto latest)
        {
            CurrentCandle.Open = latest.Close;
            CurrentCandle.High = latest.Close;
            CurrentCandle.Low = latest.Close;
            CurrentCandle.Trades = 0;
            CurrentCandle.Volume = 0;
            CurrentCandle.Timestamp = ExchangeUtils.GetTimeOffsetFromDataPoints(latest.Period, CurrentCandle.Timestamp, -1);
        }
    }
}