using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Bognabot.Config;
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
            foreach (var exchangeService in _exchangeServices)
            {
                await exchangeService.GetCandlesAsync(TimePeriod.OneMinute, DateTimeOffset.Now.AddDays(-Cfg.Exchange.User.HistoryDays), DateTimeOffset.Now, OnRecieve);
            }
        }

        private async Task OnRecieve(CandleModel[] arg)
        {
            if (arg == null || !arg.Any())
                return;

            var first = arg.First();

            var candleRepo = await _repoService.GetCandleRepository(first.ExchangeType, first.Instrument, TimePeriod.OneMinute);

            _logger.Log(LogLevel.Debug, $"Inserting {arg.Length} candle records");

            foreach (var model in arg)
                await candleRepo.Create(Mapper.Map<Candle>(model));
            
            _logger.Log(LogLevel.Debug, $"Inserting {arg.Length} candle records is complete");
        }
    }
}
