using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bognabot.Data.Exchange.Enums;
using Bognabot.Services.Exchange;
using Bognabot.Services.Exchange.Contracts;
using NLog;
using Quartz;

namespace Bognabot.Services.Jobs.Core
{
    public abstract class ExchangeSyncJob : SyncJob
    {
        protected readonly IEnumerable<IExchangeService> ExchangeServices;

        protected abstract Task ExecuteOnExchangeAsync(IExchangeService exchangeService, Instrument instrument);

        protected ExchangeSyncJob(ILogger logger, IEnumerable<IExchangeService> exchangeServices, int intervalSeconds, DateTime? startTime = null) 
            : base(logger, intervalSeconds, startTime)
        {
            ExchangeServices = exchangeServices;
        }

        protected override Task ExecuteAsync() { return Task.CompletedTask; }

        public override async Task Execute(IJobExecutionContext context)
        {
            try
            {
                var instruments = Enum.GetValues(typeof(Instrument)).Cast<Instrument>();

                foreach (var instrument in instruments)
                {
                    foreach (var exchange in ExchangeServices)
                    {
                        await ExecuteOnExchangeAsync(exchange, instrument);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, $"{ex.Message}\r\n{ex.InnerException}");
                Logger.Log(LogLevel.Error, string.Join("\r\n", ex.StackTrace));
            }
        }
    }
}