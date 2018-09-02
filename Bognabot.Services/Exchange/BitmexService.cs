using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bognabot.Config;
using Bognabot.Data.Models.Exchange;
using Bognabot.Exchanges.Bitmex.Core;
using Bognabot.Exchanges.Bitmex.Streams;
using Bognabot.Exchanges.Bitmex.Trade;
using Bognabot.Services.Core;
using Bognabot.Services.Exchange;
using AutoMapper;
using Bognabot.Data.Context;
using Bognabot.Domain.Entities.Instruments;
using Microsoft.EntityFrameworkCore;

namespace Bognabot.Services.Exchange
{
    public class CandleSyncService
    {
        private readonly IEnumerable<IExchangeService> _exchangeServices;

        private readonly bool _isSyncing;
        private DateTimeOffset? _lastCandle;

        public CandleSyncService(IEnumerable<IExchangeService> exchangeServices)
        {
            _exchangeServices = exchangeServices;

            _isSyncing = false;
            _lastCandle = null;
        }

        public async Task StartSync(TimeSpan interval, CancellationToken cancellationToken)
        {
            await SyncCatchup();

            while (!cancellationToken.IsCancellationRequested)
            {
                if (!_isSyncing)
                    await SyncCandles();

                await Task.Delay(interval, cancellationToken);
            }
        }

        private async Task SyncCatchup()
        {
            using (var db = new BognabotContext())
            {
                foreach (var exchange in _exchangeServices)
                {
                    var lastCandle = await db.BTCUSD.LastOrDefaultAsync(x => x.ExchangeType == exchange.Exchange);

                    var startTime = lastCandle?.CreatedOn.AddSeconds(1) 
                                    ?? exchange.Now().AddDays(-App.Exchange.User.HistoryDays);

                    await exchange.GetCandles(TimePeriod.OneMinute, startTime, exchange.Now(), OnRecieve);
                }
            }
        }

        private async Task OnRecieve(CandleModel[] arg)
        {
            using (var db = new BognabotContext())
            {
                var candles = arg.Select(x => new InstrumentCandle
                {

                }).ToArray();

                _lastCandle = candles.Last().CreatedOn;

                await db.BTCUSD.AddRangeAsync(candles);
                await db.SaveChangesAsync();
            }
        }

        public async Task SyncCandles()
        {
            
        }
    }

    public class BitmexService : IExchangeService
    {
        public event Func<TradeModel[], Task> OnTradeReceived;
        public event Func<BookModel[], Task> OnBookReceived;

        private readonly BitmexStream _stream;
        private readonly BitmexCommand _command;

        public BitmexService(BitmexStream stream, BitmexCommand command)
        {
            _stream = stream;
            _command = command;
        }

        public async Task SubscribeToStreams()
        {
            await _stream.ConnectAsync();

            await _stream.SubscribeAsync<BitmexTradeStreamResponse>(OnReceiveTrade, BitmexUtils.ToSymbol(InstrumentType.BtcUsd));
            await _stream.SubscribeAsync<BitmexBookStreamResponse>(OnReceiveBook, BitmexUtils.ToSymbol(InstrumentType.BtcUsd));
        }

        public async Task GetCandles(TimePeriod candleSize, DateTimeOffset startTime, DateTimeOffset endTime, Func<CandleModel[], Task> onRecieve)
        {
            var request = BitmexRequestFactory.GetTradeRequest(InstrumentType.BtcUsd, candleSize, startTime, endTime);

            var stopwatch = new Stopwatch();
            var total = 0;
            var count = 0;

            do
            {
                stopwatch.Restart();

                total += count;

                request.StartAt = total;

                var candles = await _command.GetAsync<BitmexTradeCommandRequest, BitmexTradeCommandResponse>(request);

                await onRecieve.Invoke(candles?.Select(Mapper.Map<CandleModel>).ToArray() ?? null);

                count = candles?.Length ?? 0;

                await Task.Delay(TimeSpan.FromSeconds(1).Subtract(stopwatch.Elapsed));
            } while (count == 0);
        }

        public DateTimeOffset Now()
        {
            return BitmexUtils.Now();
        }

        private async Task OnReceiveTrade(BitmexTradeStreamResponse[] arg)
        {
            if (OnTradeReceived != null)
            {
                var models = arg.Select(Mapper.Map<TradeModel>).ToArray();

                await OnTradeReceived.Invoke(models.ToArray());
            }
        }

        private async Task OnReceiveBook(BitmexBookStreamResponse[] arg)
        {
            if (OnBookReceived != null)
            {
                var models = arg.Select(Mapper.Map<BookModel>).ToArray();

                await OnBookReceived.Invoke(models);
            }
        }
    }
}
