using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Bognabot.Bitmex.Core;
using Bognabot.Bitmex.Http;
using Bognabot.Bitmex.Http.Requests;
using Bognabot.Bitmex.Http.Responses;
using Bognabot.Bitmex.Socket;
using Bognabot.Bitmex.Socket.Responses;
using Bognabot.Config;
using Bognabot.Config.Core;
using Bognabot.Config.Enums;
using Bognabot.Data.Exchange;
using Bognabot.Data.Exchange.Contracts;
using Bognabot.Data.Models.Exchange;
using Bognabot.Domain.Entities.Instruments;

namespace Bognabot.Bitmex
{
    public class BitmexService : IExchangeService
    {
        public SupportedExchange Exchange => SupportedExchange.Bitmex;
        public DateTimeOffset Now => BitmexUtils.Now();

        public event Func<TradeModel[], Task> OnTradeReceived;
        public event Func<BookModel[], Task> OnBookReceived;

        private readonly BitmexSocketClient _bitmexSocketClient;
        private readonly BitmexHttpClient _bitmexHttpClient;
        private readonly ExchangeConfig _config;

        public BitmexService(BitmexSocketClient bitmexSocketClient, BitmexHttpClient bitmexHttpClient)
        {
            _bitmexSocketClient = bitmexSocketClient;
            _bitmexHttpClient = bitmexHttpClient;
            
            _config = Cfg.GetExchangeConfig(SupportedExchange.Bitmex);
        }

        public async Task SubscribeToStreams()
        {
            await _bitmexSocketClient.ConnectAsync();

            await _bitmexSocketClient.SubscribeAsync<TradeSocketResponse>(OnReceiveTrade, BitmexUtils.ToSymbol(Instrument.BTCUSD));
            await _bitmexSocketClient.SubscribeAsync<BookSocketResponse>(OnReceiveBook, BitmexUtils.ToSymbol(Instrument.BTCUSD));
        }

        public async Task GetCandlesAsync(Instrument instrument, TimePeriod timePeriod, DateTimeOffset startTime, DateTimeOffset endTime, Func<CandleModel[], Task> onRecieve)
        {
            var stopwatch = new Stopwatch();
            var total = 0;
            var count = 0;

            var request = new TradeCommandRequest
            {
                IsAuth = true,
                Path = _config.CandlePath,
                Symbol = BitmexUtils.ToSymbol(instrument),
                StartAt = 0,
                Count = 750,
                TimeInterval = timePeriod.ToBitmexTimePeriod(),
                StartTime = startTime.ToUtcTimeString(),
                EndTime = endTime.ToUtcTimeString(),
            };

            do
            {
                stopwatch.Restart();

                total += count;

                request.StartAt = total;

                var candles = await _bitmexHttpClient.GetAsync<TradeCommandRequest, TradeCommandResponse>(request);

                var candleModels = candles?.Select(Mapper.Map<CandleModel>).ToArray() ?? null;

                if (candleModels != null)
                {
                    foreach (var model in candleModels)
                        model.Period = timePeriod;
                }

                await onRecieve.Invoke(candleModels);

                count = candles?.Length ?? 0;

                if (stopwatch.Elapsed < TimeSpan.FromSeconds(1.01))
                    await Task.Delay(TimeSpan.FromSeconds(1).Subtract(stopwatch.Elapsed));

            }
            while (count > 0);
        }

        private async Task OnReceiveTrade(TradeSocketResponse[] arg)
        {
            if (OnTradeReceived != null)
            {
                var models = arg.Select(Mapper.Map<TradeModel>).ToArray();

                await OnTradeReceived.Invoke(models.ToArray());
            }
        }

        private async Task OnReceiveBook(BookSocketResponse[] arg)
        {
            if (OnBookReceived != null)
            {
                var models = arg.Select(Mapper.Map<BookModel>).ToArray();

                await OnBookReceived.Invoke(models);
            }
        }
    }
}
