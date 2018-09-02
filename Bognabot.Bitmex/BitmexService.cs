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
using Bognabot.Data.Exchange;
using Bognabot.Data.Exchange.Contracts;
using Bognabot.Data.Models.Exchange;
using Bognabot.Domain.Entities.Instruments;

namespace Bognabot.Bitmex
{
    public class BitmexService : IExchangeService
    {
        public ExchangeType ExchangeType => ExchangeType.Bitmex;
        public DateTimeOffset Now => BitmexUtils.Now();

        public event Func<TradeModel[], Task> OnTradeReceived;
        public event Func<BookModel[], Task> OnBookReceived;

        private readonly BitmexSocketClient _bitmexSocketClient;
        private readonly BitmexHttpClient _bitmexHttpClient;

        public BitmexService(BitmexSocketClient bitmexSocketClient, BitmexHttpClient bitmexHttpClient)
        {
            _bitmexSocketClient = bitmexSocketClient;
            _bitmexHttpClient = bitmexHttpClient;
        }

        public async Task SubscribeToStreams()
        {
            await _bitmexSocketClient.ConnectAsync();

            await _bitmexSocketClient.SubscribeAsync<TradeSocketResponse>(OnReceiveTrade, BitmexUtils.ToSymbol(InstrumentType.BTCUSD));
            await _bitmexSocketClient.SubscribeAsync<BookSocketResponse>(OnReceiveBook, BitmexUtils.ToSymbol(InstrumentType.BTCUSD));
        }

        public async Task GetCandlesAsync(TimePeriod candleSize, DateTimeOffset startTime, DateTimeOffset endTime, Func<CandleModel[], Task> onRecieve)
        {
            var request = RequestFactory.GetTradeRequest(InstrumentType.BTCUSD, candleSize, startTime, endTime);

            var stopwatch = new Stopwatch();
            var total = 0;
            var count = 0;

            do
            {
                stopwatch.Restart();

                total += count;

                request.StartAt = total;

                var candles = await _bitmexHttpClient.GetAsync<TradeCommandRequest, TradeCommandResponse>(request);

                await onRecieve.Invoke(candles?.Select(Mapper.Map<CandleModel>).ToArray() ?? null);

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
