using System;
using System.Threading.Tasks;
using Bognabot.Config;
using Bognabot.Exchanges.Bitmex;
using Bognabot.Exchanges.Bitmex.Core;
using Bognabot.Exchanges.Bitmex.Streams;
using Bognabot.Exchanges.Bitmex.Trade;
using Bognabot.Exchanges.Core;
using Bognabot.Net;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bognabot.Daemon
{
    public class App
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<App> _logger;
        private readonly BitmexStream _bitmexDataStream;

        public App(IServiceProvider serviceProvider, ILogger<App> logger, BitmexStream _bitmexDataStream)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            this._bitmexDataStream = _bitmexDataStream;
        }

        public async void Run()
        {
            await _bitmexDataStream.ConnectAsync();

            await _bitmexDataStream.SubscribeAsync<BitmexTradeStreamResponse>(OnReceive, Config.App.Exchange.App.Bitmex.BtcUsdName);
            await _bitmexDataStream.SubscribeAsync<BitmexBookStreamResponse>(OnReceive, Config.App.Exchange.App.Bitmex.BtcUsdName);
        }

        private Task OnReceive<T>(T[] arg) where T : StreamResponse
        {
            foreach (var response in arg)
                Console.WriteLine(JObject.Parse(JsonConvert.SerializeObject(response)));

            return Task.CompletedTask;
        }
    }
}