using System;
using System.Threading.Tasks;
using Bognabot.Bitmex;
using Bognabot.Data.Models.Exchange;
using Microsoft.Extensions.Logging;

namespace Bognabot.Daemon
{
    public class App
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<App> _logger;
        private readonly BitmexService _bitmexService;

        public App(IServiceProvider serviceProvider, ILogger<App> logger, BitmexService bitmexService)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _bitmexService = bitmexService;
        }

        public async void Run()
        {
            await _bitmexService.StartStreamingChannels();

            //_bitmexService.OnTradeReceived += OnTradeReceived;
            //_bitmexService.OnBookReceived += OnBookReceived;
        }

        //private static Task OnBookReceived(BookModel[] arg)
        //{
        //    foreach (var response in arg)
        //        Console.WriteLine($"BOOK: {response.Instrument.ToString().ToUpper()} - {response.Timestamp} - {response.Side.ToString()}:{response.Size} @ ${response.Price:N}");

        //    return Task.CompletedTask;
        //}

        //private static Task OnTradeReceived(TradeModel[] arg)
        //{
        //    foreach (var response in arg)
        //        Console.WriteLine($"TRADE: {response.Instrument.ToString().ToUpper()} - {response.Timestamp} - {response.Side.ToString()}:{response.Size} @ ${response.Price:N}");

        //    return Task.CompletedTask;
        //}
    }
}