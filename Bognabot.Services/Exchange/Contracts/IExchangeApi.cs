using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Bognabot.Data.Config;
using Bognabot.Data.Exchange;
using Bognabot.Data.Exchange.Dtos;
using Bognabot.Data.Exchange.Enums;

namespace Bognabot.Services.Exchange.Contracts
{
    public interface IExchangeApi
    {
        DateTime Now { get; }

        Task StartAsync();
        Task SubscribeToStreamAsync<T>(ExchangeChannel channel, Instrument instrument, IStreamSubscription subscription) where T : ExchangeDto;

        Task<T> GetAsync<T, TY>(string path, Dictionary<string, string> request, Dictionary<string, string> authHeaders = null)
            where T : ExchangeDto
            where TY : IResponse;

        Task<List<T>> GetAllAsync<T, TY>(string path, ICollectionRequest request, Dictionary<string, string> authHeaders = null)
            where T : ExchangeDto
            where TY : IResponse;

        Task<T> PostAsync<T, TY>(string path, IRequest request, Dictionary<string, string> authHeaders = null)
            where T : ExchangeDto
            where TY : IResponse;

        Instrument? ToInstrumentType(string symbol);
        string ToSymbol(Instrument instrument);
        string ToTimePeriod(TimePeriod period);
    }
}