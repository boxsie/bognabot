using System;
using System.Collections.Generic;
using System.Linq;
using Bognabot.Config;
using Bognabot.Exchanges.Bitmex.Streams;
using Bognabot.Exchanges.Bitmex.Trade;
using Bognabot.Exchanges.Core;
using Newtonsoft.Json.Linq;

namespace Bognabot.Exchanges.Bitmex.Core
{
    public class BitmexStream : ExchangeStream
    {
        protected override Uri DataUri { get; }
        protected override Dictionary<Type, IStreamChannel> Channels { get; }

        public BitmexStream()
        {
            DataUri = new Uri(App.Exchange.App.Bitmex.WebSocketUrl);

            Channels = new Dictionary<Type, IStreamChannel>
            {
                { typeof(BitmexTradeStreamResponse), new BitmexStreamChannel<BitmexTradeStreamResponse>(App.Exchange.App.Bitmex.TradePath) },
                { typeof(BitmexBookStreamResponse), new BitmexStreamChannel<BitmexBookStreamResponse>(App.Exchange.App.Bitmex.BookPath) }
            };
        }

        protected override string GetAuthRequest()
        {
            return $@"{{""op"": ""authKeyExpires"", ""args"": [""{App.Exchange.User.Bitmex.Key}"", {BitmexUtils.Expires()}, ""{BitmexUtils.CreateSignature(App.Exchange.User.Bitmex.Secret)}""]}}";
        }

        protected override StreamResponse[] ParseResponseJson(string json)
        {
            var jObject = JObject.Parse(json);

            foreach (var channel in Channels)
            {
                if (channel.Value.ChannelName == jObject?["table"]?.Value<string>())
                    return channel.Value.GetResponses(json);
            }

            return null;
        }
    }
}