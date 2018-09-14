using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Bognabot.Data.Config;
using Bognabot.Data.Exchange.Enums;
using Bognabot.Data.Exchange.Models;
using NLog;

namespace Bognabot.Services.Exchange
{
    public interface IStreamSubscription
    {
        Task TriggerUpdate(object obj);
    }

    public class StreamSubscription<T> : IStreamSubscription
    {
        public T[] Latest { get; set; }

        private readonly Func<T[], Task> _onUpdate;

        public StreamSubscription(Func<T[], Task> onUpdate)
        {
            _onUpdate = onUpdate;
        }

        public Task TriggerUpdate(object obj)
        {
            Latest = (T[])obj;

            return _onUpdate.Invoke(Latest);
        }
    }

    public interface IExchangeService
    {
        ExchangeConfig ExchangeConfig { get; }
        DateTimeOffset Now { get; }
        
        void ConfigureMap(IMapperConfigurationExpression cfg);

        Task ConnectAsync();
        Task SubscribeToStreamAsync<T>(ExchangeChannel channel, IStreamSubscription subscription) where T : ExchangeModel;
        Task<List<CandleModel>> GetCandlesAsync(Instrument instrument, TimePeriod timePeriod, DateTimeOffset startTime, DateTimeOffset endTime);
    }
}