using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bognabot.Config;
using Bognabot.Data.Models.Exchange;
using Bognabot.Exchanges.Bitmex.Core;
using Bognabot.Exchanges.Bitmex.Streams;
using Bognabot.Exchanges.Bitmex.Trade;
using Bognabot.Exchanges.Core;

namespace Bognabot.Exchanges.Bitmex
{
    public interface IExchangeService
    {
        event Func<TradeModel[], Task> OnTradeReceived;
        event Func<BookModel[], Task> OnBookReceived;

        Task SubscribeToStreams();
        Task<CandleModel[]> GetCandles(TimePeriod candleSize, DateTimeOffset startTime, DateTimeOffset endTime);
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

            await _stream.SubscribeAsync<BitmexTradeStreamResponse>(OnReceiveTrade, App.Exchange.App.Bitmex.BtcUsdName);
            await _stream.SubscribeAsync<BitmexBookStreamResponse>(OnReceiveBook, App.Exchange.App.Bitmex.BtcUsdName);
        }

        public async Task<CandleModel[]> GetCandles(TimePeriod candleSize, DateTimeOffset startTime, DateTimeOffset endTime)
        {
            var request = new BitmexTradeCommandRequest
            {
                IsAuth = false,
                Path = App.Exchange.App.Bitmex.CandlePath,
                Symbol = App.Exchange.App.Bitmex.BtcUsdName,
                StartAt = 1,
                Count = 100,
                TimeInterval = candleSize.ToBitmexTimePeriod(),
                StartTime = startTime.ToUtcTimeString(),
                EndTime = endTime.ToUtcTimeString(),
            };

            var candles = await _command.GetAsync<BitmexTradeCommandRequest, BitmexTradeCommandResponse>(request);

            return candles.Select(x => new CandleModel
            {
                Instrument = InstrumentType.BtcUsd,
                Period = candleSize,
                High = x.High,
                Low = x.Low,
                Open = x.Open,
                Close = x.Close,
                Volume = x.Volume,
                Trades = x.Trades,
                Timestamp = x.Timestamp
            }).ToArray();
        }

        private async Task OnReceiveTrade(BitmexTradeStreamResponse[] arg)
        {
            if (OnTradeReceived != null)
            {
                var models = arg.Select(x => new TradeModel
                {
                    Instrument = InstrumentType.BtcUsd,
                    Price = x.Price,
                    Side = x.Side == "Buy" ? TradeType.Buy : TradeType.Sell,
                    Size = x.Size,
                    Timestamp = x.Timestamp
                });

                await OnTradeReceived.Invoke(models.ToArray());
            }
        }

        private async Task OnReceiveBook(BitmexBookStreamResponse[] arg)
        {
            if (OnBookReceived != null)
            {
                var models = arg.Select(x => new BookModel
                {
                    Instrument = InstrumentType.BtcUsd,
                    Price = x.Price,
                    Side = x.Side == "Buy" ? TradeType.Buy : TradeType.Sell,
                    Size = x.Size,
                    Timestamp = x.Timestamp
                });

                await OnBookReceived.Invoke(models.ToArray());
            }
        }
    }
}
