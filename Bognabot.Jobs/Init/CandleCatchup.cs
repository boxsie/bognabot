using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Bognabot.Config;
using Bognabot.Config.Enums;
using Bognabot.Data.Exchange;
using Bognabot.Data.Exchange.Contracts;
using Bognabot.Data.Models.Exchange;
using Bognabot.Data.Repository;
using Bognabot.Domain.Entities.Instruments;
using Bognabot.Jobs.Sync;
using Microsoft.Extensions.Logging;

namespace Bognabot.Jobs.Init
{
    public class CandleCatchup : InitJob
    {
        private readonly ILogger<CandleCatchup> _logger;
        private readonly RepositoryService _repoService;
        private readonly IEnumerable<IExchangeService> _exchangeServices;

        public CandleCatchup(ILogger<CandleCatchup> logger, RepositoryService repoService, IEnumerable<IExchangeService> exchangeServices)
        {
            _logger = logger;
            _repoService = repoService;
            _exchangeServices = exchangeServices;
        }

        public override async Task ExecuteAsync()
        {
            var instruments = Enum.GetValues(typeof(Instrument)).Cast<Instrument>();
            var periods = Enum.GetValues(typeof(TimePeriod)).Cast<TimePeriod>();

            foreach (var instrument in instruments)
            {
                var supportedExchanges = Cfg.GetExchangeConfigs();

                foreach (var exchange in supportedExchanges)
                {
                    var exchangeService = _exchangeServices.FirstOrDefault(x => x.Exchange == exchange.Exchange);

                    if (exchangeService == null)
                        continue;

                    var supportedPeriods = exchange.SupportedTimePeriods;

                    foreach (var period in supportedPeriods)
                        await exchangeService.GetCandlesAsync(
                            instrument,
                            period.Key, 
                            CalculateStartTime(period.Key, exchange.UserConfig.MaxDataPoints),
                            DateTimeOffset.Now, 
                            OnRecieve);
                }
            }
        }

        private async Task OnRecieve(CandleModel[] arg)
        {
            if (arg == null || !arg.Any())
                return;

            var first = arg.First();

            var candleRepo = await _repoService.GetCandleRepository(first.Exchange, first.Instrument, first.Period);

            _logger.Log(LogLevel.Debug, $"Inserting {arg.Length} candle records");

            var candles = arg.Select(Mapper.Map<Candle>);

            await candleRepo.CreateAsync(candles);
            
            _logger.Log(LogLevel.Information, $"Inserting {arg.Length} candle records is complete");
        }

        private DateTimeOffset CalculateStartTime(TimePeriod period, int dataPoints)
        {
            switch (period)
            {
                case TimePeriod.OneMinute:
                    return DateTimeOffset.Now.AddMinutes(-dataPoints);
                case TimePeriod.FiveMinutes:
                    return DateTimeOffset.Now.AddMinutes(-dataPoints * 5);
                case TimePeriod.FifteenMinutes:
                    return DateTimeOffset.Now.AddMinutes(-dataPoints * 15);
                case TimePeriod.OneHour:
                    return DateTimeOffset.Now.AddHours(-dataPoints);
                case TimePeriod.OneDay:
                    return DateTimeOffset.Now.AddDays(-dataPoints);
                default:
                    throw new ArgumentOutOfRangeException(nameof(period), period, null);
            }
        }
    }
}
