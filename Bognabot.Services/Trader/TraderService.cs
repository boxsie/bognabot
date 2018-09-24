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
using Bognabot.Trader.Indicators;
using NLog;

namespace Bognabot.Services.Trader
{
    public class TraderService
    {
        private readonly ILogger _logger;
        private readonly CandleService _candleService;
        private readonly Dictionary<Type, ISignal> _signals;

        public TraderService(ILogger logger, CandleService candleService, IEnumerable<ISignal> signals)
        {
            _logger = logger;
            _candleService = candleService;

            _signals = signals.ToDictionary(x => x.GetType());
        }

        public T GetSignal<T>() where T : ISignal
        {
            var tType = typeof(T);

            if (!_signals.ContainsKey(tType))
                return default(T);

            return (T)_signals[tType];
        }

        public async Task ProcessSignals(IExchangeService exchangeService, Instrument instrument)
        {
            var periods = Enum.GetValues(typeof(TimePeriod)).Cast<TimePeriod>();
            
            _logger.Log(LogLevel.Info, $"---- {exchangeService.ExchangeConfig.ExchangeName} {instrument} ----");
            
            foreach (var timePeriod in exchangeService.ExchangeConfig.SupportedTimePeriods.Keys)
            {
                var candleData = await _candleService.GetExchangeCandleDataAsync(exchangeService.ExchangeConfig.ExchangeName, instrument, timePeriod);

                if (candleData == null)
                    continue;

                var signals = _signals.Values.Where(x => x.IsPeriodSupportedAsync(timePeriod).GetAwaiter().GetResult());

                _logger.Log(LogLevel.Info, $"{exchangeService.ExchangeConfig.ExchangeName} {instrument} {timePeriod} - SMA9 = { candleData.Indicate<SMA>(9).First() }");
                _logger.Log(LogLevel.Info, $"{exchangeService.ExchangeConfig.ExchangeName} {instrument} {timePeriod} - EMA9 = { candleData.Indicate<EMA>(9).First() }");
                _logger.Log(LogLevel.Info, $"{exchangeService.ExchangeConfig.ExchangeName} {instrument} {timePeriod} - ADX14 = { candleData.Indicate<ADX>(14).First() }");

                foreach (var signal in signals)
                {
                    var ss = await signal.ProcessSignalAsync(timePeriod, candleData.GetCandles().ToArray());
                    _logger.Log(LogLevel.Info, $"{exchangeService.ExchangeConfig.ExchangeName} {instrument} {timePeriod} - {signal.GetType().Name} = {ss}");
                }
            }
        }
    }
}