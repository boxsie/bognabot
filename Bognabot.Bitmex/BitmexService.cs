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
using Bognabot.Data.Config;
using Bognabot.Data.Exchange;
using Bognabot.Data.Exchange.Enums;
using Bognabot.Data.Models.Exchange;
using Bognabot.Domain.Entities.Instruments;
using Bognabot.Services.Exchange;
using NLog;

namespace Bognabot.Bitmex
{
    public class BitmexService : BaseExchangeService
{
        public override DateTimeOffset Now => BitmexUtils.Now();

        public override event Func<TradeModel[], Task> OnTradeReceived;
        public override event Func<BookModel[], Task> OnBookReceived;

        private readonly BitmexSocketClient _bitmexSocketClient;
        private readonly BitmexHttpClient _bitmexHttpClient;

        public BitmexService(ILogger logger, ExchangeConfig config) : base(config)
        {
            _bitmexSocketClient = new BitmexSocketClient(logger, config);
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

        public override async Task SubscribeToStreams()
        {
            await _bitmexSocketClient.ConnectAsync();

            await _bitmexSocketClient.SubscribeAsync<TradeSocketResponse>(OnReceiveTrade, ToSymbol(Instrument.BTCUSD));
            await _bitmexSocketClient.SubscribeAsync<BookSocketResponse>(OnReceiveBook, ToSymbol(Instrument.BTCUSD));
        }

        public override async Task GetCandlesAsync(Instrument instrument, TimePeriod timePeriod, DateTimeOffset startTime, DateTimeOffset endTime, Func<CandleModel[], Task> onRecieve)
        {
            var stopwatch = new Stopwatch();
            var total = 0;
            var count = 0;

            var request = new TradeCommandRequest
            {
                IsAuth = true,
                Path = ExchangeConfig.CandlePath,
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
