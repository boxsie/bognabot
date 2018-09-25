using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Bognabot.Bitmex.Response;
using Bognabot.Data.Config;
using Bognabot.Data.Exchange;
using Bognabot.Data.Exchange.Dtos;
using Bognabot.Data.Exchange.Enums;
using Bognabot.Domain.Entities.Instruments;
using Bognabot.Services.Exchange;
using Bognabot.Services.Exchange.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace Bognabot.Bitmex
{
    public class BitmexApi : ExchangeApi
    {
        public override DateTime Now => BitmexUtils.Now();

        private readonly ILogger _logger;
        private readonly IExchangeSocketClient _socketClient;
        private readonly Dictionary<ExchangeChannel, List<IStreamSubscription>> _subscriptions;
        
        public BitmexApi(ILogger logger, ExchangeConfig config) : base(logger, config)
        {
            _logger = logger;

            _socketClient = new ExchangeSocketClient(logger);
            _subscriptions = new Dictionary<ExchangeChannel, List<IStreamSubscription>>();
        }

        protected override async Task OnSocketReceive(string json)
        {
            var table = JObject.Parse(json)?["table"]?.Value<string>();

            if (table == null)
                return;

            if (await ProcessSocketCandleMessage(table, json))
                return;

            if (await ProcessSocketTradeMessage(table, json))
                return;
        }

        protected override Task<string> GetSocketRequest(Instrument instrument, ExchangeChannel channel)
        {
            var result = "";

            switch (channel)
            {
                case ExchangeChannel.Trade:
                    result = BitmexUtils.GetSocketRequest(ExchangeConfig.SupportedWebsocketChannels[ExchangeChannel.Trade], ToSymbol(instrument));
                    break;
                case ExchangeChannel.Book:
                    result = BitmexUtils.GetSocketRequest(ExchangeConfig.SupportedWebsocketChannels[ExchangeChannel.Book], ToSymbol(instrument));
                    break;
                case ExchangeChannel.Candle:
                    var paths = ExchangeConfig.SupportedTimePeriods.Values.Select(x => $"{ExchangeConfig.SupportedWebsocketChannels[ExchangeChannel.Candle]}{x}").ToList();
                    var args = paths.Select(x => new[] {ToSymbol(instrument)}).ToList();

                    result = BitmexUtils.GetSocketRequest(paths, args);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(channel), channel, null);
            }

            return Task.FromResult(result);
        }

        private async Task<bool> ProcessSocketCandleMessage(string table, string json)
        {
            var candleChannel = ExchangeConfig.SupportedWebsocketChannels[ExchangeChannel.Candle];

            if (!table.Contains(candleChannel))
                return false;

            var timePeriod = ExchangeConfig.SupportedTimePeriods
                .Select(x => new { x.Key, x.Value })
                .FirstOrDefault(x => x.Value == table.Replace(candleChannel, ""))?.Key;

            if (!timePeriod.HasValue)
                return false;

            var candleModels = DeserialiseJsonToExchangeDto<CandleResponse, CandleDto>(json);

            foreach (var model in candleModels)
                model.Period = timePeriod.Value;

            await UpdateSubscriptions(ExchangeChannel.Candle, candleModels);

            return true;
        }

        private async Task<bool> ProcessSocketTradeMessage(string table, string json)
        {
            var tradeChannel = ExchangeConfig.SupportedWebsocketChannels[ExchangeChannel.Trade];

            if (!table.Contains(tradeChannel))
                return false;

            await UpdateSubscriptions(ExchangeChannel.Trade, DeserialiseJsonToExchangeDto<TradeResponse, TradeDto>(json));

            return true;
        }

        private static TY[] DeserialiseJsonToExchangeDto<T, TY>(string json) where TY : ExchangeDto
        {
            return JsonConvert.DeserializeObject<BitmexSocketResponseContainer<T>>(json).Data.Select(x => Mapper.Map<TY>(x)).ToArray();
        }
    }
}
