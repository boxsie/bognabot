using System;
using System.Collections.Generic;
using System.Net.Http;
using Bognabot.Config;
using Bognabot.Exchanges.Bitmex.Trade;
using Bognabot.Exchanges.Core;
using Bognabot.Net;

namespace Bognabot.Exchanges.Bitmex.Core
{
    public class BitmexCommand : ExchangeCommand
    {
        protected override Dictionary<Type, ICommand> Commands { get; }

        public BitmexCommand()
        {
            Commands = new Dictionary<Type, ICommand>
            {
                { typeof(BitmexTradeCommandRequest), new BitmexTradeCommand() }
            };
        }

        protected override TextHttpClient GetClient(bool isAuth)
        {
            var client = new HttpClient { BaseAddress = new Uri(App.Exchange.App.Bitmex.RestUrl) };

            if (!isAuth)
                return new TextHttpClient(client);

            client.DefaultRequestHeaders.Add("api-expires", BitmexUtils.Expires().ToString());
            client.DefaultRequestHeaders.Add("api-key", App.Exchange.User.Bitmex.Key);
            client.DefaultRequestHeaders.Add("api-signature", BitmexUtils.CreateSignature(App.Exchange.User.Bitmex.Secret));

            return new TextHttpClient(client);
        }
    }
}