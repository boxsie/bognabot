using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Bognabot.Bitmex.Http;
using Bognabot.Bitmex.Http.Requests;
using Bognabot.Bitmex.Http.Responses;
using Bognabot.Bitmex.Socket;
using Bognabot.Bitmex.Socket.Responses;
using Bognabot.Data.Config;
using Bognabot.Data.Exchange;
using Bognabot.Data.Exchange.Contracts;
using Bognabot.Data.Exchange.Enums;
using Bognabot.Data.Exchange.Models;
using Bognabot.Data.Models.Exchange;
using Bognabot.Domain.Entities.Instruments;
using Bognabot.Services.Exchange;
using NLog;

namespace Bognabot.Bitmex
{
    public class Func<Task, Action<
    {
        public Func(Action<TradeModel[]> onRecieve)
        {
            OnRecieve = onRecieve;
        }

        public Action<TradeModel[]> OnRecieve { get; private set; }
    }

    public class BitmexService : BaseExchangeService
    {
        public override DateTimeOffset Now => BitmexUtils.Now();

        private readonly BitmexSocketClient _bitmexSocketClient;
        private readonly BitmexHttpClient _bitmexHttpClient;
        
        public BitmexService(ILogger logger, ExchangeConfig config) : base(config)
        {
            var channels = new Dictionary<string, ISocketChannel>
            {
                { config.TradePathWebSocket, new SocketChannel<TradeSocketResponse>(config.TradePathWebSocket) },
                { config.BookPathWebSocket, new SocketChannel<BookSocketResponse>(config.BookPathWebSocket) },
                { $"{config.BookPathWebSocket}{ToTimePeriod(TimePeriod.OneMinute)}", new SocketChannel<CandleSocketResponse>(config.BookPathWebSocket) }
            };

            _bitmexSocketClient = new BitmexSocketClient(logger, config, channels);
            _bitmexHttpClient = new BitmexHttpClient(logger, config);
        }

        public override void ConfigureMap(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<TradeCommandResponse, CandleModel>()
                .ForMember(d => d.ExchangeName, o => o.MapFrom(s => ExchangeConfig.ExchangeName))
                .ForMember(d => d.Instrument, o => o.MapFrom(s => ToInstrumentType(s.Symbol)))
                .ForMember(d => d.Period, o => o.Ignore());

            cfg.CreateMap<TradeSocketResponse, TradeModel>()
                .ForMember(d => d.Instrument, o => o.MapFrom(s => ToInstrumentType(s.Symbol)))
                .ForMember(d => d.Side, o => o.MapFrom(s => BitmexUtils.ToTradeType(s.Side)));

            cfg.CreateMap<BookSocketResponse, BookModel>()
                .ForMember(d => d.Instrument, o => o.MapFrom(s => ToInstrumentType(s.Symbol)))
                .ForMember(d => d.Side, o => o.MapFrom(s => BitmexUtils.ToTradeType(s.Side)));
        }

        public override async Task StartStreamingChannels()
        {
            await _bitmexSocketClient.ConnectAsync();

            await _bitmexSocketClient.SubscribeAsync<TradeSocketResponse>(OnReceiveTrade, ToSymbol(Instrument.BTCUSD));
            await _bitmexSocketClient.SubscribeAsync<BookSocketResponse>(OnReceiveBook, ToSymbol(Instrument.BTCUSD));
            await _bitmexSocketClient.SubscribeAsync<CandleSocketResponse>(OnReceiveCandle, ToSymbol(Instrument.BTCUSD));
        }

        public override async Task SubscribeToTradeChannel(Func<Task, TradeModel[]> onReceivedAsync)
        {
        }

        public override Task SubscribeToBookChannel(Action<BookModel[]> onRecieve)
        {
            throw new NotImplementedException();
        }

        public override Task SubscribeToCandleChannel(TimePeriod period, Action<CandleModel[]> onRecieve)
        {
            throw new NotImplementedException();
        }

        public override async Task GetCandlesAsync(Instrument instrument, TimePeriod timePeriod, DateTimeOffset startTime, DateTimeOffset endTime, Func<CandleModel[], Task> onRecieve)
        {
            var stopwatch = new Stopwatch();
            var total = 0;
            var count = 0;

            var request = new TradeCommandRequest
            {
                IsAuth = true,
                Path = ExchangeConfig.CandlePathRest,
                Symbol = ToSymbol(instrument),
                StartAt = 0,
                Count = 750,
                TimeInterval = ToTimePeriod(timePeriod),
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

                if (count < request.Count)
                    count = 0;

                if (stopwatch.Elapsed < TimeSpan.FromSeconds(1.01))
                    await Task.Delay(TimeSpan.FromSeconds(1).Subtract(stopwatch.Elapsed));

            } while (count > 0);
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

        private async Task OnReceiveCandle(CandleSocketResponse[] arg)
        {
            if (OnCandleReceived != null)
            {
                var models = arg.Select(Mapper.Map<CandleModel>).ToArray();

                await OnCandleReceived.Invoke(models);
            }
        }
    }
}
