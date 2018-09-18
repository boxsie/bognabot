using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Bognabot.Data.Exchange.Enums;
using Bognabot.Domain.Entities.Instruments;
using Bognabot.Services.Exchange;
using Bognabot.Services.Repository;
using Bognabot.Trader;
using NLog;

namespace Bognabot.Services.Trader
{
    public class SignalsService
    {
        private readonly ILogger _logger;
        private readonly RepositoryService _repositoryService;
        private readonly CandleService _candleService;
        private readonly Dictionary<string, ISignal> _signals;

        public SignalsService(ILogger logger, RepositoryService repositoryService, CandleService candleService, IEnumerable<ISignal> signals)
        {
            _logger = logger;
            _repositoryService = repositoryService;
            _candleService = candleService;

            _signals = signals.ToDictionary(x => x.GetType().Name);
        }

        public T GetSignal<T>() where T : ISignal
        {
            var tType = typeof(T).Name;

            if (!_signals.ContainsKey(tType))
                return default(T);

            return (T)_signals[tType];
        }

        public async Task ProcessSignals(IExchangeService exchangeService, Instrument instrument)
        {
            var periods = Enum.GetValues(typeof(TimePeriod)).Cast<TimePeriod>();
            
            _logger.Log(LogLevel.Info, $"---- {exchangeService.ExchangeConfig.ExchangeName} {instrument} ----");
            
            foreach (var timePeriod in periods)
            {
                if (!exchangeService.ExchangeConfig.SupportedTimePeriods.ContainsKey(timePeriod))
                    continue;

                var candleRepo =
                    await _repositoryService.GetCandleRepositoryAsync(exchangeService.ExchangeConfig.ExchangeName,
                        instrument, timePeriod);

                if (candleRepo == null)
                    continue;

                var lastCandles = new List<Candle>()
                {
                    Mapper.Map<Candle>(_candleService.GetLatestCandle(instrument, timePeriod,
                        exchangeService.ExchangeConfig.ExchangeName))
                };

                lastCandles.AddRange(await candleRepo.GetLastEntriesAsync());

                if (!lastCandles.Any())
                    continue;

                var candles = lastCandles.ToArray();
                
                foreach (var signal in _signals.Values.Where(x => x.SupportedTimePeriods.Any(y => y == timePeriod)))
                {
                    var ss = await signal.ProcessSignal(timePeriod, candles);
                    _logger.Log(LogLevel.Info, $"{exchangeService.ExchangeConfig.ExchangeName} {instrument} {timePeriod} - {signal.GetType().Name} = {ss}");
                }
            }
        }
    }
}