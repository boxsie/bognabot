using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bognabot.Config.Enums;
using Bognabot.Data.Exchange;
using Bognabot.Data.Exchange.Contracts;
using Bognabot.Data.Repository;
using Bognabot.Domain.Entities.Instruments;
using Microsoft.Extensions.Logging;

namespace Bognabot.Jobs.Sync
{
    public class CandleSync : SyncJob
    {
        private readonly RepositoryService _repositoryService;
        private readonly IEnumerable<IExchangeService> _exchangeServices;
        
        public CandleSync(RepositoryService repositoryService, IEnumerable<IExchangeService> exchangeServices, ILogger<CandleSync> logger) : base(logger, 30)
        {
            _repositoryService = repositoryService;
            _exchangeServices = exchangeServices;
        }
        
        protected override async Task<string> ExecuteAsync()
        {
            var candles = await _repositoryService.GetCandleRepository(SupportedExchange.Bitmex, Instrument.BTCUSD, TimePeriod.OneMinute);

            

            return null;
        }
    }
}