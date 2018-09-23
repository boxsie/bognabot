using System;
using System.Collections;
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
using Bognabot.Data.Exchange.Dtos;
using Bognabot.Data.Exchange.Enums;
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
        public DateTime Now => BitmexUtils.Now();

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

        public Task StartAsync()
        {
            return _socketClient.ConnectAsync(ExchangeConfig.WebSocketUrl, OnSocketReceive);
        }

        public void ConfigureMap(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<CandleResponse, CandleDto>()
                .ForMember(d => d.ExchangeName, o => o.MapFrom(s => ExchangeConfig.ExchangeName))
                .ForMember(d => d.Instrument, o => o.MapFrom(s => ToInstrumentType(s.Symbol)))
                .ForMember(d => d.Period, o => o.Ignore());

            cfg.CreateMap<TradeResponse, TradeDto>()
                .ForMember(d => d.ExchangeName, o => o.MapFrom(s => ExchangeConfig.ExchangeName))
                .ForMember(d => d.Instrument, o => o.MapFrom(s => ToInstrumentType(s.Symbol)))
                .ForMember(d => d.Side, o => o.MapFrom(s => BitmexUtils.ToTradeType(s.Side)))
                .ForMember(d => d.Timestamp, o => o.MapFrom(s => BitmexUtils.Now()));

            cfg.CreateMap<BookResponse, BookDto>()
                .ForMember(d => d.Instrument, o => o.MapFrom(s => ToInstrumentType(s.Symbol)))
                .ForMember(d => d.Side, o => o.MapFrom(s => BitmexUtils.ToTradeType(s.Side)));
        }

        public async Task SubscribeToStreamAsync<T>(ExchangeChannel channel, Instrument instrument, IStreamSubscription subscription) where T : ExchangeDto
        {
            if (!ExchangeConfig.SupportedWebsocketChannels.ContainsKey(channel))
                return;

            if (!_subscriptions.ContainsKey(channel))
                _subscriptions.Add(channel, new List<IStreamSubscription>());

            _subscriptions[channel].Add(subscription);

            await _socketClient.SubscribeAsync(GetSocketRequest(instrument, channel));
        }

        public async Task<List<CandleDto>> GetCandlesAsync(Instrument instrument, TimePeriod timePeriod, DateTime startTime, DateTime endTime)
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

            var models = await GetAllAsync<CandleDto, CandleResponse, CandleRequest>(ExchangeConfig.SupportedRestChannels[ExchangeChannel.Candle], request);

            foreach (var candleModel in models)
                candleModel.Period = timePeriod;

            return models.ToList();
        }

        private async Task<List<T>> GetAllAsync<T, TY, TTy>(string path, TTy request)
            where T : ExchangeDto where TTy : ICollectionRequest
        {
            var stopwatch = new Stopwatch();
            var total = 0;
            var count = 0;

            var returnModels = new List<T>();

            do
            {
                using (var client = new ExchangeHttpClient(ExchangeConfig.RestUrl))
                {
                    stopwatch.Restart();
                    total += count;
                    request.StartAt = total;

                    var query = request.AsDictionary().BuildQueryString();

                    var response = await client.GetAsync<TY>(path, query,
                        BitmexUtils.GetHttpAuthHeaders(ExchangeConfig.RestUrl, HttpMethod.GET, path, query,
                            ExchangeConfig.UserConfig.Key, ExchangeConfig.UserConfig.Secret));

                    if (response == null)
                        throw new NullReferenceException();

                    var models = response.Select(x => Mapper.Map<T>(x)).ToArray();

                    returnModels.AddRange(models);

                    count = response.Length;

                    if (count < request.Count)
                        count = 0;

                    if (stopwatch.Elapsed < TimeSpan.FromSeconds(1.01))
                        await Task.Delay(TimeSpan.FromSeconds(1.5).Subtract(stopwatch.Elapsed));
                }

            } while (count > 0);

            return returnModels;
        }

        private async Task OnSocketReceive(string json)
        {
            var table = JObject.Parse(json)?["table"]?.Value<string>();

            if (table == null)
                return;

            if (await ProcessSocketCandleMessage(table, json))
                return;

            if (await ProcessSocketTradeMessage(table, json))
                return;
        }

        private async Task<bool> ProcessSocketCandleMessage(string table, string json)
        {
            const ExchangeChannel channel = ExchangeChannel.Candle;
            var candleChannel = ExchangeConfig.SupportedWebsocketChannels[channel];

            if (table.Contains(candleChannel))
            {
                var timePeriod = ExchangeConfig.SupportedTimePeriods
                    .Select(x => new { x.Key, x.Value })
                    .FirstOrDefault(x => x.Value == table.Replace(candleChannel, ""))?.Key;

                if (!timePeriod.HasValue)
                    return false;

                var candleModels = DeserialiseJsonToModel<CandleResponse, CandleDto>(json);

                foreach (var model in candleModels)
                    model.Period = timePeriod.Value;

                await UpdateSubscriptions(channel, candleModels);
                return true;
            }

            return false;
        }

        private async Task<bool> ProcessSocketTradeMessage(string table, string json)
        {
            const ExchangeChannel channel = ExchangeChannel.Trade;
            var tradeChannel = ExchangeConfig.SupportedWebsocketChannels[channel];

            if (!table.Contains(tradeChannel))
                return false;

            await UpdateSubscriptions(channel, DeserialiseJsonToModel<TradeResponse, TradeDto>(json));
            return true;
        }

        private TY[] DeserialiseJsonToModel<T, TY>(string json) where TY : ExchangeDto
        {
            return JsonConvert.DeserializeObject<BitmexSocketResponseContainer<T>>(json).Data.Select(x => Mapper.Map<TY>(x)).ToArray();
        }

        private async Task UpdateSubscriptions(ExchangeChannel channel, IEnumerable models)
        {
            if (_subscriptions.ContainsKey(channel))
            {
                var subs = _subscriptions[channel];

                foreach (var subscription in subs)
                    await subscription.TriggerUpdate(models);
            }
        }

        private string GetSocketRequest(Instrument instrument, ExchangeChannel channel)
        {
            switch (channel)
            {
                case ExchangeChannel.Trade:
                    return BitmexUtils.GetSocketRequest(ExchangeConfig.SupportedWebsocketChannels[ExchangeChannel.Trade], ToSymbol(instrument));
                case ExchangeChannel.Book:
                    return BitmexUtils.GetSocketRequest(ExchangeConfig.SupportedWebsocketChannels[ExchangeChannel.Book], ToSymbol(instrument));
                case ExchangeChannel.Candle:
                    var paths = ExchangeConfig.SupportedTimePeriods.Values.Select(x => $"{ExchangeConfig.SupportedWebsocketChannels[ExchangeChannel.Candle]}{x}").ToList();
                    var args = paths.Select(x => new[] {ToSymbol(instrument)}).ToList();

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
