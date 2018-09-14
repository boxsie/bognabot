using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Bognabot.Bitmex.Request;
using Bognabot.Bitmex.Response;
using Bognabot.Bitmex.Socket;
using Bognabot.Bitmex.Socket.Responses;
using Bognabot.Data.Config;
using Bognabot.Data.Exchange;
using Bognabot.Data.Exchange.Enums;
using Bognabot.Data.Exchange.Models;
using Bognabot.Domain.Entities.Instruments;
using Bognabot.Services.Exchange;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace Bognabot.Bitmex
{
    public class BitmexService : IExchangeService
    {
        public ExchangeConfig ExchangeConfig { get; }
        public DateTimeOffset Now => BitmexUtils.Now();

        private readonly ILogger _logger;
        private readonly IExchangeSocketClient _socketClient;
        private readonly Dictionary<ExchangeChannel, List<IStreamSubscription>> _subscriptions;
        
        public BitmexService(ILogger logger, ExchangeConfig config)
        {
            _logger = logger;
            ExchangeConfig = config;

            _socketClient = new ExchangeSocketClient(logger);
            _subscriptions = new Dictionary<ExchangeChannel, List<IStreamSubscription>>();
        }

        public Task ConnectAsync()
        {
            return _socketClient.ConnectAsync(ExchangeConfig.WebSocketUrl, OnSocketReceive);
        }

        public void ConfigureMap(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<CandleResponse, CandleModel>()
                .ForMember(d => d.ExchangeName, o => o.MapFrom(s => ExchangeConfig.ExchangeName))
                .ForMember(d => d.Instrument, o => o.MapFrom(s => ToInstrumentType(s.Symbol)))
                .ForMember(d => d.Period, o => o.Ignore());

            cfg.CreateMap<TradeResponse, TradeModel>()
                .ForMember(d => d.Instrument, o => o.MapFrom(s => ToInstrumentType(s.Symbol)))
                .ForMember(d => d.Side, o => o.MapFrom(s => BitmexUtils.ToTradeType(s.Side)));

            cfg.CreateMap<BookResponse, BookModel>()
                .ForMember(d => d.Instrument, o => o.MapFrom(s => ToInstrumentType(s.Symbol)))
                .ForMember(d => d.Side, o => o.MapFrom(s => BitmexUtils.ToTradeType(s.Side)));
        }

        public async Task SubscribeToStreamAsync<T>(ExchangeChannel channel, IStreamSubscription subscription) where T : ExchangeModel
        {
            if (!_subscriptions.ContainsKey(channel))
                _subscriptions.Add(channel, new List<IStreamSubscription>());

            _subscriptions[channel].Add(subscription);

            await _socketClient.SubscribeAsync(GetSocketRequest(channel));
        }

        public async Task<List<CandleModel>> GetCandlesAsync(Instrument instrument, TimePeriod timePeriod, DateTimeOffset startTime, DateTimeOffset endTime)
        {
            var request = new CandleRequest
            {
                Symbol = ToSymbol(instrument),
                StartAt = 0,
                Count = 750,
                TimeInterval = ToTimePeriod(timePeriod),
                StartTime = startTime.ToUtcTimeString(),
                EndTime = endTime.ToUtcTimeString(),
            };

            var models = await GetAllAsync<CandleModel, CandleResponse, CandleRequest>(ExchangeConfig.SupportedRestChannels[ExchangeChannel.Candle], request);

            foreach (var candleModel in models)
                candleModel.Period = timePeriod;

            return models;
        }

        private async Task<List<T>> GetAllAsync<T, TY, TTy>(string path, TTy request) where T : ExchangeModel where TTy : ICollectionRequest
        {
            var stopwatch = new Stopwatch();
            var total = 0;
            var count = 0;

            var returnModels = new List<T>();

            using (var client = new ExchangeHttpClient(ExchangeConfig.RestUrl))
            {
                do
                {
                    stopwatch.Restart();
                    total += count;
                    request.StartAt = total;

                    var query = request.AsDictionary().BuildQueryString();
                    
                    var response = await client.GetAsync<TY>(path, query, 
                        BitmexUtils.GetHttpAuthHeaders(ExchangeConfig.RestUrl, HttpMethod.GET, path, query, ExchangeConfig.UserConfig.Key, ExchangeConfig.UserConfig.Secret));

                    if (response == null)
                        throw new NullReferenceException();

                    var models = response.Select(x => Mapper.Map<T>(x)).ToArray();
                    
                    returnModels.AddRange(models);

                    count = response.Length;

                    if (count < request.Count)
                        count = 0;

                    if (stopwatch.Elapsed < TimeSpan.FromSeconds(1.01))
                        await Task.Delay(TimeSpan.FromSeconds(1).Subtract(stopwatch.Elapsed));

                } while (count > 0);

                return returnModels;
            }
        }

        private async Task OnSocketReceive(string json)
        {
            var table = JObject.Parse(json)?["table"]?.Value<string>();

            if (table == null)
                return;

            var candleChannel = ExchangeConfig.SupportedWebsocketChannels[ExchangeChannel.Candle];

            if (table.Contains(candleChannel))
            {
                var timePeriod = ExchangeConfig.SupportedTimePeriods
                    .Select(x => new { x.Key, x.Value })
                    .FirstOrDefault(x => x.Value == table.Replace(candleChannel, ""))?.Key;

                if (timePeriod.HasValue)
                {
                    var candleModels = JsonConvert.DeserializeObject<BitmexSocketResponseContainer<CandleResponse>>(json).Data.Select(Mapper.Map<CandleModel>).ToArray();

                    foreach (var model in candleModels)
                        model.Period = timePeriod.Value;

                    if (_subscriptions.ContainsKey(ExchangeChannel.Candle))
                    {
                        var subs = _subscriptions[ExchangeChannel.Candle];

                        foreach (var subscription in subs)
                            await subscription.TriggerUpdate(candleModels);
                    }
                }
            }
        }

        private string GetSocketRequest(ExchangeChannel channel)
        {
            switch (channel)
            {
                case ExchangeChannel.Trade:
                    return BitmexUtils.GetSocketRequest(ExchangeConfig.SupportedWebsocketChannels[ExchangeChannel.Trade], ToSymbol(Instrument.BTCUSD));
                case ExchangeChannel.Book:
                    return BitmexUtils.GetSocketRequest(ExchangeConfig.SupportedWebsocketChannels[ExchangeChannel.Book], ToSymbol(Instrument.BTCUSD));
                case ExchangeChannel.Candle:
                    var paths = ExchangeConfig.SupportedTimePeriods.Values.Select(x => $"{ExchangeConfig.SupportedWebsocketChannels[ExchangeChannel.Candle]}{x}").ToList();
                    var args = paths.Select(x => new[] {ToSymbol(Instrument.BTCUSD)}).ToList();

                    return BitmexUtils.GetSocketRequest(paths, args);
                default:
                    throw new ArgumentOutOfRangeException(nameof(channel), channel, null);
            }
        }

        private Instrument? ToInstrumentType(string symbol)
        {
            var instrumentKvp = ExchangeConfig.SupportedInstruments.FirstOrDefault(x => x.Value == symbol);

            if (instrumentKvp.Value == null)
                throw new ArgumentOutOfRangeException();

            return instrumentKvp.Key;
        }

        private string ToSymbol(Instrument instrument)
        {
            var supportedInstruments = ExchangeConfig.SupportedInstruments;

            return supportedInstruments.ContainsKey(instrument)
                ? supportedInstruments[instrument]
                : throw new ArgumentOutOfRangeException();
        }

        private string ToTimePeriod(TimePeriod period)
        {
            var supportedPeriods = ExchangeConfig.SupportedTimePeriods;

            return supportedPeriods.ContainsKey(period)
                ? supportedPeriods[period]
                : throw new ArgumentOutOfRangeException();
        }
    }
}
