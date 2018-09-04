using AutoMapper;
using Bognabot.Bitmex.Http.Responses;
using Bognabot.Bitmex.Socket.Responses;
using Bognabot.Config.Enums;
using Bognabot.Data.Models.Exchange;
using Bognabot.Domain.Entities.Instruments;

namespace Bognabot.Bitmex.Core
{
    public class BitmexProfile : Profile
    {
        public BitmexProfile()
        {
            CreateMap<TradeCommandResponse, CandleModel>()
                .ForMember(d => d.Exchange, o => o.MapFrom(s => SupportedExchange.Bitmex))
                .ForMember(d => d.Instrument, o => o.MapFrom(s => BitmexUtils.ToInstrumentType(s.Symbol)))
                .ForMember(d => d.Period, o => o.Ignore());

            CreateMap<TradeSocketResponse, TradeModel>()
                .ForMember(d => d.Instrument, o => o.MapFrom(s => BitmexUtils.ToInstrumentType(s.Symbol)))
                .ForMember(d => d.Side, o => o.MapFrom(s => BitmexUtils.ToTradeType(s.Side)));

            CreateMap<BookSocketResponse, BookModel>()
                .ForMember(d => d.Instrument, o => o.MapFrom(s => BitmexUtils.ToInstrumentType(s.Symbol)))
                .ForMember(d => d.Side, o => o.MapFrom(s => BitmexUtils.ToTradeType(s.Side)));
        }
    }
}
