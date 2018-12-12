using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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
using Bognabot.Storage.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace Bognabot.Bitmex
{
    public class BitmexApi : ExchangeApi
    {
        public override DateTime Now => BitmexUtils.Now();

        private readonly ILogger _logger;
        
        public BitmexApi(ILogger logger, ExchangeConfig config) : base(logger, config)
        {
            _logger = logger;
        }

        protected override Task<Dictionary<string, string>> GetHttpAuthHeader(HttpMethod httpMethod, string requestPath, string requestData)
        {
            var sb = new StringBuilder();

            sb.Append(httpMethod.ToString());
            sb.Append("/api/v1");
            sb.Append(requestPath);

            if (httpMethod == HttpMethod.GET)
                sb.Append(requestData);

            sb.Append(Expires());

            if (httpMethod != HttpMethod.GET)
                sb.Append(requestData);

            var signatureMessage = sb.ToString();
            var signatureBytes = StorageUtils.EncryptHMACSHA256(Encoding.UTF8.GetBytes(ExchangeConfig.UserConfig.Secret), Encoding.UTF8.GetBytes(signatureMessage));

            return Task.FromResult(new Dictionary<string, string>
            {
                { "api-expires", Expires().ToString() },
                { "api-key", ExchangeConfig.UserConfig.Key },
                { "api-signature", StorageUtils.ByteArrayToHexString(signatureBytes) }
            });
        }

        protected override Task<string> GetSocketAuthRequest()
        {
            var message = $"GET/realtime{Expires()}";
            var signatureBytes = StorageUtils.EncryptHMACSHA256(Encoding.UTF8.GetBytes(ExchangeConfig.UserConfig.Secret), Encoding.UTF8.GetBytes(message));
            var sig = StorageUtils.ByteArrayToHexString(signatureBytes);

            return Task.FromResult($@"{{""op"": ""authKeyExpires"", ""args"": [""{ExchangeConfig.UserConfig.Key}"", {Expires()}, ""{sig}""]}}");
        }

        protected override Task<string> GetSocketRequest(ExchangeChannel channel, Instrument? instrument = null)
        {
            var result = "";

            switch (channel)
            {
                case ExchangeChannel.Trade:
                    result = GetSocketRequest(ExchangeConfig.SupportedWebsocketChannels[ExchangeChannel.Trade], instrument.HasValue ? ToSymbol(instrument.Value) : null);
                    break;
                case ExchangeChannel.Book:
                    result = GetSocketRequest(ExchangeConfig.SupportedWebsocketChannels[ExchangeChannel.Book], instrument.HasValue ? ToSymbol(instrument.Value) : null);
                    break;
                case ExchangeChannel.Candle:
                    var paths = ExchangeConfig.SupportedTimePeriods.Values
                        .Select(x => $"{ExchangeConfig.SupportedWebsocketChannels[ExchangeChannel.Candle]}{x}")
                        .ToList();

                    var args = instrument.HasValue 
                        ? paths.Select(x => new[] {ToSymbol(instrument.Value)}).ToList() 
                        : null;

                    result = GetSocketRequest(paths, args);
                    break;
                case ExchangeChannel.Position:
                    result = GetSocketRequest(ExchangeConfig.SupportedWebsocketChannels[ExchangeChannel.Position], instrument.HasValue ? $"filter={ToSymbol(instrument.Value)}" : null);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(channel), channel, null);
            }

            return Task.FromResult(result);
        }

        protected override async Task OnSocketReceive(string json)
        {
            var jsonObj = JObject.Parse(json);

            if (jsonObj == null)
                return;

            var table = jsonObj["table"]?.Value<string>();

            if (table == null)
            {
                ProcessNoneTableMessage(jsonObj);
                return;
            }

            if (await ProcessSocketCandleMessage(table, json))
                return;

            if (await ProcessSocketTradeMessage(table, json))
                return;

            if (await ProcessSocketPositionMessage(table, json))
                return;
        }

        private void ProcessNoneTableMessage(JObject jsonObj)
        {
            var success = jsonObj["success"]?.Value<string>();

            if (success != null)
            {
                var sub = jsonObj["subscribe"]?.Value<string>();

                if (sub != null)
                    _logger.Log(LogLevel.Info, $"Successfully subscribed to '{sub}' data stream");
            }

            var info = jsonObj["info"]?.Value<string>();

            if (info != null)
            {
                var version = jsonObj["version"]?.Value<string>();

                if (version != null)
                    _logger.Log(LogLevel.Info, $"{info} {version}");
            }

            var error = jsonObj["error"]?.Value<string>();

            if (error != null)
                _logger.Log(LogLevel.Error, $"{error} {jsonObj.ToString(Formatting.None)}");
        }

        //private async Task<TY[]> ProcessSocketMessage<T, TY>(ExchangeChannel channel, string json)
        //{
        //    switch (channel)
        //    {
        //        case ExchangeChannel.Trade:
        //            break;
        //        case ExchangeChannel.Book:
        //            break;
        //        case ExchangeChannel.Candle:
        //            break;
        //        case ExchangeChannel.Order:
        //            break;
        //        case ExchangeChannel.Position:
        //            break;
        //        default:
        //            throw new ArgumentOutOfRangeException(nameof(channel), channel, null);
        //    }
        //}

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
        
        private async Task<bool> ProcessSocketPositionMessage(string table, string json)
        {
            var positionChannel = ExchangeConfig.SupportedWebsocketChannels[ExchangeChannel.Position];

            if (!table.Contains(positionChannel))
                return false;

            var response = JsonConvert.DeserializeObject<BitmexSocketResponseContainer<PositionResponse>>(json).Data;

            if (response == null || !response.Any())
                return false;

            var first = response.First();

            await UpdateSubscriptions(
                ExchangeChannel.Position, 
                response.Where(x => ExchangeConfig.SupportedInstruments.ContainsValue(x.Symbol))
                        .Select(Mapper.Map<PositionDto>)
                        .ToArray());

            return true;
        }

        private static TY[] DeserialiseJsonToExchangeDto<T, TY>(string json) where TY : ExchangeDto
        {
            return JsonConvert.DeserializeObject<BitmexSocketResponseContainer<T>>(json).Data.Select(x => Mapper.Map<TY>(x)).ToArray();
        }

        private static string GetSocketRequest(string path, params string[] args)
        {
            if (!args.Any())
                throw new MissingFieldException();

            return $@"{{""op"": ""subscribe"", ""args"": [""{path}:{args[0]}""]}}";
        }

        private static string GetSocketRequest(IEnumerable<string> path, List<string[]> args)
        {
            if (!args.Any())
                throw new MissingFieldException();

            return $@"{{""op"": ""subscribe"", ""args"": [""{string.Join("\", \"", path.Select((x, i) => $"{x}:{args[i][0]}"))}""]}}";
        }

        private long Expires()
        {
            return Now.ToUnixTimestamp() + ExchangeConfig.AuthExpireSeconds;
        }
    }
}
