using System.Collections.Generic;
using System.Threading.Tasks;
using Bognabot.Services.Exchange;
using Bognabot.Services.Jobs;

namespace Bognabot.Services
{
    public class ServiceManager
    {
        private readonly IEnumerable<IExchangeService> _exchangeServices;
        private readonly JobService _jobService;
        private readonly CandleService _candleService;

        public ServiceManager(IEnumerable<IExchangeService> exchangeServices, JobService jobService, CandleService candleService)
        {
            _exchangeServices = exchangeServices;
            _jobService = jobService;
            _candleService = candleService;
        }

        public async Task StartAsync()
        {
            foreach (var exchangeService in _exchangeServices)
                await exchangeService.StartAsync();
            
            await _candleService.StartAsync();
            await _jobService.StartAsync();
        }
    }
}