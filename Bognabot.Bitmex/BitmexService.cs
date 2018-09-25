using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Bognabot.Bitmex.Request;
using Bognabot.Bitmex.Response;
using Bognabot.Data.Config;
using Bognabot.Data.Exchange;
using Bognabot.Data.Exchange.Dtos;
using Bognabot.Data.Exchange.Enums;
using Bognabot.Services.Exchange;
using Bognabot.Services.Exchange.Contracts;
using Newtonsoft.Json;
using NLog;

namespace Bognabot.Bitmex
{
    public class OrderService
    {
        private readonly IEnumerable<IExchangeService> _exchangeServices;
        private readonly List<OrderDto> _openOrders;

        public OrderService(IEnumerable<IExchangeService> exchangeServices)
        {
            _exchangeServices = exchangeServices;

            _openOrders = new List<OrderDto>();
        }
    }

    public class BitmexService : IExchangeService
    {
        public ExchangeConfig ExchangeConfig { get; }
        public DateTime Now => _exchangeApi.Now;

        private readonly IExchangeApi _exchangeApi;
        private readonly List<OrderDto> _openOrders;

        public BitmexService(ILogger logger, ExchangeConfig config)
        {
            ExchangeConfig = config;

            _exchangeApi = new BitmexApi(logger, ExchangeConfig);
            _openOrders = new List<OrderDto>();
        }

        public void ConfigureMap(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<CandleResponse, CandleDto>()
                .ForMember(d => d.ExchangeName, o => o.MapFrom(s => ExchangeConfig.ExchangeName))
                .ForMember(d => d.Instrument, o => o.MapFrom(s => _exchangeApi.ToInstrumentType(s.Symbol)))
                .ForMember(d => d.Period, o => o.Ignore());

            cfg.CreateMap<TradeResponse, TradeDto>()
                .ForMember(d => d.ExchangeName, o => o.MapFrom(s => ExchangeConfig.ExchangeName))
                .ForMember(d => d.Instrument, o => o.MapFrom(s => _exchangeApi.ToInstrumentType(s.Symbol)))
                .ForMember(d => d.Side, o => o.MapFrom(s => BitmexUtils.ToTradeType(s.Side)))
                .ForMember(d => d.Timestamp, o => o.MapFrom(s => BitmexUtils.Now()));

            cfg.CreateMap<BookResponse, BookDto>()
                .ForMember(d => d.Instrument, o => o.MapFrom(s => _exchangeApi.ToInstrumentType(s.Symbol)))
                .ForMember(d => d.Side, o => o.MapFrom(s => BitmexUtils.ToTradeType(s.Side)));

            cfg.CreateMap<OrderResponse, OrderDto>()
                .ForMember(d => d.ExchangeName, o => o.MapFrom(s => ExchangeConfig.ExchangeName))
                .ForMember(d => d.Instrument, o => o.MapFrom(s => _exchangeApi.ToInstrumentType(s.Symbol)))
                .ForMember(d => d.Side, o => o.MapFrom(s => BitmexUtils.ToTradeType(s.Side)))
                .ForMember(d => d.Timestamp, o => o.MapFrom(s => BitmexUtils.Now()));
        }

        public async Task StartAsync()
        {
            await _exchangeApi.StartAsync();

            //var o = await PlaceOrder(Instrument.BTCUSD, 6427, -500);
        }

        public Task SubscribeToStreamAsync<T>(ExchangeChannel channel, Instrument instrument, IStreamSubscription subscription) where T : ExchangeDto
        {
            return _exchangeApi.SubscribeToStreamAsync<T>(channel, instrument, subscription);
        }

        public async Task<List<CandleDto>> GetCandlesAsync(Instrument instrument, TimePeriod timePeriod, DateTime startTime, DateTime endTime)
        {
            var request = new CandleRequest
            {
                Symbol = _exchangeApi.ToSymbol(instrument),
                StartAt = 0,
                Count = 750,
                TimeInterval = _exchangeApi.ToTimePeriod(timePeriod),
                StartTime = startTime.ToUtcTimeString(),
                EndTime = endTime.ToUtcTimeString(),
            };

            var channelPath = ExchangeConfig.SupportedRestChannels[ExchangeChannel.Candle];

            var authHeaders = BitmexUtils.GetHttpAuthHeaders(
                HttpMethod.GET,
                channelPath,
                $"?{request.AsDictionary().BuildQueryString()}",
                ExchangeConfig.UserConfig.Key,
                ExchangeConfig.UserConfig.Secret);

            var response = await _exchangeApi.GetAllAsync<CandleDto, CandleResponse>(channelPath, request, authHeaders);

            foreach (var candleModel in response)
                candleModel.Period = timePeriod;

            return response.ToList();
        }

        public async Task<OrderDto> PlaceOrder(Instrument instrument, double price, double quantity)
        {
            var request = new PlaceOrderRequest
            {
                Symbol = _exchangeApi.ToSymbol(instrument),
                OrderQty = quantity,
                Price = price,
                OrderType = "Limit"
            };

            var channelPath = ExchangeConfig.SupportedRestChannels[ExchangeChannel.Order];
            var jsonRequest = JsonConvert.SerializeObject(request);

            var authHeaders = BitmexUtils.GetHttpAuthHeaders(
                HttpMethod.POST,
                channelPath,
                request.AsDictionary().BuildQueryString(),
                ExchangeConfig.UserConfig.Key,
                ExchangeConfig.UserConfig.Secret);

            return await _exchangeApi.PostAsync<OrderDto, OrderResponse>(channelPath, request, authHeaders);
        }
    }
}