using System;
using System.Collections.Generic;
using System.Text;
using Bognabot.Bitmex.Core;
using Bognabot.Bitmex.Socket.Responses;
using Bognabot.Config;
using Bognabot.Config.Core;
using Bognabot.Config.Enums;
using Bognabot.Data.Exchange;
using Bognabot.Net.Api;
using Bognabot.Storage.Core;
using Newtonsoft.Json.Linq;

namespace Bognabot.Bitmex.Socket
{
    public class BitmexSocketClient : ExchangeSocketClient
    {
        protected override Uri DataUri { get; }
        protected override Dictionary<Type, ISocketChannel> Channels { get; }

        private readonly ExchangeConfig _config;
        
        public BitmexSocketClient()
        {
            _config = Cfg.GetExchangeConfig(SupportedExchange.Bitmex);

            DataUri = new Uri(_config.WebSocketUrl);

            Channels = new Dictionary<Type, ISocketChannel>
            {
                { typeof(TradeSocketResponse), new SocketChannel<TradeSocketResponse>(_config.TradePath) },
                { typeof(BookSocketResponse), new SocketChannel<BookSocketResponse>(_config.BookPath) }
            };
        }

        protected override string GetAuthRequest()
        {
            return $@"{{""op"": ""authKeyExpires"", ""args"": [""{_config.UserConfig.Key}"", {BitmexUtils.Expires()}, ""{CreateSignature(_config.UserConfig.Secret)}""]}}";
        }

        protected override SocketResponse[] ParseResponseJson(string json)
        {
            var jObject = JObject.Parse(json);

            foreach (var channel in Channels)
            {
                if (channel.Value.ChannelName == jObject?["table"]?.Value<string>())
                    return channel.Value.GetResponses(json);
            }

            return null;
        }

        private static string CreateSignature(string secret)
        {
            var message = $"GET/realtime{BitmexUtils.Expires()}";
            var signatureBytes = StorageUtils.EncryptHMACSHA256(Encoding.UTF8.GetBytes(secret), Encoding.UTF8.GetBytes(message));

            return StorageUtils.ByteArrayToHexString(signatureBytes);
        }
    }
}