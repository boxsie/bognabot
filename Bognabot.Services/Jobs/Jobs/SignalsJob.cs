using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bognabot.Data.Exchange.Enums;
using Bognabot.Services.Exchange;
using Bognabot.Services.Jobs.Core;
using Bognabot.Services.Repository;
using Bognabot.Trader;
using NLog;
using AutoMapper;
using Bognabot.Domain.Entities.Instruments;
using Bognabot.Services.Trader;

namespace Bognabot.Services.Jobs.Jobs
{
    public class SignalsJob : ExchangeSyncJob
    {
        private readonly ILogger _logger;
        private readonly TraderService _traderService;

        public SignalsJob(ILogger logger, IEnumerable<IExchangeService> exchangeServices, TraderService traderService)
            : base(logger, exchangeServices, 10)
        {
            _logger = logger;
            _traderService = traderService;
        }

        protected override async Task ExecuteOnExchangeAsync(IExchangeService exchangeService, Instrument instrument)
        {
            await _traderService.ProcessSignals(exchangeService, instrument);
        }
    }
}