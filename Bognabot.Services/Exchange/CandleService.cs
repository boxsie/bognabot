using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bognabot.Data.Exchange.Dtos;
using Bognabot.Data.Exchange.Enums;
using Bognabot.Services.Exchange.Contracts;
using Bognabot.Services.Exchange.Factories;
using Bognabot.Services.Repository;
using NLog;

namespace Bognabot.Services.Exchange
{
    public class CandleService
    {
        private readonly ILogger _logger;
        private readonly RepositoryService _repoService;
        private readonly List<IExchangeService> _exchangeServices;
        private readonly Dictionary<Instrument, IStreamSubscription> _candleSubscriptions;
        private readonly Dictionary<Instrument, IStreamSubscription> _tradeSubscriptions;
        private readonly Dictionary<string, ExchangeCandles> _exchangeCandles;

        public CandleService(ILogger logger, RepositoryService repoService, IEnumerable<IExchangeService> exchangeServices, IndicatorFactory indicatorFactory)
        {
            _repoService = repoService;
            _exchangeServices = exchangeServices.ToList();
            _logger = logger;
            
            var instruments = Enum.GetValues(typeof(Instrument)).Cast<Instrument>().ToArray();

            _candleSubscriptions = new Dictionary<Instrument, IStreamSubscription>();
            _tradeSubscriptions = new Dictionary<Instrument, IStreamSubscription>();
            _exchangeCandles = new Dictionary<string, ExchangeCandles>();

            foreach (var instrument in instruments)
            {
                _candleSubscriptions.Add(instrument, new StreamSubscription<CandleDto>(OnNewCandle));
                _tradeSubscriptions.Add(instrument, new StreamSubscription<TradeDto>(OnNewTrade));

                foreach (var exchange in _exchangeServices)
                {
                    foreach (var timePeriod in exchange.ExchangeConfig.SupportedTimePeriods)
                    {
                        if (!exchange.ExchangeConfig.SupportedInstruments.ContainsKey(instrument)) 
                            continue;

                        var exchangeData = new ExchangeCandles(logger, repoService, exchange, indicatorFactory, timePeriod.Key, instrument);

                        _exchangeCandles.Add(exchangeData.Key, exchangeData);
                    }
                }
            }
        }

        public async Task StartAsync()
        {
            foreach (var candleData in _exchangeCandles.Values)
                await candleData.LoadAsync();

            foreach (var exchange in _exchangeServices)
            {
                var supportedInstruments = exchange.ExchangeConfig.SupportedInstruments;

                foreach (var instrument in supportedInstruments.Keys)
                {
                    await exchange.SubscribeToStreamAsync<CandleDto>(ExchangeChannel.Candle, _candleSubscriptions[instrument], instrument);
                    await exchange.SubscribeToStreamAsync<TradeDto>(ExchangeChannel.Trade, _tradeSubscriptions[instrument], instrument);
                }
            }
        }

        public CandleDto GetLatestCandle(string exchangeName, Instrument instrument, TimePeriod timePeriod)
        {
            var candleData = GetData(exchangeName, instrument, timePeriod);

            if (candleData != null)
                return candleData.CurrentCandle;

            _logger.Log(LogLevel.Error, $"{exchangeName} {instrument} {timePeriod} candle data not found");
            return null;
        }

        public Task<List<CandleDto>> GetCandlesAsync(string exchangeName, Instrument instrument, TimePeriod timePeriod, int dataPoints)
        {
            var candleData = GetData(exchangeName, instrument, timePeriod);

            if (candleData != null)
                return Task.FromResult(candleData.GetCandles());

            _logger.Log(LogLevel.Error, $"{exchangeName} {instrument} {timePeriod} candle data not found");

            return null;
        }

        public Task<ExchangeCandles> GetExchangeCandleDataAsync(string exchangeName, Instrument instrument, TimePeriod timePeriod)
        {
            var candleData = GetData(exchangeName, instrument, timePeriod);

            if (candleData != null)
                return Task.FromResult(candleData);

            _logger.Log(LogLevel.Error, $"{exchangeName} {instrument} {timePeriod} candle data not found");

            return null;
        }

        private async Task OnNewCandle(CandleDto[] arg)
        {
            if (arg == null || !arg.Any())
                return;

            var last = arg.Last();

            var key = ExchangeUtils.GetCandleDataKey(last.ExchangeName, last.Instrument, last.Period);

            if (!_exchangeCandles.ContainsKey(key))
                return;

            await _exchangeCandles[key].InsertCandlesAsync(arg);
        }

        private Task OnNewTrade(TradeDto[] arg)
        {
            if (arg == null || !arg.Any())
                return Task.CompletedTask;

            var last = arg.Last();

            var timePeriods = Enum.GetValues(typeof(TimePeriod)).Cast<TimePeriod>().ToArray();

            foreach (var period in timePeriods)
            {
                var key = ExchangeUtils.GetCandleDataKey(last.ExchangeName, last.Instrument, period);

                if (!_exchangeCandles.ContainsKey(key))
                    continue;

                _exchangeCandles[key].UpdateCurrentCandle(last.Price, arg.Length, arg.Sum(x => x.Size));
            }

            return Task.CompletedTask;
        }

        private ExchangeCandles GetData(string exchangeName, Instrument instrument, TimePeriod period)
        {
            var key = ExchangeUtils.GetCandleDataKey(exchangeName, instrument, period);

            return _exchangeCandles.ContainsKey(key) 
                ? _exchangeCandles[key] 
                : null;
        }
    }
}
